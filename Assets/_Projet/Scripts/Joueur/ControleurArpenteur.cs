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
        Vector3 elan;

        /// <summary>
        /// Poussée extérieure (bond propulsé, recul d'un coup, ver) : la part horizontale s'ajoute à la marche
        /// et s'amortit, au lieu d'être effacée par le contrôle à l'image suivante.
        /// </summary>
        public void Propulser(Vector3 impulsion)
        {
            Vector3 haut = gravite != null ? gravite.Haut : Vector3.up;
            elan += Vector3.ProjectOnPlane(impulsion, haut);
            if (corps != null) corps.AddForce(Vector3.Project(impulsion, haut), ForceMode.VelocityChange);
        }

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
            bool auSol = AuSol(haut);
            elan = Vector3.ProjectOnPlane(elan, haut) * Mathf.Exp(-Time.fixedDeltaTime * (auSol ? 5f : 0.8f));
            corps.linearVelocity = horizontale + elan + verticale;

            if (sautDemande && auSol)
                corps.AddForce(haut * impulsionSaut, ForceMode.VelocityChange);
            sautDemande = false;
        }

        bool AuSol(Vector3 haut)
        {
            return Physics.Raycast(corps.position, -haut, demiHauteur + 0.15f, ~0, QueryTriggerInteraction.Ignore);
        }
    }
}
