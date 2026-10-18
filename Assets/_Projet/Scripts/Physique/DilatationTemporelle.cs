using UnityEngine;

namespace EchosEther.Physique
{
    /// <summary>
    /// Calcule le temps propre d'un observateur près d'un trou noir (Schwarzschild)
    /// et le compare au temps écoulé sur le vaisseau-mère, supposé loin.
    ///   dτ/dt = sqrt(1 - rs / r)
    /// L'échelle est "jouable" : rs est en mètres de scène, pas en mètres réels,
    /// et le facteur est borné pour éviter un temps qui s'arrête complètement.
    /// </summary>
    public class DilatationTemporelle : MonoBehaviour
    {
        [Tooltip("Centre du trou noir.")]
        public Transform trouNoir;

        [Tooltip("Rayon de Schwarzschild en unités de scène.")]
        public float rayonSchwarzschild = 1000f;

        [Tooltip("Facteur minimal dτ/dt (0.0001 ≈ 1 h en surface = 1,1 an en orbite).")]
        [Range(0.00001f, 1f)] public float facteurMin = 0.0001f;

        [Tooltip("Observateur dont on mesure le temps (le joueur). Par défaut : cet objet.")]
        public Transform observateur;

        /// <summary>dτ/dt actuel : 1 = pas de dilatation, proche de 0 = temps très ralenti.</summary>
        public float Facteur { get; private set; } = 1f;

        /// <summary>Secondes vécues par l'observateur depuis le début.</summary>
        public double TempsPropre { get; private set; }

        /// <summary>Secondes écoulées sur le vaisseau-mère pendant ce temps.</summary>
        public double TempsOrbital { get; private set; }

        void Awake()
        {
            if (observateur == null) observateur = transform;
        }

        void Update()
        {
            if (trouNoir == null) return;

            float r = Vector3.Distance(observateur.position, trouNoir.position);
            float rapport = 1f - rayonSchwarzschild / Mathf.Max(r, rayonSchwarzschild * 1.0001f);
            Facteur = Mathf.Max(Mathf.Sqrt(Mathf.Max(rapport, 0f)), facteurMin);

            TempsPropre += Time.deltaTime;
            TempsOrbital += Time.deltaTime / Facteur;
        }

        /// <summary>Texte lisible pour l'interface : "1 h ici = 3 j 4 h là-haut".</summary>
        public static string Formater(double secondes)
        {
            var t = System.TimeSpan.FromSeconds(secondes);
            if (t.TotalDays >= 365) return $"{t.TotalDays / 365.25:0.0} ans";
            if (t.TotalDays >= 1) return $"{t.Days} j {t.Hours} h";
            if (t.TotalHours >= 1) return $"{t.Hours} h {t.Minutes} min";
            return $"{t.Minutes} min {t.Seconds} s";
        }
    }
}
