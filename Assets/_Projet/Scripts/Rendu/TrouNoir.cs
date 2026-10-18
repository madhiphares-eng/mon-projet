using UnityEngine;

namespace EchosEther.Rendu
{
    /// <summary>
    /// Envoie au shader "FullScreen/LentilleGravitationnelle" la position et les réglages du trou noir.
    /// À poser sur l'objet qui représente le trou noir dans la scène.
    /// </summary>
    [ExecuteAlways]
    public class TrouNoir : MonoBehaviour
    {
        static readonly int IdPosition = Shader.PropertyToID("_TN_Position");
        static readonly int IdParams = Shader.PropertyToID("_TN_Params");
        static readonly int IdCouleur = Shader.PropertyToID("_TN_Couleur");

        [Tooltip("Rayon de Schwarzschild, en unités de scène. Doit correspondre à DilatationTemporelle.")]
        public float rayonSchwarzschild = 1000f;

        [Tooltip("Force de la déviation. 1 = anneau d'Einstein collé à l'ombre, 2-3 = déformation spectaculaire.")]
        [Range(0f, 5f)] public float forceDeviation = 1.8f;

        [Tooltip("Luminosité de l'anneau de photons (HDR, profite du bloom).")]
        [Range(0f, 50f)] public float intensiteAnneau = 8f;

        [ColorUsage(false, true)]
        public Color couleurAnneau = new Color(1f, 0.75f, 0.45f);

        /// <summary>L'ombre visible mesure environ 2,6 fois le rayon de Schwarzschild.</summary>
        public float RayonOmbre => rayonSchwarzschild * 2.6f;

        void Update()
        {
            Shader.SetGlobalVector(IdPosition, transform.position);
            Shader.SetGlobalVector(IdParams, new Vector4(RayonOmbre, forceDeviation, intensiteAnneau, 0f));
            Shader.SetGlobalColor(IdCouleur, couleurAnneau);
        }

        void OnDisable()
        {
            // Force nulle et rayon minuscule : la passe n'a plus d'effet visible.
            Shader.SetGlobalVector(IdParams, new Vector4(0.0001f, 0f, 0f, 0f));
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(transform.position, RayonOmbre);
            Gizmos.color = new Color(1f, 0.6f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, RayonOmbre * 1.03f);
        }
    }
}
