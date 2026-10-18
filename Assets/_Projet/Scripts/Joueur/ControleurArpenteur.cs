using EchosEther.Physique;
using UnityEngine;

namespace EchosEther.Joueur
{
    /// <summary>
    /// Déplacement de l'Arpenteur sur une surface courbe : la marche se fait dans le plan
    /// perpendiculaire à la gravité locale, relativement à la caméra.
    /// </summary>
    [RequireComponent(typeof(CorpsGravitationnel))]
    public class ControleurArpenteur : MonoBehaviour
    {
        public float vitesseMarche = 6f;
        public float vitesseCourse = 12f;

        [Tooltip("Vitesse verticale donnée au saut, en m/s.")]
        public float impulsionSaut = 6f;

        [Tooltip("Caméra servant de référence pour avant/arrière. Par défaut : la caméra principale.")]
        public Transform cameraRef;

        [Tooltip("Distance entre le centre du personnage et ses pieds.")]
        public float demiHauteur = 1f;

        Rigidbody corps;
        CorpsGravitationnel gravite;
        bool sautDemande;

        void Awake()
        {
            corps = GetComponent<Rigidbody>();
            gravite = GetComponent<CorpsGravitationnel>();
            if (cameraRef == null && Camera.main != null) cameraRef = Camera.main.transform;
        }

        void Update()
        {
            if (Entrees.Saut()) sautDemande = true;
        }

        void FixedUpdate()
        {
            Vector3 haut = gravite.Haut;
            Vector2 entree = Entrees.Deplacement();

            Vector3 reference = cameraRef != null ? cameraRef.forward : transform.forward;
            Vector3 avant = Vector3.ProjectOnPlane(reference, haut).normalized;
            Vector3 droite = Vector3.Cross(haut, avant);

            float vitesse = Entrees.Course() ? vitesseCourse : vitesseMarche;
            Vector3 horizontale = (avant * entree.y + droite * entree.x) * vitesse;
            Vector3 verticale = Vector3.Project(corps.linearVelocity, haut);
            corps.linearVelocity = horizontale + verticale;

            if (sautDemande && AuSol(haut))
                corps.AddForce(haut * impulsionSaut, ForceMode.VelocityChange);
            sautDemande = false;
        }

        bool AuSol(Vector3 haut)
        {
            return Physics.Raycast(corps.position, -haut, demiHauteur + 0.15f, ~0, QueryTriggerInteraction.Ignore);
        }
    }
}
