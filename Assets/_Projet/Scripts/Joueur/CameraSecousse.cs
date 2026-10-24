using UnityEngine;

namespace EchosEther.Joueur
{
    /// <summary>
    /// Secoue la caméra après son placement (tremblement de la Ruche, impacts, atterrissages).
    /// S'exécute après <see cref="CameraOrbitale"/> et <see cref="CameraNavette"/>.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class CameraSecousse : MonoBehaviour
    {
        public static CameraSecousse Instance { get; private set; }

        [Tooltip("Amplitude maximale du déplacement, en mètres.")]
        public float amplitude = 0.35f;

        /// <summary>Secousse continue (0 à 1), par exemple pendant un tremblement.</summary>
        public float Continue { get; set; }
        float choc;

        void OnEnable() => Instance = this;
        void OnDisable() { if (Instance == this) Instance = null; }

        /// <summary>Ajoute un choc ponctuel (0 à 1) qui s'amortit en une seconde environ.</summary>
        public static void Choc(float force)
        {
            if (Instance != null) Instance.choc = Mathf.Max(Instance.choc, force);
        }

        void LateUpdate()
        {
            choc = Mathf.Max(0f, choc - Time.deltaTime * 1.2f);
            float force = Mathf.Max(Continue, choc);
            if (force <= 0.001f || Time.timeScale == 0f) return;
            transform.position += Random.insideUnitSphere * (amplitude * force);
        }
    }
}
