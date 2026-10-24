using System.Collections.Generic;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Bulle temporelle (inspiration Hyperion) : à l'intérieur, le temps local ralentit (rapport &lt; 1)
    /// ou accélère (rapport &gt; 1) par rapport au vaisseau-mère. Une bulle peut "respirer" :
    /// dense pendant un moment, puis relâchée.
    /// </summary>
    public class BulleTemporelle : MonoBehaviour
    {
        public static readonly List<BulleTemporelle> Toutes = new List<BulleTemporelle>();

        [Tooltip("Rythme du temps local comparé à l'extérieur : 0,05 = vingt fois plus lent, 4 = quatre fois plus rapide.")]
        public float rapport = 0.05f;
        public float rayon = 15f;
        public bool respire = true;
        public float dureeDense = 26f;
        public float dureeRelachee = 14f;
        [ColorUsage(false, true)] public Color couleur = new Color(0.45f, 0.75f, 1.4f);
        [Tooltip("Distance à laquelle l'Arpenteur remarque la bulle sans scanner.")]
        public float distanceDecouverte = 55f;
        [Tooltip("Bulle créée par une compétence : sans repère, ignorée par le didacticiel et le monolithe.")]
        public bool ephemere;

        public bool Decouverte { get; set; }
        public float RayonActuel { get; private set; }
        public bool EstLente => rapport < 1f;

        float force = 1f, cycle;
        Material materiau;
        Transform coque;

        void OnEnable() => Toutes.Add(this);
        void OnDisable() => Toutes.Remove(this);

        void Start()
        {
            RayonActuel = rayon;
            materiau = Formes.Lueur(couleur, 3f, 0.03f, 1.4f);
            materiau.SetFloat("_VitesseBandes", EstLente ? 1.2f : 6f);
            coque = Formes.Primitive(PrimitiveType.Sphere, transform, materiau, "Coque").transform;
            coque.localPosition = Vector3.zero;
        }

        void Update()
        {
            if (respire)
            {
                cycle = (cycle + Time.deltaTime) % (dureeDense + dureeRelachee);
                bool dense = cycle < dureeDense;
                force = Mathf.Lerp(force, dense ? 1f : 0.15f, 1f - Mathf.Exp(-Time.deltaTime * 1.5f));
            }
            RayonActuel = Mathf.Lerp(4f, rayon, force);
            if (coque != null) coque.localScale = Vector3.one * RayonActuel * 2f;
            if (materiau != null) materiau.SetFloat("_Intensite", 0.35f + 0.65f * force);

            var jeu = GestionnaireJeu.Instance;
            if (!ephemere && jeu != null && Vector3.Distance(jeu.PositionObservateur, transform.position) < distanceDecouverte) Decouverte = true;
        }

        /// <summary>0 hors de la bulle, 1 au cœur (tient compte de la respiration).</summary>
        public float Presence(Vector3 position)
        {
            float d = Vector3.Distance(position, transform.position);
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(RayonActuel + 1.5f, RayonActuel - 1.5f, d)) * force;
        }

        /// <summary>Multiplicateur de dτ/dt dû aux bulles en un point (1 = aucun effet).</summary>
        public static float Multiplicateur(Vector3 position)
        {
            float m = 1f;
            foreach (var b in Toutes)
            {
                float p = b.Presence(position);
                if (p > 0f) m /= Mathf.Lerp(1f, 1f / Mathf.Max(0.001f, b.rapport), p);
            }
            return m;
        }

        /// <summary>Présence dans la bulle lente la plus dense (pour le monolithe et le didacticiel).</summary>
        public static float PresenceLente(Vector3 position)
        {
            float p = 0f;
            foreach (var b in Toutes) if (b.EstLente) p = Mathf.Max(p, b.Presence(position));
            return p;
        }

        /// <summary>Présence dans une vraie bulle lente du décor (sans les bulles de stase des compétences).</summary>
        public static float PresenceLenteNaturelle(Vector3 position)
        {
            float p = 0f;
            foreach (var b in Toutes) if (b.EstLente && !b.ephemere) p = Mathf.Max(p, b.Presence(position));
            return p;
        }

        /// <summary>Facteur d'écoulement du temps local pour les créatures et projectiles (1 = normal).</summary>
        public static float Ecoulement(Vector3 position)
        {
            float m = 1f;
            foreach (var b in Toutes)
            {
                if (!b.EstLente) continue;
                float p = b.Presence(position);
                if (p > 0f) m = Mathf.Min(m, Mathf.Lerp(1f, Mathf.Max(0.12f, b.rapport), p));
            }
            return m;
        }
    }
}
