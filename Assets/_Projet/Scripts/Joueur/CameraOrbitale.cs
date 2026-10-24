using EchosEther.Physique;
using UnityEngine;

namespace EchosEther.Joueur
{
    /// <summary>
    /// Caméra à la 3e personne qui tourne autour de la cible à la souris
    /// et garde son "haut" aligné sur la gravité locale (marche autour d'une planète).
    /// Échap libère la souris, clic gauche la recapture.
    /// </summary>
    public class CameraOrbitale : MonoBehaviour
    {
        public Transform cible;
        public float distance = 6f;
        public float hauteur = 1.6f;
        public float sensibilite = 2f;
        public float tangageMin = -40f;
        public float tangageMax = 75f;

        [Header("Visée (clic droit)")]
        public float distanceVisee = 2.3f;
        [Tooltip("Décalage vers la droite en visée : on vise par-dessus l'épaule.")]
        public float epauleVisee = 0.7f;
        [Tooltip("Réduction du champ de vision en visée (degrés).")]
        public float champVisee = 18f;

        /// <summary>Réglages du joueur (menu Options) : multiplicateur de sensibilité et axe vertical inversé.</summary>
        public static float MultiplicateurSensibilite = 1f;
        public static bool InverserY;
        /// <summary>0 = caméra normale, 1 = visée par-dessus l'épaule (écrit par l'arsenal).</summary>
        public static float ViseeCible;
        static float reculVertical, reculHorizontal;

        /// <summary>Recul d'un tir : relève le regard (degrés) et le décale un peu sur le côté.</summary>
        public static void Recul(float vertical, float horizontal)
        {
            reculVertical += vertical;
            reculHorizontal += horizontal;
        }

        float visee, champBase = -1f;
        Camera cam;

        CorpsGravitationnel graviteCible;
        Quaternion repere = Quaternion.identity;
        float lacet;
        float tangage = 15f;

        void Start()
        {
            if (cible != null) graviteCible = cible.GetComponent<CorpsGravitationnel>();
            Capturer(true);
        }

        void LateUpdate()
        {
            if (cible == null) return;

            if (Entrees.LibererCurseur()) Capturer(false);
            else if (Entrees.CapturerCurseur()) Capturer(true);

            visee = Mathf.MoveTowards(visee, ViseeCible, Time.deltaTime * 6f);
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 souris = Entrees.Regard();
                float s = sensibilite * MultiplicateurSensibilite * Mathf.Lerp(1f, 0.6f, visee);
                lacet += souris.x * s + reculHorizontal;
                tangage = Mathf.Clamp(tangage - souris.y * s * (InverserY ? -1f : 1f) - reculVertical, tangageMin, tangageMax);
            }
            reculVertical = reculHorizontal = 0f;

            // Le repère suit le "haut" local sans rotation parasite autour de lui.
            Vector3 haut = graviteCible != null ? graviteCible.Haut : Vector3.up;
            repere = Quaternion.FromToRotation(repere * Vector3.up, haut) * repere;

            Quaternion rotation = repere * Quaternion.Euler(tangage, lacet, 0f);
            float doux = visee * visee * (3f - 2f * visee);
            Vector3 pivot = cible.position + haut * hauteur + rotation * Vector3.right * (epauleVisee * doux);
            transform.position = pivot - rotation * Vector3.forward * Mathf.Lerp(distance, distanceVisee, doux);
            transform.rotation = Quaternion.LookRotation(pivot - transform.position, haut);
            if (cam == null) cam = GetComponent<Camera>();
            if (cam != null)
            {
                if (champBase < 0f) champBase = cam.fieldOfView;
                cam.fieldOfView = champBase - champVisee * doux;
            }
        }

        static void Capturer(bool capturer)
        {
            Cursor.lockState = capturer ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !capturer;
        }
    }
}
