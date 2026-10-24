using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Navette pilotable. Garée : E à côté pour monter à bord. En vol : souris pour orienter,
    /// Z/S poussée, Q/D latéral, Espace/C monter/descendre, Maj boost, E pour se poser
    /// (près du sol, lentement, sur un terrain assez plat) ou pour s'arrimer à la balise d'extraction.
    /// </summary>
    public class Navette : MonoBehaviour
    {
        public enum EtatNavette { Garee, Vol }

        [Header("Pilotage")]
        public float poussee = 32f;
        public float pousseeBoost = 75f;
        public float lateral = 20f;
        public float montee = 24f;
        public float amortissement = 0.9f;
        public float sensibilite = 1.6f;
        public float plafond = 900f;

        [Header("Atterrissage")]
        public float altitudeMinimale = 2.2f;
        public float altitudeAtterrissage = 9f;
        public float vitesseAtterrissage = 12f;
        [Tooltip("Dénivelé maximal (m) sous la navette pour se poser.")]
        public float deniveleMax = 4f;
        public float distanceEmbarquement = 9f;
        [Tooltip("Distance entre la navette et le point où l'Arpenteur descend.")]
        public float distanceSortie = 7.5f;
        [Tooltip("Hauteur du centre de la navette au-dessus du sol une fois posée.")]
        public float hauteurPosee = 0.3f;

        [Header("Effets")]
        [ColorUsage(false, true)] public Color couleurReacteurs = new Color(2.6f, 1.4f, 0.6f);
        public Vector3[] positionsReacteurs = { new Vector3(-2.2f, -0.4f, 2.5f), new Vector3(2.2f, -0.4f, 2.5f), new Vector3(-2.2f, -0.4f, -2.5f), new Vector3(2.2f, -0.4f, -2.5f) };

        /// <summary>La navette vient de se poser (l'Arpenteur est redescendu).</summary>
        public static event System.Action Posee;

        public EtatNavette Etat { get; private set; } = EtatNavette.Garee;
        public float Altitude { get; private set; }
        public float Vitesse => vitesse.magnitude;
        /// <summary>Pilotée par un script (largage) : on ne peut pas monter à bord.</summary>
        public bool EnManoeuvre { get; set; }

        Vector3 vitesse;
        float lacet, tangage, roulis;
        Quaternion cadre = Quaternion.identity;
        Transform[] reacteurs;
        Material materiauReacteurs;
        Light lumiere;
        AudioSource moteur;

        void Start()
        {
            materiauReacteurs = Formes.Lueur(couleurReacteurs, 1.5f, 0.6f);
            reacteurs = new Transform[positionsReacteurs.Length];
            for (int i = 0; i < reacteurs.Length; i++)
            {
                reacteurs[i] = Formes.Primitive(PrimitiveType.Sphere, transform, materiauReacteurs, "Reacteur").transform;
                reacteurs[i].localPosition = positionsReacteurs[i];
            }
            lumiere = Formes.Lumiere(transform, "LumiereReacteurs", new Color(1f, 0.66f, 0.38f), 0f, 60f);
            lumiere.transform.localPosition = Vector3.down * 2f;
            moteur = gameObject.AddComponent<AudioSource>();
            moteur.loop = true;
            moteur.spatialBlend = 0.8f;
            moteur.playOnAwake = false;
            if (BanqueSons.Instance != null) moteur.clip = BanqueSons.Instance.moteurNavette;
            Reacteurs(0f);
        }

        /// <summary>Flammes, lumière et son des réacteurs (0 = éteints). Utilisé aussi par la séquence de largage.</summary>
        public void Reacteurs(float intensite)
        {
            if (reacteurs != null)
                foreach (var r in reacteurs) r.localScale = Vector3.one * (0.8f + Random.value * 0.25f) * 2.2f * intensite;
            if (materiauReacteurs != null) materiauReacteurs.SetFloat("_Intensite", intensite);
            if (lumiere != null) { lumiere.enabled = intensite > 0.01f; lumiere.intensity = 20000f * intensite; }
            if (moteur != null && moteur.clip != null)
            {
                if (intensite > 0.01f && !moteur.isPlaying) moteur.Play();
                else if (intensite <= 0.01f && moteur.isPlaying) moteur.Stop();
                moteur.volume = Mathf.Clamp01(intensite) * 0.7f;
                moteur.pitch = 0.8f + intensite * 0.3f;
            }
        }

        void Update()
        {
            var jeu = GestionnaireJeu.Instance;
            if (jeu == null) return;
            if (Etat == EtatNavette.Garee)
            {
                if (jeu.Etat != EtatJeu.Jeu || EnManoeuvre || jeu.joueur == null) return;
                if (Vector3.Distance(jeu.joueur.transform.position, transform.position) > distanceEmbarquement) return;
                Invite.Proposer("Appuyer sur E · monter à bord de la navette", -1f, 5f);
                if (EntreesJeu.Interagir()) Embarquer();
            }
            else if (jeu.Etat == EtatJeu.Vol) Piloter(Time.deltaTime);
        }

        public void Embarquer()
        {
            var jeu = GestionnaireJeu.Instance;
            Etat = EtatNavette.Vol;
            vitesse = Vector3.zero;
            Vector3 haut = Planete.Haut(transform.position);
            cadre = Quaternion.FromToRotation(Vector3.up, haut);
            Vector3 avantLocal = Quaternion.Inverse(cadre) * transform.forward;
            lacet = Mathf.Atan2(avantLocal.x, avantLocal.z) * Mathf.Rad2Deg;
            tangage = 0f; roulis = 0f;
            transform.position += haut * 1.5f;
            Ruche.Instance?.Annuler();
            BanqueSons.Jouer(BanqueSons.Instance?.clic);
            jeu.EntrerVol(this);
            if (!Transmissions.DejaNote("Navette"))
                Transmissions.Envoyer("Navette", "Navette déverrouillée. Souris pour orienter, Z/S pour la poussée, Q/D latéral, Espace/C pour monter ou descendre, Maj pour accélérer, E pour se poser.", duree: 9f);
        }

        void Piloter(float dt)
        {
            Vector3 haut = Planete.Haut(transform.position);
            cadre = Quaternion.FromToRotation(cadre * Vector3.up, haut) * cadre;

            Vector2 souris = Entrees.Regard() * (sensibilite * CameraOrbitale.MultiplicateurSensibilite);
            lacet += souris.x;
            tangage = Mathf.Clamp(tangage - souris.y * (CameraOrbitale.InverserY ? -1f : 1f), -50f, 50f);

            Quaternion orientation = cadre * Quaternion.Euler(tangage, lacet, 0f);
            Vector3 avant = orientation * Vector3.forward, droite = orientation * Vector3.right;
            Vector2 d = Entrees.Deplacement();
            float m = (EntreesJeu.Monter() ? 1f : 0f) - (EntreesJeu.Descendre() ? 1f : 0f);
            bool boost = EntreesJeu.Boost();

            Vector3 acceleration = avant * (d.y * (boost ? pousseeBoost : poussee)) + droite * (d.x * lateral) + haut * (m * montee);
            vitesse += acceleration * dt;
            vitesse *= Mathf.Exp(-dt * amortissement);       // les réacteurs compensent la gravité
            Vector3 position = transform.position + vitesse * dt;

            // altitude au-dessus du relief
            if (Planete.Sol(position, out RaycastHit sol, transform, 30f, 2000f))
            {
                Altitude = Vector3.Dot(position - sol.point, haut);
                if (Altitude < altitudeMinimale)
                {
                    position += haut * (altitudeMinimale - Altitude);
                    float vr = Vector3.Dot(vitesse, haut);
                    if (vr < 0f) vitesse -= haut * vr;
                    Altitude = altitudeMinimale;
                }
            }
            else Altitude = Vector3.Distance(position, Planete.Centre) - Planete.Rayon;
            if (Altitude > plafond) vitesse -= haut * ((Altitude - plafond) * dt * 2f);

            roulis = Mathf.Lerp(roulis, -d.x * 25f, 1f - Mathf.Exp(-dt * 4f));
            transform.SetPositionAndRotation(position, orientation * Quaternion.Euler(0f, 0f, roulis));
            Reacteurs(0.35f + Mathf.Abs(d.y) * 0.6f + (boost && d.y != 0f ? 0.4f : 0f) + Mathf.Abs(m) * 0.25f);

            var balise = Mission.Instance != null ? Mission.Instance.balise : null;
            if (balise != null && balise.AccessibleDepuis(transform.position))
            {
                Invite.Proposer("Appuyer sur E · s'arrimer à la balise et rentrer", -1f, 6f);
                if (EntreesJeu.Interagir()) { Reacteurs(1.4f); GestionnaireJeu.Instance.Extraction(); }
                return;
            }
            if (Altitude < altitudeAtterrissage && Vitesse < vitesseAtterrissage)
            {
                bool plat = Planete.Denivele(transform.position, 6f, transform) <= deniveleMax;
                Invite.Proposer(plat ? "Appuyer sur E · se poser ici" : "Terrain trop accidenté pour se poser", -1f, 4f);
                if (plat && EntreesJeu.Interagir()) Atterrir();
            }
            else if (EntreesJeu.Interagir())
                Transmissions.Envoyer("Navette", "Trop haut ou trop vite pour se poser. Descendez près du sol et ralentissez.", duree: 3.5f, noter: false);
        }

        void Atterrir()
        {
            Vector3 haut = Planete.Haut(transform.position);
            if (!Planete.Sol(transform.position, out RaycastHit sol, transform)) return;
            Quaternion posee = cadre * Quaternion.Euler(0f, lacet, 0f);
            transform.SetPositionAndRotation(sol.point + haut * hauteurPosee, posee);
            Etat = EtatNavette.Garee;
            vitesse = Vector3.zero;
            Reacteurs(0f);
            CameraSecousse.Choc(0.35f);
            BanqueSons.JouerA(BanqueSons.Instance?.atterrissageNavette, transform.position);

            // l'Arpenteur descend du côté le plus plat
            float hNavette = Vector3.Dot(sol.point - Planete.Centre, haut);
            Vector3 meilleur = transform.position + transform.right * distanceSortie;
            float ecart = float.MaxValue;
            for (int k = 0; k < 8; k++)
            {
                Vector3 cote = posee * Quaternion.Euler(0f, k * 45f, 0f) * Vector3.right;
                Vector3 p = transform.position + cote * distanceSortie;
                if (!Planete.Sol(p, out RaycastHit h, transform)) continue;
                float e = Mathf.Abs(Vector3.Dot(h.point - Planete.Centre, haut) - hNavette) + k * 0.01f;
                if (e < ecart) { ecart = e; meilleur = h.point; }
            }
            if (GestionnaireJeu.Instance != null && GestionnaireJeu.Instance.jauge != null) GestionnaireJeu.Instance.jauge.Ajouter(30f);
            GestionnaireJeu.Instance.SortirVol(meilleur, posee * Vector3.forward);
            Posee?.Invoke();
        }
    }
}
