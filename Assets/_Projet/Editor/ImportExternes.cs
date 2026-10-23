using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace EchosEther.Editeur
{
    /// <summary>
    /// Règle l'import des textures téléchargées par outils/telecharger-assets.ps1 (Assets/_Projet/Externes) :
    /// normal maps, cartes en espace linéaire (rugosité, AO, hauteur, métal) et ciels en cubemap.
    /// </summary>
    public class ImportExternes : AssetPostprocessor
    {
        const string Dossier = "Assets/_Projet/Externes";

        // Poly Haven : roche_nor_gl_2k.jpg ; ambientCG : Rock029_2K-JPG_NormalGL.jpg
        static readonly Regex Normale = new Regex(@"(_nor_gl_\d+k|_NormalGL)$", RegexOptions.IgnoreCase);
        static readonly Regex Lineaire = new Regex(
            @"(_(rough|ao|disp|metal|arm)_\d+k|_(Roughness|AmbientOcclusion|Displacement|Metalness|Opacity))$",
            RegexOptions.IgnoreCase);
        static readonly string[] ExtensionsImages = { ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".exr", ".hdr" };

        public override uint GetVersion() => 1;

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Dossier + "/", StringComparison.Ordinal)) return;

            var importeur = (TextureImporter)assetImporter;
            string nom = Path.GetFileNameWithoutExtension(assetPath);

            // Ciels équirectangulaires (HDRI, cartes d'étoiles) : cubemap pour HDRI Sky
            if (assetPath.Contains("/HDRI/") || assetPath.Contains("/Ciel/Etoiles/"))
            {
                importeur.textureType = TextureImporterType.Default;
                importeur.textureShape = TextureImporterShape.TextureCube;
                importeur.generateCubemap = TextureImporterGenerateCubemap.Cylindrical;
                importeur.maxTextureSize = 8192;
                importeur.textureCompression = TextureImporterCompression.CompressedHQ;
                return;
            }

            if (Normale.IsMatch(nom))
            {
                importeur.textureType = TextureImporterType.NormalMap;
                return;
            }

            // Données (pas des couleurs) : à lire sans correction gamma
            if (Lineaire.IsMatch(nom) || assetPath.Contains("/Ciel/Lune/ldem"))
                importeur.sRGBTexture = false;

            // Textures de planètes et relief lunaire en 8K
            if (assetPath.Contains("/Ciel/Planetes/") || assetPath.Contains("/Ciel/Lune/"))
                importeur.maxTextureSize = 8192;
        }

        [MenuItem("Echos d'Ether/Régler les textures téléchargées")]
        static void Reimporter()
        {
            var chemins = new System.Collections.Generic.List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture", new[] { Dossier }))
            {
                string chemin = AssetDatabase.GUIDToAssetPath(guid);
                if (Array.IndexOf(ExtensionsImages, Path.GetExtension(chemin).ToLowerInvariant()) >= 0)
                    chemins.Add(chemin);
            }

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < chemins.Count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Échos d'Éther", chemins[i], (float)i / chemins.Count)) break;
                    AssetDatabase.ImportAsset(chemins[i], ImportAssetOptions.ForceUpdate);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }
            Debug.Log($"Textures téléchargées réglées : {chemins.Count} fichiers.");
        }
    }
}
