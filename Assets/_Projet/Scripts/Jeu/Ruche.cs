using System;
using System.Collections.Generic;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// La Ruche des Sables : quand la jauge de bruit est pleine, le sol tremble trois secondes,
    /// puis un ver jaillit exactement là où était le bruit. Si l'Arpenteur y est encore, il est projeté
    /// et perd la moitié de ses cristaux (qui retournent à leurs gisements).
    /// </summary>
    public class Ruche : MonoBehaviour
    {
        public static Ruche Instance { get; private set; }

        public float dureeTremblement = 3f;
        public float dureeAttaque = 2.8f;
        public float rayonImpact = 7.5f;
        [Header("Trajectoire du ver (arc qui sort puis replonge)")]
        public float hauteurArc = 17f;
        public float profondeur = 4f;
        public float longueur = 30f;

        [Tooltip("Modèle du ver (facultatif, par exemple le ver des sables de Sketchfab). Sinon : segments générés.")]
        public GameObject modeleVer;
        public float echelleModele = 1f;
        [Tooltip("Rotation du modèle pour que son avant suive la trajectoire.")]
        public Vector3 rotationModele;

        /// <summary>Fin d'attaque : true si l'Arpenteur a été touché.</summary>
        public static event Action<bool> AttaqueTerminee;

        enum Phase { Repos, Tremblement, Attaque }
        Phase phase = Phase.Repos;
        public bool EnCours => phase != Phase.Repos;

        float t, sortie;
        bool touche, gerbe;
        Vector3 point, haut, avant;
        readonly List<Transform> segments = new List<Transform>();
        Transform tete, instanceModele;
        GameObject racine;

        void OnEnable()
        {
            Instance = this;
            JaugeBruit.BruitMaximal += Declencher;
        }

        void OnDisable()
        {
            JaugeBruit.BruitMaximal -= Declencher;
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            sortie = Mathf.Asin(profondeur / hauteurArc) / Mathf.PI;   // instant où la gueule perce le sol
            racine = new GameObject("Ver");
            racine.transform.SetParent(transform, false);
            if (modeleVer != null)
            {
                instanceModele = Instantiate(modeleVer, racine.transform).transform;
                instanceModele.localScale = Vector3.one * echelleModele;
            }
            else
            {
                var carapace = Formes.Opaque(new Color(0.24f, 0.17f, 0.12f), 0.1f, 0.35f);
                for (int i = 0; i < 20; i++)
                {
                    var s = Formes.Primitive(PrimitiveType.Sphere, racine.transform, carapace, "Segment").transform;
                    s.localScale = Vector3.one * Mathf.Lerp(5.4f, 1.8f, i / 19f);
                    segments.Add(s);
                }
                tete = segments[0];
                var gueule = Formes.Primitive(PrimitiveType.Sphere, tete, Formes.Emissif(new Color(0.08f, 0.02f, 0.02f), new Color(0.9f, 0.25f, 0.1f) * 0.6f), "Gueule").transform;
                gueule.localScale = new Vector3(0.7f, 0.7f, 0.2f);
                gueule.localPosition = new Vector3(0f, 0f, 0.42f);
            }
            racine.SetActive(false);
        }

        /// <summary>Déclenche l'attaque au point donné (appelé par la jauge de bruit).</summary>
        public void Declencher(Vector3 position)
        {
            if (EnCours) return;
            phase = Phase.Tremblement;
            t = 0f;
            touche = false;
            gerbe = false;
            point = position;
            haut = Planete.Haut(position);
            avant = Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, haut).normalized;
            if (avant.sqrMagnitude < 0.01f) avant = Vector3.ProjectOnPlane(Vector3.forward, haut).normalized;
            GestionnaireJeu.Instance?.interfaceJeu?.Alerte("Le sol tremble", "Éloignez-vous de ce point, vite");
            BanqueSons.JouerA(BanqueSons.Instance?.tremblement, position, 1f);
        }

        /// <summary>Arrête tout (montée dans la navette, fin de partie).</summary>
        public void Annuler()
        {
            phase = Phase.Repos;
            if (racine != null) racine.SetActive(false);
            if (CameraSecousse.Instance != null) CameraSecousse.Instance.Continue = 0f;
            GestionnaireJeu.Instance?.interfaceJeu?.Alerte(null, null);
        }

        Vector3 Chemin(float s) =>
            point + haut * (Mathf.Sin(Mathf.PI * s) * hauteurArc - profondeur) + avant * ((s - sortie) * longueur);

        void Update()
        {
            if (phase == Phase.Repos) return;
            t += Time.deltaTime;
            var secousse = CameraSecousse.Instance;

            if (phase == Phase.Tremblement)
            {
                if (secousse != null) secousse.Continue = Mathf.SmoothStep(0f, 1f, t / dureeTremblement);
                if (t < dureeTremblement) return;
                phase = Phase.Attaque;
                t = 0f;
                racine.SetActive(true);
                GestionnaireJeu.Instance?.interfaceJeu?.Alerte(null, null);
                BanqueSons.JouerA(BanqueSons.Instance?.attaqueVer, point, 1f);
                return;
            }

            float k = t / dureeAttaque;
            if (secousse != null) secousse.Continue = Mathf.Max(0f, 1f - k * 1.6f);
            float avance = Mathf.Min(1.25f, k * 1.35f);

            if (instanceModele != null)
            {
                float s = Mathf.Clamp01(avance);
                instanceModele.position = Chemin(s);
                Vector3 direction = Chemin(Mathf.Min(1f, s + 0.02f)) - instanceModele.position;
                if (direction.sqrMagnitude > 1e-4f)
                    instanceModele.rotation = Quaternion.LookRotation(direction, haut) * Quaternion.Euler(rotationModele);
                instanceModele.gameObject.SetActive(avance < 1.02f);
            }
            for (int i = 0; i < segments.Count; i++)
            {
                float si = avance - i * 0.034f;
                segments[i].gameObject.SetActive(si > 0f && si < 1.02f);
                segments[i].position = Chemin(Mathf.Clamp01(si));
            }
            if (tete != null)
            {
                Vector3 direction = Chemin(Mathf.Min(1f, Mathf.Clamp01(avance) + 0.02f)) - tete.position;
                if (direction.sqrMagnitude > 1e-4f) tete.rotation = Quaternion.LookRotation(direction, haut);
            }

            if (!gerbe && avance >= sortie)
            {
                gerbe = true;
                CameraSecousse.Choc(0.8f);
            }
            if (!touche && avance >= sortie - 0.01f && avance <= sortie + 0.12f) VerifierImpact();

            if (k >= 1f) Terminer();
        }

        void VerifierImpact()
        {
            var jeu = GestionnaireJeu.Instance;
            if (jeu == null || jeu.Etat != EtatJeu.Jeu) return;
            Vector3 ecart = Vector3.ProjectOnPlane(jeu.joueur.transform.position - point, haut);
            if (ecart.magnitude >= rayonImpact) return;
            touche = true;
            int perte = Mission.Instance != null ? Mission.Instance.PerdreMoitie() : 0;
            var corps = jeu.joueur.GetComponent<Rigidbody>();
            if (corps != null)
            {
                Vector3 recul = ecart.sqrMagnitude > 0.01f ? ecart.normalized : -avant;
                corps.linearVelocity = Vector3.zero;
                corps.AddForce(haut * 9f + recul * 14f, ForceMode.VelocityChange);
            }
            CameraSecousse.Choc(1f);
            BanqueSons.Jouer(BanqueSons.Instance?.impact);
            string texte = perte > 0
                ? $"Impact. {perte} {(perte > 1 ? "cristaux" : "cristal")} d'Éther perdu{(perte > 1 ? "s" : "")} dans le sable."
                : "Impact. Combinaison endommagée, rien de perdu.";
            Transmissions.Envoyer("Alerte combinaison", texte, duree: 6f);
            jeu.interfaceJeu?.Impact();
        }

        void Terminer()
        {
            phase = Phase.Repos;
            racine.SetActive(false);
            if (CameraSecousse.Instance != null) CameraSecousse.Instance.Continue = 0f;
            var jauge = GestionnaireJeu.Instance != null ? GestionnaireJeu.Instance.jauge : null;
            if (jauge != null) jauge.ApresAttaque(35f, 6f);
            if (!Transmissions.DejaNote("La Ruche des Sables"))
                Transmissions.Noter("La Ruche des Sables", "Quelque chose vit sous le sable et chasse au son. Il frappe là où le bruit était, pas là où vous êtes.", true);
            AttaqueTerminee?.Invoke(touche);
        }
    }
}
