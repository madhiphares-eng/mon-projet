using System;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Arrivée sur la planète : la navette descend du ciel, survole le point de départ, l'Arpenteur saute,
    /// puis la navette va se poser à sa place. Caméra de cinéma depuis le sol, répliques du pilote.
    /// Espace, Entrée, Échap ou un clic pour passer.
    /// </summary>
    public class Largage : MonoBehaviour
    {
        [Header("Trajectoire de la navette")]
        public float dureeArrivee = 6f;
        public float dureeSurvol = 1.2f;
        public float dureeStationnement = 4.5f;
        public float hauteurDepart = 170f;
        public float distanceDepart = 220f;
        public float hauteurSurvol = 9f;

        [Header("Saut de l'Arpenteur")]
        [Tooltip("Hauteur au-dessus du sol où l'Arpenteur apparaît en sautant de la navette.")]
        public float hauteurSaut = 3f;

        /// <summary>La cinématique a la main (caméra et contrôles).</summary>
        public bool EnCours { get; private set; }
        /// <summary>La navette n'a pas encore fini de se garer.</summary>
        public bool NavetteEnManoeuvre => navette != null && navette.EnManoeuvre;

        GestionnaireJeu jeu;
        Navette navette;
        Camera cam;
        Vector3 depart, haut, avant, droite, debut, survol, parking;
        Quaternion rotationParking;
        float t;
        bool saute, passerDemande;
        int replique;
        Action fin;

        float InstantSaut => dureeArrivee + dureeSurvol * 0.4f;
        float FinSurvol => dureeArrivee + dureeSurvol;
        float FinStationnement => FinSurvol + dureeStationnement;

        static readonly (float instant, string texte)[] Repliques =
        {
            (0.4f, "Kestrel-4 : entrée dans la zone. Turbulences gravitationnelles, accrochez-vous."),
            (3.2f, "Kestrel-4 : survol de la Lisière. Rampe ouverte, sol stable."),
            (5.6f, "Kestrel-4 : à vous, Arpenteur. Je me pose juste à côté."),
        };

        /// <summary>
        /// Lance la séquence. <paramref name="quandFini"/> est appelé quand l'Arpenteur est au sol et peut jouer ;
        /// la navette finit alors de se garer toute seule. Renvoie false si la séquence est impossible.
        /// </summary>
        public bool Demarrer(GestionnaireJeu gestionnaire, Navette n, Vector3 pointDepart, Quaternion orientationDepart, Action quandFini)
        {
            if (gestionnaire == null || n == null) return false;
            jeu = gestionnaire;
            navette = n;
            cam = jeu.cameraOrbitale != null ? jeu.cameraOrbitale.GetComponent<Camera>() : Camera.main;
            fin = quandFini;

            haut = Planete.Haut(pointDepart);
            depart = Planete.Sol(pointDepart, out RaycastHit sol, navette.transform) ? sol.point : pointDepart;
            avant = Vector3.ProjectOnPlane(orientationDepart * Vector3.forward, haut).normalized;
            if (avant.sqrMagnitude < 0.01f) avant = Vector3.ProjectOnPlane(Vector3.forward, haut).normalized;
            droite = Vector3.Cross(haut, avant);
            parking = navette.transform.position;
            rotationParking = navette.transform.rotation;
            debut = depart + haut * hauteurDepart + avant * distanceDepart;
            survol = depart + haut * hauteurSurvol;

            t = 0f;
            saute = false;
            passerDemande = false;
            replique = 0;
            EnCours = true;
            navette.EnManoeuvre = true;
            if (jeu.joueur != null) jeu.joueur.gameObject.SetActive(false);
            PlacerNavette();
            jeu.interfaceJeu?.Fondu(Color.black, 1f);
            return true;
        }

        /// <summary>Passe directement à la fin : navette garée, Arpenteur au sol.</summary>
        public void Passer()
        {
            if (!EnCours) return;
            t = FinStationnement;
            PlacerNavette();
            if (!saute) Sauter(depart + haut * (jeu.joueur != null ? jeu.joueur.demiHauteur + 0.1f : 1f));
            Terminer();
        }

        static float Doux(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }

        void Update()
        {
            if (navette == null || (!EnCours && !navette.EnManoeuvre)) return;
            // passé une image plus tard : la touche Espace ne fait pas aussi sauter l'Arpenteur
            if (passerDemande) { passerDemande = false; Passer(); return; }
            // un appui encore en cours sur l'écran d'enrôlement ne doit pas passer la séquence
            if (EnCours && t > 0.5f && (EntreesJeu.Valider() || EntreesJeu.Pause() || Entrees.CapturerCurseur())) { passerDemande = true; return; }

            t += Time.deltaTime;
            PlacerNavette();
            if (!EnCours) return;

            jeu.interfaceJeu?.Fondu(Color.black, 1f - Mathf.Clamp01(t / 1.4f));
            while (replique < Repliques.Length && t >= Repliques[replique].instant)
                jeu.interfaceJeu?.RepliqueLargage(Repliques[replique++].texte);

            if (!saute && t >= InstantSaut) Sauter(survol - haut * (hauteurSurvol - hauteurSaut));
            // l'Arpenteur prend la main dès qu'il touche le sol ; la navette continue sa manœuvre
            if (saute && t >= InstantSaut + 0.6f && (jeu.etatJoueur == null || jeu.etatJoueur.AuSol)) Terminer();
            else if (saute && t >= InstantSaut + 4f) Terminer();
        }

        /// <summary>Position et poussée de la navette à l'instant t.</summary>
        void PlacerNavette()
        {
            if (t < dureeArrivee)
            {
                float k = Doux(t / dureeArrivee);
                Vector3 p = Vector3.Lerp(debut, survol, k);
                Quaternion cap = Quaternion.LookRotation(Vector3.ProjectOnPlane(survol - debut, haut), haut);
                navette.transform.SetPositionAndRotation(p, cap * Quaternion.Euler((1f - k) * 18f, 0f, Mathf.Sin(k * Mathf.PI) * 8f));
                navette.Reacteurs(0.7f + k * 0.5f);
            }
            else if (t < FinSurvol)
            {
                Quaternion cap = Quaternion.LookRotation(Vector3.ProjectOnPlane(survol - debut, haut), haut);
                navette.transform.SetPositionAndRotation(survol + haut * (Mathf.Sin(t * 2.2f) * 0.25f), cap);
                navette.Reacteurs(1.2f);
            }
            else if (t < FinStationnement)
            {
                float k = Doux((t - FinSurvol) / dureeStationnement);
                Quaternion cap = Quaternion.LookRotation(Vector3.ProjectOnPlane(survol - debut, haut), haut);
                Vector3 p = Vector3.Lerp(survol, parking, k) + Planete.Haut(parking) * (Mathf.Sin(k * Mathf.PI) * 4f);
                navette.transform.SetPositionAndRotation(p, Quaternion.Slerp(cap, rotationParking, k));
                navette.Reacteurs(Mathf.Lerp(1.2f, 0.4f, k));
            }
            else if (navette.EnManoeuvre)
            {
                navette.transform.SetPositionAndRotation(parking, rotationParking);
                navette.Reacteurs(0f);
                navette.EnManoeuvre = false;
                BanqueSons.JouerA(BanqueSons.Instance?.atterrissageNavette, parking);
                if (jeu.joueur != null && Vector3.Distance(jeu.joueur.transform.position, parking) < 40f) CameraSecousse.Choc(0.25f);
            }
        }

        void Sauter(Vector3 position)
        {
            saute = true;
            if (jeu.joueur == null) return;
            jeu.joueur.transform.SetPositionAndRotation(position, Planete.Debout(position, avant));
            jeu.joueur.gameObject.SetActive(true);
            var corps = jeu.joueur.GetComponent<Rigidbody>();
            if (corps != null)
            {
                corps.position = position;
                corps.linearVelocity = -Planete.Haut(position) * 1.5f;
            }
            jeu.interfaceJeu?.RepliqueLargage("");
        }

        void LateUpdate()
        {
            if (!EnCours || cam == null) return;
            // caméra posée au sol derrière le point de départ, qui suit la navette puis l'Arpenteur
            Vector3 position = depart - avant * 15f + droite * 6f + haut * 1.8f;
            if (Planete.Sol(position, out RaycastHit sol, navette.transform) && Vector3.Dot(position - sol.point, haut) < 1.4f)
                position = sol.point + haut * 1.4f;
            Vector3 cible = navette.transform.position + haut * 2f;
            if (saute && jeu.joueur != null) cible = Vector3.Lerp(jeu.joueur.transform.position, cible, 0.3f);
            cam.transform.SetPositionAndRotation(position, Quaternion.LookRotation(cible - position, haut));
        }

        void Terminer()
        {
            if (!EnCours) return;
            EnCours = false;
            jeu.interfaceJeu?.Fondu(Color.black, 0f);
            jeu.interfaceJeu?.RepliqueLargage("");
            fin?.Invoke();
        }

        void OnDisable()
        {
            if (navette != null && navette.EnManoeuvre)
            {
                navette.transform.SetPositionAndRotation(parking, rotationParking);
                navette.Reacteurs(0f);
                navette.EnManoeuvre = false;
            }
        }
    }
}
