using EchosEther.Physique;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Temps vécu par l'Arpenteur et temps écoulé sur le vaisseau-mère, avec la dilatation de la planète
    /// et celle des bulles temporelles. Remplace l'affichage provisoire <c>AffichageTemps</c>.
    /// </summary>
    public class Horloges : MonoBehaviour
    {
        public enum Source { Jeu, Physique }

        [Tooltip("Jeu : facteur fixe (1 h ici = 3 jours là-haut). Physique : DilatationTemporelle (Schwarzschild).")]
        public Source source = Source.Jeu;

        public DilatationTemporelle dilatation;

        [Tooltip("dτ/dt à la surface en mode Jeu. 1/72 : une heure ici = trois jours sur le vaisseau.")]
        public float facteurPlanete = 1f / 72f;

        [Tooltip("Accélération du temps vécu : 8 = une seconde réelle vaut huit secondes pour l'Arpenteur.")]
        public float echelleTemps = 8f;

        public static Horloges Instance { get; private set; }
        public double TempsVecu { get; private set; }
        public double TempsVaisseau { get; private set; }
        /// <summary>dτ/dt actuel (bulles comprises).</summary>
        public float Facteur { get; private set; } = 1f;
        public bool EnMarche { get; set; }

        void OnEnable() => Instance = this;
        void OnDisable() { if (Instance == this) Instance = null; }

        public void Reinitialiser()
        {
            TempsVecu = 0;
            TempsVaisseau = 0;
        }

        void Update()
        {
            var jeu = GestionnaireJeu.Instance;
            Vector3 position = jeu != null ? jeu.PositionObservateur : transform.position;
            float f = source == Source.Physique && dilatation != null ? dilatation.Facteur : facteurPlanete;
            Facteur = Mathf.Max(1e-7f, f * BulleTemporelle.Multiplicateur(position));
            if (!EnMarche) return;
            double dt = Time.deltaTime * echelleTemps;
            TempsVecu += dt;
            TempsVaisseau += dt / Facteur;
        }

        /// <summary>00:12:45</summary>
        public static string FormatHorloge(double s)
        {
            long t = (long)s;
            return $"{t / 3600:00}:{t % 3600 / 60:00}:{t % 60:00}";
        }

        /// <summary>J+3 · 07 h 12</summary>
        public static string FormatVaisseau(double s)
        {
            long t = (long)s;
            return $"J+{t / 86400} · {t % 86400 / 3600:00} h {t % 3600 / 60:00}";
        }

        /// <summary>Durée lisible : « 3 j 4 h », « 2,5 ans », « 12 min ».</summary>
        public static string FormatDuree(double s, bool court = false)
        {
            long t = (long)s;
            long j = t / 86400, h = t % 86400 / 3600, m = t % 3600 / 60;
            if (j >= 365) return $"{j / 365.25:0.0} ans".Replace('.', ',');
            if (j >= 1) return court ? $"{j} j {h} h" : $"{j} j {h:00} h";
            if (h >= 1) return $"{h} h {m:00}";
            return $"{m} min";
        }
    }
}
