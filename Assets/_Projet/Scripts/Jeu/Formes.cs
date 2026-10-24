using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace EchosEther.Jeu
{
    /// <summary>Matériaux HDRP et formes simples créés par le code (cristaux, bulles, ondes, faisceaux).</summary>
    public static class Formes
    {
        static Shader shaderLit, shaderLueur;
        static Mesh eclat;

        static Shader Lit => shaderLit != null ? shaderLit : shaderLit = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");

        /// <summary>Matériau opaque HDRP/Lit.</summary>
        public static Material Opaque(Color couleur, float metal = 0f, float lisse = 0.4f)
        {
            var m = new Material(Lit);
            m.SetColor("_BaseColor", couleur);
            m.SetColor("_Color", couleur);
            m.SetFloat("_Metallic", metal);
            m.SetFloat("_Smoothness", lisse);
            return m;
        }

        /// <summary>
        /// Matériau qui brille. Avec un poids d'exposition nul, l'émission ne dépend pas de l'exposition
        /// de la caméra : 1 = blanc, 2 à 4 = lueur avec bloom.
        /// </summary>
        public static Material Emissif(Color couleur, Color emission, float lisse = 0.85f)
        {
            var m = Opaque(couleur, 0f, lisse);
            DefinirEmission(m, emission);
            return m;
        }

        public static void DefinirEmission(Material m, Color emission)
        {
            if (m.HasProperty("_EmissiveExposureWeight")) m.SetFloat("_EmissiveExposureWeight", 0f);
            if (m.HasProperty("_EmissiveColor")) m.SetColor("_EmissiveColor", emission);
            else if (m.HasProperty("_EmissionColor")) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emission); }
        }

        /// <summary>Lueur additive (shader EchosEther/Lueur) : bulles, onde du scanner, faisceaux.</summary>
        public static Material Lueur(Color couleur, float fresnel = 3f, float baseVisible = 0.05f, float bandes = 0f)
        {
            if (shaderLueur == null) shaderLueur = Shader.Find("EchosEther/Lueur");
            if (shaderLueur == null)
            {
                Debug.LogWarning("Shader EchosEther/Lueur introuvable : lueur remplacée par un matériau émissif.");
                return Emissif(Color.black, couleur);
            }
            var m = new Material(shaderLueur);
            m.SetColor("_Couleur", couleur);
            m.SetFloat("_Fresnel", fresnel);
            m.SetFloat("_Base", baseVisible);
            m.SetFloat("_Bandes", bandes);
            m.SetFloat("_Intensite", 1f);
            return m;
        }

        /// <summary>
        /// Lumière ponctuelle HDRP (avec ses données HDRP, sinon elle risque de ne pas s'afficher).
        /// Intensité en lumens.
        /// </summary>
        public static Light Lumiere(Transform parent, string nom, Color couleur, float lumens, float portee)
        {
            var go = new GameObject(nom);
            go.transform.SetParent(parent, false);
            go.AddComponent<HDAdditionalLightData>();          // ajoute aussi le composant Light
            var l = go.GetComponent<Light>();
            l.type = LightType.Point;
            l.color = couleur;
            l.intensity = lumens;
            l.range = portee;
            l.shadows = LightShadows.None;
            return l;
        }

        /// <summary>Primitive Unity sans collision, rangée sous <paramref name="parent"/>.</summary>
        public static GameObject Primitive(PrimitiveType type, Transform parent, Material materiau, string nom)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = nom;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshRenderer>().sharedMaterial = materiau;
            return go;
        }

        /// <summary>Éclat de cristal : prisme hexagonal terminé en pointe, hauteur 1, rayon 1.</summary>
        public static Mesh Eclat
        {
            get
            {
                if (eclat != null) return eclat;
                const int cotes = 6;
                var sommets = new System.Collections.Generic.List<Vector3>();
                var triangles = new System.Collections.Generic.List<int>();
                void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
                {
                    int i = sommets.Count;
                    sommets.Add(a); sommets.Add(b); sommets.Add(c); sommets.Add(d);
                    triangles.AddRange(new[] { i, i + 2, i + 1, i, i + 3, i + 2 });
                }
                void Tri(Vector3 a, Vector3 b, Vector3 c)
                {
                    int i = sommets.Count;
                    sommets.Add(a); sommets.Add(b); sommets.Add(c);
                    triangles.AddRange(new[] { i, i + 2, i + 1 });
                }
                Vector3 pointe = new Vector3(0f, 1.45f, 0f);
                for (int k = 0; k < cotes; k++)
                {
                    float a0 = k * Mathf.PI * 2f / cotes, a1 = (k + 1) * Mathf.PI * 2f / cotes;
                    Vector3 b0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)), b1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                    Vector3 h0 = b0 * 0.85f + Vector3.up, h1 = b1 * 0.85f + Vector3.up;
                    Face(b0, h0, h1, b1);          // flanc
                    Tri(h0, pointe, h1);           // pointe
                    Tri(b1, Vector3.zero, b0);     // dessous
                }
                eclat = new Mesh { name = "EclatEther" };
                eclat.SetVertices(sommets);
                eclat.SetTriangles(triangles, 0);
                eclat.RecalculateNormals();
                eclat.RecalculateBounds();
                return eclat;
            }
        }
    }
}
