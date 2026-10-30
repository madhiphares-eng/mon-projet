using System;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Prologue
{
    /// <summary>
    /// La caméra à la 3e personne du prologue (à l'intérieur du Colosse, gravité normale).
    /// Un bras de caméra part de l'épaule du héros et se raccourcit devant les murs (il rentre vite, ressort doucement).
    /// Quatre réglages selon l'état (normal, visée, accroupi, course) : distance, décalage d'épaule, hauteur, champ de vision.
    /// X (manette : croix droite) passe d'une épaule à l'autre. Le contrôleur du héros écrit Visee, Accroupi et Course.
    /// Même comportement que l'essai du navigateur (vitrine/vues.html).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraTroisiemePersonne : MonoBehaviour
    {
        [Serializable]
        public struct Reglage
        {
            public float distance, decalage, hauteur, champ;
            public Reglage(float distance, float decalage, float hauteur, float champ)
            {
                this.distance = distance; this.decalage = decalage; this.hauteur = hauteur; this.champ = champ;
            }
        }

        public Transform cible;
        public Reglage normal = new Reglage(3.3f, 0.5f, 1.62f, 62f);
        public Reglage visee = new Reglage(1.45f, 0.62f, 1.58f, 48f);
        public Reglage accroupi = new Reglage(2.8f, 0.55f, 1.05f, 60f);
        public Reglage course = new Reglage(3.7f, 0.45f, 1.6f, 70f);
        public float sensibilite = 2f;
        public float tangageMin = -50f, tangageMax = 65f;
        [Tooltip("Ce que la caméra ne traverse pas (retirer la couche du héros)")] public LayerMask murs = ~0;
        [Tooltip("Rayon de la sphère qui sonde les murs")] public float rayonCollision = 0.2f;

        public bool Visee { get; set; }
        public bool Accroupi { get; set; }
        public bool Course { get; set; }
        /// <summary>1 : par-dessus l'épaule droite ; -1 : la gauche.</summary>
        public int Epaule { get; private set; } = 1;
        /// <summary>Le lacet de la caméra (degrés) : le héros s'y tourne quand il vise.</summary>
        public float Lacet => lacet;
        /// <summary>L'avant de la caméra, à plat : la marche se fait par rapport à lui.</summary>
        public Vector3 AvantAPlat => Quaternion.Euler(0f, lacet, 0f) * Vector3.forward;

        static float recul;
        /// <summary>Le recul d'un tir : le regard se relève un peu (degrés), puis redescend.</summary>
        public static void Recul(float degres) => recul += degres;

        float lacet, tangage = 8f, distance, decalage, hauteur, libre = -1f;
        Camera cam;
        readonly RaycastHit[] touches = new RaycastHit[8];

        void Start()
        {
            cam = GetComponent<Camera>();
            distance = normal.distance; decalage = normal.decalage; hauteur = normal.hauteur;
            if (cible != null) lacet = cible.eulerAngles.y;
            Capturer(true);
        }

        void LateUpdate()
        {
            if (cible == null) return;
            if (Entrees.LibererCurseur()) Capturer(false);
            else if (Entrees.CapturerCurseur()) Capturer(true);
            if (EntreesJeu.ChangerEpaule()) Epaule = -Epaule;

            float dt = Time.deltaTime;
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 souris = Entrees.Regard();
                float s = sensibilite * ReglagesCamera.Sensibilite * (Visee ? 0.6f : 1f);
                lacet += souris.x * s;
                tangage = Mathf.Clamp(tangage - souris.y * s * (ReglagesCamera.InverserY ? -1f : 1f), tangageMin, tangageMax);
            }
            tangage -= recul;
            recul = 0f;

            var r = Visee ? visee : Accroupi ? accroupi : Course ? course : normal;
            distance = Amortir(distance, r.distance, 7f, dt);
            decalage = Amortir(decalage, r.decalage * Epaule, 8f, dt);
            hauteur = Amortir(hauteur, r.hauteur, 6f, dt);
            cam.fieldOfView = Amortir(cam.fieldOfView, r.champ, 6f, dt);

            Quaternion rotation = Quaternion.Euler(tangage, lacet, 0f);
            Vector3 pivot = cible.position + Vector3.up * hauteur + Quaternion.Euler(0f, lacet, 0f) * Vector3.right * decalage;
            Vector3 voulu = pivot - rotation * Vector3.forward * distance + Vector3.up * 0.12f;
            Vector3 versCam = voulu - pivot;
            float longueur = versCam.magnitude;
            versCam /= Mathf.Max(longueur, 0.0001f);

            // le bras se raccourcit devant un mur (on ignore le héros lui-même)
            float place = longueur;
            int n = Physics.SphereCastNonAlloc(pivot, rayonCollision, versCam, touches, longueur, murs, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                if (touches[i].transform.IsChildOf(cible)) continue;
                place = Mathf.Min(place, Mathf.Max(0.25f, touches[i].distance - 0.05f));
            }
            libre = libre < 0f || place < libre ? place : Amortir(libre, place, 4f, dt);

            transform.position = pivot + versCam * libre;
            transform.rotation = Quaternion.LookRotation(pivot + rotation * Vector3.forward * 20f - transform.position, Vector3.up);
        }

        /// <summary>Ce que vise le centre de l'écran (pour le tir) : le point touché, ou un point au loin.</summary>
        public Vector3 PointVise(float portee, LayerMask masque, out RaycastHit touche)
        {
            var rayon = new Ray(transform.position, transform.forward);
            int n = Physics.RaycastNonAlloc(rayon.origin, rayon.direction, touches, portee, masque, QueryTriggerInteraction.Ignore);
            touche = default;
            float meilleur = portee;
            bool trouve = false;
            for (int i = 0; i < n; i++)
            {
                if (cible != null && touches[i].transform.IsChildOf(cible)) continue;
                if (touches[i].distance < meilleur) { meilleur = touches[i].distance; touche = touches[i]; trouve = true; }
            }
            return trouve ? touche.point : rayon.origin + rayon.direction * portee;
        }

        static float Amortir(float a, float b, float vitesse, float dt) => Mathf.Lerp(a, b, 1f - Mathf.Exp(-vitesse * dt));

        public static void Capturer(bool capturer)
        {
            Cursor.lockState = capturer ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !capturer;
        }
    }
}
