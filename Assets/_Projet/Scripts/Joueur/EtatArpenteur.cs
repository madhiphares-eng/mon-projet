using System;
using EchosEther.Physique;
using UnityEngine;

namespace EchosEther.Joueur
{
    /// <summary>
    /// Observe l'Arpenteur (sans modifier <see cref="ControleurArpenteur"/>) : marche, course, silence,
    /// sauts et atterrissages. Sert au bruit, au didacticiel et aux sons de pas.
    /// La marche silencieuse (C) ralentit le contrôleur pendant que la touche est tenue.
    /// </summary>
    [RequireComponent(typeof(ControleurArpenteur))]
    public class EtatArpenteur : MonoBehaviour
    {
        [Tooltip("Vitesse pendant la marche silencieuse (touche C), en m/s.")]
        public float vitesseSilence = 3f;

        [Tooltip("Vitesse de chute minimale (m/s) pour qu'un atterrissage compte.")]
        public float chuteMinimale = 2f;

        public bool Actif => controleur != null && controleur.enabled;
        public bool Bouge { get; private set; }
        public bool Course { get; private set; }
        public bool Silence { get; private set; }
        public bool AuSol { get; private set; } = true;
        /// <summary>Vitesse horizontale (m/s).</summary>
        public float Vitesse { get; private set; }
        public Vector3 Haut => gravite != null ? gravite.Haut : Vector3.up;

        public event Action Saut;
        /// <summary>Vitesse de chute à l'impact (m/s).</summary>
        public event Action<float> Atterrissage;

        ControleurArpenteur controleur;
        CorpsGravitationnel gravite;
        Rigidbody corps;
        float marcheBase, courseBase, vitesseVerticalePrecedente;
        bool etaitAuSol = true;

        void Awake()
        {
            controleur = GetComponent<ControleurArpenteur>();
            gravite = GetComponent<CorpsGravitationnel>();
            corps = GetComponent<Rigidbody>();
            marcheBase = controleur.vitesseMarche;
            courseBase = controleur.vitesseCourse;
        }

        void OnDisable()
        {
            if (controleur == null) return;
            controleur.vitesseMarche = marcheBase;
            controleur.vitesseCourse = courseBase;
        }

        void Update()
        {
            bool actif = Actif;
            Vector2 deplacement = actif ? Entrees.Deplacement() : Vector2.zero;
            Silence = actif && EntreesJeu.Silence();
            Course = actif && !Silence && Entrees.Course();
            Bouge = deplacement.sqrMagnitude > 0.01f;

            controleur.vitesseMarche = Silence ? vitesseSilence : marcheBase;
            controleur.vitesseCourse = Silence ? vitesseSilence : courseBase;

            Vector3 haut = Haut;
            Vector3 v = corps.linearVelocity;
            Vitesse = Vector3.ProjectOnPlane(v, haut).magnitude;
            float vVerticale = Vector3.Dot(v, haut);
            AuSol = Physics.Raycast(corps.position, -haut, controleur.demiHauteur + 0.2f, ~0, QueryTriggerInteraction.Ignore);

            if (actif && etaitAuSol && Entrees.Saut()) Saut?.Invoke();
            if (AuSol && !etaitAuSol && vitesseVerticalePrecedente < -chuteMinimale) Atterrissage?.Invoke(-vitesseVerticalePrecedente);

            etaitAuSol = AuSol;
            vitesseVerticalePrecedente = vVerticale;
        }
    }
}
