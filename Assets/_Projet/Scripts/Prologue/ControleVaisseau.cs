using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Le pilotage d'un vaisseau du prologue (la navette du hangar) : les gaz, la postcombustion, la souris pour le
    /// tangage et le lacet, Q/D (A/D en QWERTY) pour le roulis, Espace et C pour monter et descendre.
    /// L'assistance de vol amortit la dérive : le vaisseau va où pointe son nez. Le corps physique (Rigidbody, sans
    /// gravité) fait buter le vaisseau contre les murs. Dans une zone « hangar » (un déclencheur), la vitesse est bridée.
    /// L'avant est +Z. Les propulseurs suivent les gaz.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ControleVaisseau : MonoBehaviour
    {
        public bool pilote;
        public float vitesseMax = 70f;
        public float vitesseHangar = 14f;
        public float postCombustion = 2.2f;
        public float vitesseVerticale = 6f;
        [Tooltip("Degrés par seconde pour un mouvement de souris")] public float sensibilite = 6f;
        [Tooltip("Degrés par seconde de roulis")] public float roulis = 95f;
        public bool assistance = true;
        public Propulseur[] propulseurs;
        [Tooltip("Les yeux du pilote (vue cockpit)")] public Transform place;
        [Tooltip("Là où l'on monte et descend")] public Transform porte;
        [Tooltip("Déclencheur qui délimite le hangar (vitesse bridée)")] public Collider hangar;

        public float Gaz { get; private set; }
        public bool Boost { get; private set; }
        public Vector3 Vitesse => corps != null ? corps.linearVelocity : Vector3.zero;
        public bool DansLeHangar => hangar != null && hangar.bounds.Contains(transform.position);

        Rigidbody corps;
        Vector3 rotation;           // tangage, lacet, roulis voulus (degrés par seconde)
        float montee;

        void Awake()
        {
            corps = GetComponent<Rigidbody>();
            corps.useGravity = false;
            corps.interpolation = RigidbodyInterpolation.Interpolate;
            corps.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            if (propulseurs == null || propulseurs.Length == 0) propulseurs = GetComponentsInChildren<Propulseur>();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            Boost = pilote && EntreesJeu.Boost();
            if (pilote)
            {
                Vector2 d = Entrees.Deplacement();
                Gaz = Mathf.Clamp01(Gaz + d.y * dt * (d.y > 0f ? 0.6f : 0.8f));
                Vector2 souris = Cursor.lockState == CursorLockMode.Locked ? Entrees.Regard() : Vector2.zero;
                rotation.x += -souris.y * sensibilite * CameraOrbitale.MultiplicateurSensibilite * (CameraOrbitale.InverserY ? -1f : 1f);
                rotation.y += souris.x * sensibilite * CameraOrbitale.MultiplicateurSensibilite;
                rotation.z = Mathf.Lerp(rotation.z, -d.x * roulis, 1f - Mathf.Exp(-6f * dt));
                montee = (EntreesJeu.Monter() ? 1f : 0f) - (EntreesJeu.Descendre() ? 1f : 0f);
            }
            else montee = 0f;
            rotation.x *= Mathf.Exp(-4f * dt);
            rotation.y *= Mathf.Exp(-4f * dt);
            foreach (var p in propulseurs) if (p != null) p.puissance = pilote ? 0.15f + Gaz * 0.85f + (Boost ? 0.4f : 0f) : 0f;
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            corps.MoveRotation(corps.rotation * Quaternion.Euler(rotation.x * dt * 6f, rotation.y * dt * 6f, rotation.z * dt));
            float vmax = (DansLeHangar ? vitesseHangar : vitesseMax) * (Boost ? postCombustion : 1f);
            Vector3 voulue = transform.forward * Gaz * vmax + transform.up * montee * vitesseVerticale;
            corps.linearVelocity = Vector3.Lerp(corps.linearVelocity, voulue, 1f - Mathf.Exp(-(assistance ? 1.4f : 0.3f) * dt));
        }

        /// <summary>Posé : dans le hangar, presque arrêté, près du sol.</summary>
        public bool Pose()
        {
            if (!DansLeHangar || Vitesse.magnitude > 2f) return false;
            return Physics.Raycast(transform.position, Vector3.down, 2.2f, ~0, QueryTriggerInteraction.Ignore);
        }

        public void Couper()
        {
            pilote = false;
            Gaz = 0f;
            rotation = Vector3.zero;
        }
    }
}
