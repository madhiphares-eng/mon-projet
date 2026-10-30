using EchosEther.Combat;
using EchosEther.Jeu;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Le champ de force d'une cellule ou d'une porte : hexagones, balayage, cadre, ondes aux impacts.
    /// Il bloque le passage tant qu'il est allumé ; il s'éteint en clignotant (Couper) et se rallume (Allumer).
    /// Le plan va de -largeur/2 à +largeur/2 et de 0 à hauteur, dans le plan XY de l'objet.
    /// </summary>
    public class ChampDeForce : MonoBehaviour
    {
        public float largeur = 2f, hauteur = 2.6f;
        public Color couleur = new Color(0.35f, 0.85f, 1.2f);
        public float intensite = 1f;
        [Tooltip("Bloque le passage tant qu'il est allumé")] public bool bloquer = true;
        public bool Actif { get; private set; } = true;
        readonly Vector4[] impacts = new Vector4[8];
        int prochain;
        float temps, niveau = 1f;
        Material materiau;
        MeshRenderer rendu;
        BoxCollider mur;

        void Awake()
        {
            for (int i = 0; i < impacts.Length; i++) impacts[i] = new Vector4(0f, 0f, 0f, -1f);
            var go = new GameObject("Champ");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = EffetsColosse.Plan(largeur, hauteur);
            rendu = go.AddComponent<MeshRenderer>();
            rendu.shadowCastingMode = ShadowCastingMode.Off;
            materiau = EffetsColosse.Champ(couleur, largeur, hauteur);
            rendu.sharedMaterial = materiau;
            if (bloquer)
            {
                mur = gameObject.AddComponent<BoxCollider>();
                mur.center = new Vector3(0f, hauteur / 2f, 0f);
                mur.size = new Vector3(largeur, hauteur, 0.1f);
            }
        }

        /// <summary>Le champ est touché en ce point (un tir, un détenu qui le heurte) : une onde et des étincelles.</summary>
        public void Toucher(Vector3 pointMonde)
        {
            if (!Actif) return;
            var l = transform.InverseTransformPoint(pointMonde);
            impacts[prochain] = new Vector4(l.x, l.y, 0f, temps);
            prochain = (prochain + 1) % impacts.Length;
            EffetsColosse.Gerbe(pointMonde, -transform.forward, couleur, 8, 0.5f);
        }

        public void Couper() => Actif = false;
        public void Allumer() => Actif = true;

        void Update()
        {
            float dt = Time.deltaTime;
            temps += dt;
            niveau = Mathf.MoveTowards(niveau, Actif ? 1f : 0f, dt * (Actif ? 2.5f : 4f));
            // il s'éteint en clignotant, comme un générateur qui décroche
            float coupure = !Actif && niveau > 0.05f && Random.value < 0.3f ? 0f : 1f;
            materiau.SetFloat("_Temps", temps);
            materiau.SetVectorArray("_Impacts", impacts);
            materiau.SetFloat("_Intensite", intensite * niveau * coupure);
            rendu.enabled = niveau > 0.01f;
            if (mur != null) mur.enabled = Actif;
        }
    }
}
