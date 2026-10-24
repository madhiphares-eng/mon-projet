using System;
using System.Collections.Generic;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Gisement de cristaux d'Éther : maintenir E à proximité pour récolter (bruyant).
    /// Si l'objet n'a pas de modèle enfant, une grappe d'éclats lumineux est générée.
    /// </summary>
    public class CristalEther : MonoBehaviour
    {
        public static readonly List<CristalEther> Tous = new List<CristalEther>();
        public static event Action<CristalEther> Recolte;

        static readonly string[] EchosParDefaut =
        {
            "… j'ai déjà tenu ce cristal. Demain, peut-être.",
            "Une voix d'enfant compte jusqu'à cinq, puis recommence.",
            "Le cristal est tiède. Il se souvient d'une main qui n'est pas la vôtre.",
            "Vous entendez votre propre respiration, avec un léger retard.",
            "Un vaisseau décolle, quelque part, il y a mille ans.",
            "« Ne prends pas le dernier. » La voix est la vôtre.",
            "Une odeur de pluie, sur une planète sans eau.",
            "Quelqu'un rit, très loin. Puis le rire s'inverse.",
            "Une porte se ferme, dans une maison que vous n'avez pas encore.",
        };

        public float distanceRecolte = 3.8f;
        [Tooltip("Durée de récolte en secondes (divisée par la vitesse de la spécialité).")]
        public float dureeRecolte = 2.2f;
        [TextArea] public string echo;
        public Color couleur = new Color(0.4f, 1.6f, 1.5f);
        [Tooltip("Graine de la forme générée.")]
        public int graine = 1;

        public bool EstRecolte { get; private set; }
        public float Progression { get; private set; }
        /// <summary>Secondes restantes pendant lesquelles le scanner affiche ce cristal.</summary>
        public float Revele { get; private set; }

        Material materiau;
        Light lueur;
        Transform visuel;

        void OnEnable() => Tous.Add(this);
        void OnDisable() => Tous.Remove(this);

        void Start()
        {
            if (string.IsNullOrEmpty(echo)) echo = EchosParDefaut[Mathf.Abs(graine) % EchosParDefaut.Length];
            if (GetComponentInChildren<Renderer>() == null) Construire();
            else materiau = GetComponentInChildren<Renderer>().material;
            Appliquer();
        }

        void Construire()
        {
            visuel = new GameObject("Eclats").transform;
            visuel.SetParent(transform, false);
            materiau = Formes.Emissif(new Color(0.15f, 0.45f, 0.45f), couleur * 2.2f);
            var alea = new System.Random(graine * 7919);
            int n = 6 + alea.Next(4);
            for (int k = 0; k < n; k++)
            {
                var e = new GameObject("Eclat", typeof(MeshFilter), typeof(MeshRenderer));
                e.transform.SetParent(visuel, false);
                e.GetComponent<MeshFilter>().sharedMesh = Formes.Eclat;
                e.GetComponent<MeshRenderer>().sharedMaterial = materiau;
                float r = 0.16f + (float)alea.NextDouble() * 0.2f;
                float h = 0.8f + (float)alea.NextDouble() * (k == 0 ? 2.4f : 1.6f);
                float ang = (float)alea.NextDouble() * 360f, d = k == 0 ? 0f : 0.25f + (float)alea.NextDouble() * 0.6f;
                e.transform.localPosition = Quaternion.Euler(0f, ang, 0f) * Vector3.forward * d + Vector3.down * 0.15f;
                e.transform.localRotation = Quaternion.Euler(0f, ang, 0f) * Quaternion.Euler(d * 40f, 0f, 0f);
                e.transform.localScale = new Vector3(r, h, r);
            }
            var socle = Formes.Primitive(PrimitiveType.Sphere, visuel, Formes.Opaque(new Color(0.28f, 0.22f, 0.18f), 0f, 0.2f), "Socle");
            socle.transform.localScale = new Vector3(2.2f, 0.8f, 2.2f);
            socle.transform.localPosition = Vector3.down * 0.25f;

            lueur = Formes.Lumiere(transform, "Lueur", new Color(0.45f, 1f, 0.95f), 400f, 9f);
            lueur.transform.localPosition = Vector3.up * 1.2f;
        }

        void Appliquer()
        {
            if (visuel != null) visuel.localScale = Vector3.one * (EstRecolte ? 0.6f : 1f);
            if (lueur != null) lueur.enabled = !EstRecolte;
            if (materiau != null) Formes.DefinirEmission(materiau, EstRecolte ? Color.black : couleur * 2.2f);
        }

        public void Reveler(float duree) => Revele = Mathf.Max(Revele, duree);

        void Update()
        {
            if (Revele > 0f) Revele -= Time.deltaTime;
            if (EstRecolte) return;

            float pulsation = 1.9f + Mathf.Sin(Time.time * 2f + graine) * 0.5f + Progression * 3f;
            if (materiau != null) Formes.DefinirEmission(materiau, couleur * pulsation);

            var jeu = GestionnaireJeu.Instance;
            if (jeu == null || jeu.Etat != EtatJeu.Jeu) return;
            float d = Vector3.Distance(jeu.joueur.transform.position, transform.position);
            if (d > distanceRecolte) { Progression = Mathf.Max(0f, Progression - Time.deltaTime); return; }

            var s = ProfilArpenteur.Actuel.Donnees;
            if (EntreesJeu.InteragirMaintenu())
            {
                Progression += Time.deltaTime * s.vitesseRecolte * (ProfilArpenteur.Actuel.A(Talent.RecolteurExpert) ? 1.4f : 1f) / dureeRecolte;
                if (jeu.jauge != null) jeu.jauge.SignalerRecolte();
                if (Progression >= 1f) Recolter();
            }
            else Progression = Mathf.Max(0f, Progression - Time.deltaTime);
            if (!EstRecolte) Invite.Proposer("Maintenir E · récolter l'Éther", Progression, 10f - d);
        }

        public void Recolter()
        {
            EstRecolte = true;
            Progression = 0f;
            Appliquer();
            BanqueSons.Jouer(BanqueSons.Instance?.cristal, 0.8f);
            Transmissions.Echo("Écho d'un cristal", echo);
            if (Mission.Instance != null) Mission.Instance.AjouterEther();
            Recolte?.Invoke(this);
        }

        /// <summary>Le cristal retourne à son gisement (perdu après une attaque de la Ruche).</summary>
        public void Rendre()
        {
            EstRecolte = false;
            Progression = 0f;
            Appliquer();
        }
    }
}
