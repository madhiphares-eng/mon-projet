using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EchosEther.Interface;
using EchosEther.Jeu;
using EchosEther.Joueur;
using EchosEther.Physique;
using EchosEther.Rendu;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace EchosEther.Editeur
{
    /// <summary>
    /// Menu "Echos d'Ether > Vérifier l'installation" : passe en revue le projet et la scène ouverte
    /// (HDRP, paquets, ressources du jeu, modèles et sons téléchargés, composants installés) et copie
    /// le rapport dans le presse-papiers, pour le coller tel quel dans une conversation.
    /// </summary>
    public static class Diagnostic
    {
        const string Externes = "Assets/_Projet/Externes";

        enum Niveau { Ok, Attention, Erreur }
        static readonly List<(Niveau niveau, string texte)> lignes = new List<(Niveau, string)>();

        static void Ok(string t) => lignes.Add((Niveau.Ok, t));
        static void Attention(string t) => lignes.Add((Niveau.Attention, t));
        static void Erreur(string t) => lignes.Add((Niveau.Erreur, t));

        [MenuItem("Echos d'Ether/Vérifier l'installation", false, 30)]
        public static void Verifier()
        {
            lignes.Clear();
            Projet();
            Ressources();
            Telechargements();
            bool jeuInstalle = Scene();

            var rapport = new StringBuilder();
            rapport.AppendLine($"Échos d'Éther · diagnostic du {System.DateTime.Now:yyyy-MM-dd HH:mm} · Unity {Application.unityVersion}");
            foreach (var (niveau, texte) in lignes)
                rapport.AppendLine((niveau == Niveau.Ok ? "[OK] " : niveau == Niveau.Attention ? "[!]  " : "[X]  ") + texte);
            EditorGUIUtility.systemCopyBuffer = rapport.ToString();

            int erreurs = lignes.Count(l => l.niveau == Niveau.Erreur);
            int alertes = lignes.Count(l => l.niveau == Niveau.Attention);
            if (erreurs > 0) Debug.LogError(rapport.ToString());
            else if (alertes > 0) Debug.LogWarning(rapport.ToString());
            else Debug.Log(rapport.ToString());

            var message = new StringBuilder();
            message.AppendLine(erreurs == 0 && alertes == 0 ? "Tout est en place : appuie sur Play." : $"{erreurs} problème(s), {alertes} point(s) à surveiller.");
            foreach (var (_, texte) in lignes.Where(l => l.niveau != Niveau.Ok).OrderByDescending(l => l.niveau).Take(8))
                message.AppendLine("• " + texte);
            message.AppendLine();
            message.Append("Rapport complet dans la Console, et copié dans le presse-papiers (Ctrl+V pour le coller à Claude).");

            if (!jeuInstalle)
            {
                if (EditorUtility.DisplayDialog("Échos d'Éther · diagnostic", message.ToString(), "Installer le jeu maintenant", "Fermer"))
                    InstallateurJeu.Installer();
            }
            else EditorUtility.DisplayDialog("Échos d'Éther · diagnostic", message.ToString(), "OK");
        }

        // ------------------------------------------------------------------ projet

        static void Projet()
        {
            if (Application.unityVersion.StartsWith("6")) Ok($"Unity {Application.unityVersion}");
            else Attention($"Unity {Application.unityVersion} : le projet est prévu pour Unity 6 (6000.x)");

            var rp = GraphicsSettings.currentRenderPipeline;
            if (rp == null) Erreur("Aucun pipeline de rendu : le projet doit être créé avec le modèle High Definition 3D (HDRP)");
            else if (!rp.GetType().Name.Contains("HDRenderPipeline")) Erreur($"Pipeline {rp.GetType().Name} : HDRP attendu");
            else Ok($"HDRP actif ({rp.name})");

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            Ok("Entrées : Input System et ancien Input Manager");
#elif ENABLE_INPUT_SYSTEM
            Ok("Entrées : Input System");
#else
            Ok("Entrées : ancien Input Manager");
#endif

            string manifeste = File.Exists("Packages/manifest.json") ? File.ReadAllText("Packages/manifest.json") : "";
            if (manifeste.Contains("\"com.unity.cloud.gltfast\"")) Ok("glTFast présent (modèles .glb et .gltf)");
            else Attention("glTFast absent : les modèles .glb/.gltf ne s'importent pas (relancer telecharger-assets.ps1, ou Package Manager > Add package by name > com.unity.cloud.gltfast)");
        }

        // ------------------------------------------------------------------ ressources du jeu

        static void Ressources()
        {
            if (UnityEngine.Resources.Load<StyleSheet>("EchosEtherUI") != null) Ok("Styles de l'interface (Resources/EchosEtherUI.uss)");
            else Erreur("Resources/EchosEtherUI.uss introuvable : interface sans style (relancer installer.ps1)");
            if (UnityEngine.Resources.Load<ThemeStyleSheet>("EchosEtherTheme") != null) Ok("Thème de l'interface (Resources/EchosEtherTheme.tss)");
            else Attention("Resources/EchosEtherTheme.tss introuvable : textes et curseurs avec le thème par défaut");

            var lueur = Shader.Find("EchosEther/Lueur");
            if (lueur == null) Erreur("Shader EchosEther/Lueur introuvable : bulles, onde du scanner et faisceaux invisibles");
            else if (ShaderUtil.ShaderHasError(lueur)) Erreur("Le shader Lueur a des erreurs : sélectionner Assets/_Projet/Resources/Lueur.shader pour les lire dans l'Inspector");
            else Ok("Shader des lueurs compilé");
        }

        // ------------------------------------------------------------------ assets téléchargés

        static int Compter(string filtre, string dossier) =>
            AssetDatabase.IsValidFolder(dossier) ? AssetDatabase.FindAssets(filtre, new[] { dossier }).Length : 0;

        static void Telechargements()
        {
            if (!AssetDatabase.IsValidFolder(Externes))
            {
                Attention("Aucun asset téléchargé (dossier Externes absent) : le jeu marche avec des formes simples. Lancer telecharger-assets.ps1, Unity fermé");
                return;
            }
            int modeles = Compter("t:GameObject", Externes);
            int vaisseaux = Compter("t:GameObject", Externes + "/Sketchfab/Vaisseaux");
            int textures = Compter("t:Texture2D", Externes);
            int ciels = Compter("t:Cubemap", Externes);
            int sons = Compter("t:AudioClip", Externes + "/Sons");
            Ok($"Assets téléchargés : {modeles} modèles (dont {vaisseaux} vaisseaux), {textures} textures, {ciels} ciels, {sons} sons");
            if (modeles == 0) Attention("Aucun modèle importé : vérifier glTFast, puis clic droit sur Externes > Reimport");
            if (sons == 0) Attention("Aucun son dans Externes/Sons : le jeu sera muet, sauf l'ambiance générée");

            int malReglees = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Externes }))
            {
                string chemin = AssetDatabase.GUIDToAssetPath(guid);
                if (!ImportExternes.Normale.IsMatch(Path.GetFileNameWithoutExtension(chemin))) continue;
                if (AssetImporter.GetAtPath(chemin) is TextureImporter ti && ti.textureType != TextureImporterType.NormalMap) malReglees++;
            }
            if (malReglees > 0) Attention($"{malReglees} carte(s) de normales mal réglée(s) : menu Echos d'Ether > Régler les textures téléchargées");
        }

        // ------------------------------------------------------------------ scène ouverte

        static bool EstVaisseau(string nom)
        {
            nom = nom.ToLowerInvariant();
            return nom.Contains("dropship") || nom.Contains("drop-ship") || nom.Contains("navette") || nom.Contains("shuttle") || nom.Contains("spaceship");
        }

        static T Trouver<T>() where T : Object => Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
        static int Nombre<T>() where T : Object => Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

        /// <returns>true si le jeu est installé dans la scène.</returns>
        static bool Scene()
        {
            var scene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path)) Attention("Scène ouverte jamais enregistrée");
            else Ok($"Scène ouverte : {scene.path}");

            var planete = Object.FindObjectsByType<SourceGravite>(FindObjectsSortMode.None).OrderByDescending(s => s.rayonSurface).FirstOrDefault();
            if (planete != null) Ok($"Planète : rayon {planete.rayonSurface:0} m");
            else Erreur("Pas de planète (SourceGravite) : menu Echos d'Ether > Créer la scène Lisière de l'Horizon");

            var joueur = Trouver<ControleurArpenteur>();
            if (joueur == null) Erreur("Pas d'Arpenteur (ControleurArpenteur) dans la scène");
            else if (joueur.GetComponent<Rigidbody>() == null || joueur.GetComponent<CorpsGravitationnel>() == null)
                Erreur("L'Arpenteur n'a pas de Rigidbody ou de CorpsGravitationnel");
            else Ok("Arpenteur prêt (corps, gravité)");

            var camera = Trouver<CameraOrbitale>();
            if (camera == null) Erreur("Pas de caméra CameraOrbitale");
            else if (!camera.CompareTag("MainCamera")) Attention("La caméra du joueur n'a pas le tag MainCamera");
            else Ok("Caméra du joueur");

            int ecouteurs = Nombre<AudioListener>();
            if (ecouteurs == 0) Erreur("Aucun AudioListener : aucun son ne sera entendu (en ajouter un sur la caméra)");
            else if (ecouteurs > 1) Attention($"{ecouteurs} AudioListener dans la scène : n'en garder qu'un, sur la caméra");

            if (Trouver<TrouNoir>() == null) Attention("Pas de trou noir (composant TrouNoir) : le menu regarde un ciel vide");

            var jeu = Trouver<GestionnaireJeu>();
            if (jeu == null)
            {
                Erreur("Le jeu n'est pas installé dans cette scène : menu Echos d'Ether > Installer le jeu dans la scène");
                return false;
            }
            var manquants = new List<string>();
            if (jeu.interfaceJeu == null) manquants.Add("interface");
            if (jeu.horloges == null) manquants.Add("horloges");
            if (jeu.transmissions == null) manquants.Add("transmissions");
            if (jeu.mission == null) manquants.Add("mission");
            if (jeu.didacticiel == null) manquants.Add("didacticiel");
            if (jeu.navette == null) manquants.Add("navette");
            if (jeu.sequenceLargage == null) manquants.Add("arrivée en navette");
            if (Trouver<AmbianceSonore>() == null) manquants.Add("ambiance sonore");
            if (Trouver<Ruche>() == null) manquants.Add("Ruche");
            if (manquants.Count > 0) Attention("Jeu installé mais incomplet (" + string.Join(", ", manquants) + ") : relancer Installer le jeu dans la scène");
            else Ok("Jeu installé : menus, enrôlement, arrivée, didacticiel, mission, ambiance");

            int cristaux = Nombre<CristalEther>(), bulles = Nombre<BulleTemporelle>();
            bool balise = Trouver<BaliseExtraction>() != null, monolithe = Trouver<Monolithe>() != null;
            if (cristaux >= 5 && bulles > 0 && balise && monolithe) Ok($"Lieux : {cristaux} cristaux, {bulles} bulle(s), monolithe, balise");
            else Attention($"Lieux incomplets : {cristaux} cristaux (5 requis), {bulles} bulle(s), monolithe {(monolithe ? "oui" : "non")}, balise {(balise ? "oui" : "non")}");

            if (jeu.navette != null)
            {
                if (jeu.navette.transform.Find("Modele") != null) Ok("Navette : modèle téléchargé");
                else Attention("Navette : silhouette simple (aucun modèle dans Externes/Sketchfab/Vaisseaux)");
                // un vaisseau posé à la main comme décor (par exemple par Claude en local) fait doublon avec celle du jeu
                var decors = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                    .Where(t => EstVaisseau(t.name) && !t.IsChildOf(jeu.navette.transform) && (t.parent == null || !EstVaisseau(t.parent.name)))
                    .Select(t => t.name).Distinct().Take(3).ToList();
                if (decors.Count > 0)
                    Attention("Vaisseau de décor en plus de la navette du jeu (" + string.Join(", ", decors) + ") : le supprimer, ou l'éloigner pour éviter deux navettes au même endroit");
            }
            var ruche = Trouver<Ruche>();
            if (ruche != null)
            {
                if (ruche.modeleVer != null) Ok("Ver des sables : modèle téléchargé");
                else Attention("Ver des sables : forme procédurale (aucun modèle dans Externes/Sketchfab/Personnages)");
            }

            var banque = Trouver<BanqueSons>();
            if (banque != null)
            {
                var champs = typeof(BanqueSons).GetFields().Where(f => f.FieldType == typeof(AudioClip)).ToArray();
                int branches = champs.Count(f => f.GetValue(banque) as AudioClip != null);
                int pas = banque.pas != null ? banque.pas.Count(c => c != null) : 0;
                if (branches == 0) Attention("Aucun son branché : télécharger les sons Kenney, puis relancer Installer le jeu");
                else Ok($"Sons branchés : {branches}/{champs.Length}, {pas} bruit(s) de pas");
            }

            if (!string.IsNullOrEmpty(scene.path))
            {
                var entree = EditorBuildSettings.scenes.FirstOrDefault(s => s.path == scene.path);
                if (entree == null) Attention("Scène absente des Build Settings : relancer Installer le jeu (nécessaire pour un exécutable)");
                else if (!entree.enabled) Attention("Scène décochée dans les Build Settings");
                else Ok("Scène dans les Build Settings");
            }
            return true;
        }
    }
}
