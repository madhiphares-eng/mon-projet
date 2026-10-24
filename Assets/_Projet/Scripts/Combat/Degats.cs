using System.Collections.Generic;
using EchosEther.Jeu;
using UnityEngine;

namespace EchosEther.Combat
{
    /// <summary>Un coup porté : quantité, point d'impact, direction, recul et ralentissement éventuel.</summary>
    public struct Degats
    {
        public float quantite;
        public Vector3 point;
        public Vector3 direction;
        /// <summary>Vitesse de recul donnée à la cible (m/s).</summary>
        public float recul;
        /// <summary>Secondes de ralentissement (fusil Sablier, compétence de stase).</summary>
        public float ralenti;
        /// <summary>Coup porté par l'Arpenteur (expérience, repère de touche).</summary>
        public bool parJoueur;

        public Degats(float quantite, Vector3 point, Vector3 direction, float recul = 0f, bool parJoueur = true, float ralenti = 0f)
        {
            this.quantite = quantite; this.point = point; this.direction = direction;
            this.recul = recul; this.parJoueur = parJoueur; this.ralenti = ralenti;
        }
    }

    /// <summary>Tout ce qui peut encaisser des dégâts (créatures, Arpenteur).</summary>
    public interface ICible
    {
        bool Vivante { get; }
        Transform transform { get; }
        /// <summary>Point visé (centre du corps).</summary>
        Vector3 Centre { get; }
        void Subir(Degats d);
    }

    /// <summary>Registre des cibles hostiles et dégâts de zone.</summary>
    public static class Combat
    {
        /// <summary>Créatures vivantes (l'Arpenteur n'y est pas : il ne se blesse pas avec ses propres explosions).</summary>
        public static readonly List<ICible> Hostiles = new List<ICible>();

        /// <summary>Un coup de l'Arpenteur a touché (true si la cible est morte).</summary>
        public static event System.Action<bool> Touche;

        internal static void SignalerTouche(bool mort) => Touche?.Invoke(mort);

        /// <summary>Applique des dégâts à une cible et prévient l'interface si l'Arpenteur en est l'auteur.</summary>
        public static void Frapper(ICible cible, Degats d)
        {
            if (cible == null || !cible.Vivante) return;
            cible.Subir(d);
            if (d.parJoueur) SignalerTouche(!cible.Vivante);
        }

        /// <summary>Dégâts dégressifs autour d'un point (explosions, onde de choc, ver).</summary>
        public static int Zone(Vector3 centre, float rayon, float degats, float recul, bool parJoueur = true, float ralenti = 0f)
        {
            int n = 0;
            for (int i = Hostiles.Count - 1; i >= 0; i--)
            {
                if (i >= Hostiles.Count) continue;
                var c = Hostiles[i];
                if (c == null || !c.Vivante) continue;
                Vector3 ecart = c.Centre - centre;
                float d = ecart.magnitude;
                if (d > rayon) continue;
                float k = Mathf.Lerp(1f, 0.35f, d / Mathf.Max(0.01f, rayon));
                Vector3 dir = d > 0.01f ? ecart / d : Planete.Haut(centre);
                Frapper(c, new Degats(degats * k, c.Centre, dir, recul * k, parJoueur, ralenti));
                n++;
            }
            return n;
        }

        /// <summary>Cible touchée par un collider (créature ou Arpenteur), sinon null.</summary>
        public static ICible De(Collider c) => c != null ? c.GetComponentInParent<ICible>() : null;
    }
}
