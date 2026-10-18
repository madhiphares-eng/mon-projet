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

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 souris = Entrees.Regard();
                lacet += souris.x * sensibilite;
                tangage = Mathf.Clamp(tangage - souris.y * sensibilite, tangageMin, tangageMax);
            }

            // Le repère suit le "haut" local sans rotation parasite autour de lui.
            Vector3 haut = graviteCible != null ? graviteCible.Haut : Vector3.up;
            repere = Quaternion.FromToRotation(repere * Vector3.up, haut) * repere;

            Quaternion rotation = repere * Quaternion.Euler(tangage, lacet, 0f);
            Vector3 pivot = cible.position + haut * hauteur;
            transform.position = pivot - rotation * Vector3.forward * distance;
            transform.rotation = Quaternion.LookRotation(pivot - transform.position, haut);
        }

        static void Capturer(bool capturer)
        {
            Cursor.lockState = capturer ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !capturer;
        }
    }
}
