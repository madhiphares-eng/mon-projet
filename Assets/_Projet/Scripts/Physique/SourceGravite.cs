using System.Collections.Generic;
using UnityEngine;

namespace EchosEther.Physique
{
    /// <summary>
    /// Attire les CorpsGravitationnels vers son centre (planète, anomalie, trou noir).
    /// La gravité globale d'Unity doit être mise à zéro (voir CorpsGravitationnel).
    /// </summary>
    public class SourceGravite : MonoBehaviour
    {
        public static readonly List<SourceGravite> Toutes = new List<SourceGravite>();

        [Tooltip("Accélération en surface, en m/s². Terre = 9.81.")]
        public float graviteSurface = 9.81f;

        [Tooltip("Rayon de la surface, en mètres. L'accélération suit 1/r² au-delà.")]
        public float rayonSurface = 500f;

        [Tooltip("Au-delà de cette distance, la source n'a plus d'effet.")]
        public float porteeMax = 5000f;

        void OnEnable() => Toutes.Add(this);
        void OnDisable() => Toutes.Remove(this);

        /// <summary>Accélération gravitationnelle subie à une position donnée.</summary>
        public Vector3 Acceleration(Vector3 position)
        {
            Vector3 versCentre = transform.position - position;
            float distance = versCentre.magnitude;
            if (distance > porteeMax || distance < 0.001f) return Vector3.zero;

            // Constante à l'intérieur de la planète, en 1/r² à l'extérieur.
            float r = Mathf.Max(distance, rayonSurface);
            float intensite = graviteSurface * (rayonSurface * rayonSurface) / (r * r);
            return versCentre / distance * intensite;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, rayonSurface);
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, porteeMax);
        }
    }
}
