using System;
using EchosEther.Combat;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Compétence active (Q, bouton RB à la manette), talents passifs et expérience de l'Arpenteur.
    /// Les talents s'appliquent par des multiplicateurs lus par les autres systèmes (bruit, vitesse, armes...).
    /// </summary>
    [RequireComponent(typeof(EtatArpenteur))]
    public class CompetencesArpenteur : MonoBehaviour
    {
        public static CompetencesArpenteur Instance { get; private set; }

        public static event Action<CompetenceActive> Utilisee;
        /// <summary>Nouveau niveau atteint.</summary>
        public static event Action<int> NiveauGagne;
        /// <summary>Expérience gagnée (quantité, raison).</summary>
        public static event Action<int, string> ExperienceGagnee;

        public CompetenceActive Active => ProfilArpenteur.Actuel.competence;
        public InfoCompetence Info => Competences.Info(Active);
        /// <summary>Secondes avant la prochaine utilisation.</summary>
        public float Attente => Mathf.Max(0f, prochain - Time.time);
        public float AttenteMax { get; private set; } = 1f;
        /// <summary>Effet en cours (voile, bouclier...), de 1 à 0.</summary>
        public float Effet => dureeEffet > 0f ? Mathf.Clamp01((finEffet - Time.time) / dureeEffet) : 0f;
        public bool VoileActif => Active == CompetenceActive.Voile && Time.time < finEffet;

        EtatArpenteur etat;
        Rigidbody corps;
        float prochain, finEffet, dureeEffet;
        Transform coquille;
        Material materiauCoquille;
        bool monolitheCompte;

        void Awake()
        {
            etat = GetComponent<EtatArpenteur>();
            corps = GetComponent<Rigidbody>();
        }

        void OnEnable()
        {
            Instance = this;
            CristalEther.Recolte += SurRecolte;
            Echarde.Morte += SurMort;
            Ruche.AttaqueTerminee += SurAttaque;
            Ruche.CreaturesAvalees += SurAvalees;
        }

        void OnDisable()
        {
            CristalEther.Recolte -= SurRecolte;
            Echarde.Morte -= SurMort;
            Ruche.AttaqueTerminee -= SurAttaque;
            Ruche.CreaturesAvalees -= SurAvalees;
            if (Instance == this) Instance = null;
            JaugeBruit.MultiplicateurBruit = 1f;
            EtatArpenteur.MultiplicateurVitesse = 1f;
        }

        void OnDestroy() => ProfilArpenteur.Actuel.Sauver();

        void SurRecolte(CristalEther c) => Experience(40, "Cristal d'Éther");
        void SurMort(Echarde e, bool parJoueur) { if (parJoueur) Experience(e.Experience, e.NomType); }
        void SurAttaque(bool touche) { if (!touche) Experience(20, "Esquive"); }
        void SurAvalees(int n) => Experience(10 * n, "Le ver a mangé");

        /// <summary>Ajoute de l'expérience ; à chaque niveau, un talent est à choisir (touche K).</summary>
        public void Experience(int quantite, string raison)
        {
            var profil = ProfilArpenteur.Actuel;
            int gagnes = profil.AjouterExperience(quantite);
            ExperienceGagnee?.Invoke(quantite, raison);
            if (gagnes <= 0) return;
            profil.Sauver();
            BanqueSons.Jouer(BanqueSons.Instance?.niveau, 0.8f);
            EffetsCombat.Anneau(transform.position, etat.Haut, 3f, new Color(2.2f, 1.6f, 0.6f), 0.9f);
            for (int i = 0; i < 6; i++) EffetsCombat.Etincelle(transform.position + UnityEngine.Random.insideUnitSphere * 0.6f, new Color(2.2f, 1.6f, 0.6f), 0.18f);
            Transmissions.Envoyer($"Niveau {profil.niveau}", "Le Bureau reconnaît vos mérites. Un nouveau talent est à choisir : touche K (ou Select longtemps à la manette).", duree: 7f, noter: false);
            NiveauGagne?.Invoke(profil.niveau);
        }

        void Update()
        {
            var profil = ProfilArpenteur.Actuel;
            JaugeBruit.MultiplicateurBruit = (profil.A(Talent.PasFeutres) ? 0.75f : 1f) * (VoileActif ? 0.08f : 1f);
            EtatArpenteur.MultiplicateurVitesse = profil.A(Talent.PiedLeger) ? 1.12f : 1f;

            var jeu = GestionnaireJeu.Instance;
            if (!monolitheCompte && Mission.Instance != null && Mission.Instance.MonolitheActive) { monolitheCompte = true; Experience(120, "Le monolithe"); }

            AnimerCoquille();
            if (jeu == null || jeu.Etat != EtatJeu.Jeu || !etat.Actif) return;
            if (EntreesJeu.Competence())
            {
                if (Attente <= 0f) Activer();
                else BanqueSons.Jouer(BanqueSons.Instance?.vide, 0.4f);
            }
        }

        /// <summary>Utilise la compétence équipée.</summary>
        public void Activer()
        {
            var c = Active;
            var info = Competences.Info(c);
            var jeu = GestionnaireJeu.Instance;
            Vector3 haut = etat.Haut;
            Vector3 pieds = transform.position - haut * (jeu != null && jeu.joueur != null ? jeu.joueur.demiHauteur : 1f);

            switch (c)
            {
                case CompetenceActive.Bond:
                {
                    Vector3 avant = Camera.main != null ? Vector3.ProjectOnPlane(Camera.main.transform.forward, haut).normalized : transform.forward;
                    Vector3 droite = Vector3.Cross(haut, avant);
                    Vector2 d = Entrees.Deplacement();
                    Vector3 dir = d.sqrMagnitude > 0.05f ? (avant * d.y + droite * d.x).normalized : avant;
                    if (corps != null) corps.linearVelocity = Vector3.ProjectOnPlane(corps.linearVelocity, haut);
                    if (jeu != null && jeu.joueur != null) jeu.joueur.Propulser(haut * 8.5f + dir * 13f);
                    EffetsCombat.Anneau(pieds, haut, 2.5f, info.couleur, 0.4f);
                    EffetsCombat.Impact(pieds, -dir + haut * 0.2f, info.couleur, 12, 0.8f);
                    jeu?.jauge?.Ajouter(10f);
                    break;
                }
                case CompetenceActive.Voile:
                    finEffet = Time.time + info.duree;
                    dureeEffet = info.duree;
                    PreparerCoquille(info.couleur, 1.25f, 3f);
                    if (jeu != null && jeu.jauge != null) jeu.jauge.Ajouter(-40f);
                    break;
                case CompetenceActive.Stase:
                {
                    Vector3 point = Visee.PointAuSol(32f);
                    var go = new GameObject("Bulle de stase");
                    go.transform.position = point + Planete.Haut(point) * 0.5f;
                    var b = go.AddComponent<BulleTemporelle>();
                    b.rapport = 0.08f;
                    b.rayon = 6.5f;
                    b.respire = false;
                    b.ephemere = true;
                    b.couleur = info.couleur;
                    b.distanceDecouverte = 0f;
                    Destroy(go, info.duree);
                    EffetsCombat.Anneau(point, Planete.Haut(point), 7f, info.couleur, 0.6f);
                    jeu?.jauge?.Ajouter(6f);
                    break;
                }
                case CompetenceActive.Leurre:
                {
                    Vector3 point = Visee.PointAuSol(26f);
                    Hologramme.Creer(point, transform.rotation, info.couleur, info.duree);
                    break;
                }
                case CompetenceActive.Onde:
                    Combat.Combat.Zone(transform.position, 7f, 32f, 13f);
                    EffetsCombat.Anneau(pieds, haut, 7.5f, info.couleur, 0.55f);
                    EffetsCombat.Impact(pieds, haut, info.couleur, 18, 1.4f);
                    EffetsCombat.Flash(pieds + haut, info.couleur, 30000f, 0.25f, 14f);
                    CameraSecousse.Choc(0.55f);
                    BanqueSons.JouerA(BanqueSons.Instance?.explosion, pieds, 0.8f);
                    jeu?.jauge?.Ajouter(28f);
                    break;
                case CompetenceActive.Bouclier:
                    finEffet = Time.time + info.duree;
                    dureeEffet = info.duree;
                    GetComponent<VieArpenteur>()?.ActiverBouclier(60f, info.duree);
                    PreparerCoquille(info.couleur, 1.35f, 2.2f);
                    break;
            }
            AttenteMax = info.recharge * (ProfilArpenteur.Actuel.A(Talent.Recuperation) ? 0.7f : 1f);
            prochain = Time.time + AttenteMax;
            BanqueSons.Jouer(c == CompetenceActive.Onde ? null : BanqueSons.Instance?.competence, 0.7f);
            Utilisee?.Invoke(c);
        }

        void PreparerCoquille(Color couleur, float rayon, float fresnel)
        {
            if (coquille == null)
            {
                materiauCoquille = Formes.Lueur(couleur, fresnel, 0.02f, 2f);
                coquille = Formes.Primitive(PrimitiveType.Sphere, transform, materiauCoquille, "Coquille").transform;
            }
            materiauCoquille.SetColor("_Couleur", couleur);
            materiauCoquille.SetFloat("_Fresnel", fresnel);
            coquille.localScale = Vector3.one * rayon * 2f;
            coquille.gameObject.SetActive(true);
        }

        void AnimerCoquille()
        {
            if (coquille == null || !coquille.gameObject.activeSelf) return;
            bool actif = Time.time < finEffet;
            if (Active == CompetenceActive.Bouclier)
            {
                var vie = GetComponent<VieArpenteur>();
                actif &= vie != null && vie.Bouclier > 0f;
            }
            if (!actif) { coquille.gameObject.SetActive(false); return; }
            float fin = Mathf.Clamp01((finEffet - Time.time) / 1.2f);
            float clignote = fin < 1f ? (Mathf.Repeat(Time.time * 6f, 1f) < 0.5f ? 1f : 0.4f) : 1f;
            materiauCoquille.SetFloat("_Intensite", (0.6f + 0.25f * Mathf.Sin(Time.time * 5f)) * clignote);
        }
    }

    /// <summary>
    /// Double holographique de l'Arpenteur (compétence Leurre) : attire les créatures,
    /// puis crie pour appeler la Ruche à sa place.
    /// </summary>
    public class Hologramme : MonoBehaviour
    {
        /// <summary>Leurre en cours (null si aucun) : les créatures le préfèrent à l'Arpenteur.</summary>
        public static Hologramme Actuel { get; private set; }

        float duree, t;
        bool cri;
        Material materiau;
        Color couleur;

        public static Hologramme Creer(Vector3 sol, Quaternion orientation, Color couleur, float duree)
        {
            var go = new GameObject("Leurre holographique");
            Vector3 haut = Planete.Haut(sol);
            go.transform.SetPositionAndRotation(sol + haut * 1f, Planete.Debout(sol, orientation * Vector3.forward));
            var h = go.AddComponent<Hologramme>();
            h.duree = duree;
            h.couleur = couleur;
            h.materiau = Formes.Lueur(couleur, 1.6f, 0.15f, 3f);
            var corps = Formes.Primitive(PrimitiveType.Capsule, go.transform, h.materiau, "Silhouette").transform;
            corps.localScale = new Vector3(0.7f, 1f, 0.7f);
            var tete = Formes.Primitive(PrimitiveType.Sphere, go.transform, h.materiau, "Casque").transform;
            tete.localScale = Vector3.one * 0.42f;
            tete.localPosition = Vector3.up * 0.95f;
            EffetsCombat.Anneau(sol, haut, 2.5f, couleur, 0.5f);
            Actuel = h;
            return h;
        }

        void Update()
        {
            t += Time.deltaTime;
            materiau.SetFloat("_Intensite", 0.5f + 0.35f * Mathf.Sin(t * 13f) + (Mathf.Repeat(t * 3.1f, 1f) < 0.08f ? -0.4f : 0f));
            if (!cri && t > duree * 0.7f)
            {
                cri = true;
                Vector3 sol = transform.position - Planete.Haut(transform.position);
                JaugeBruit.Signaler(sol, 50f);
                JaugeBruit.Leurre(sol);
                EffetsCombat.Anneau(sol, Planete.Haut(sol), 6f, couleur, 0.7f);
                BanqueSons.JouerA(BanqueSons.Instance?.cri, sol, 1f);
            }
            if (t >= duree) Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (Actuel == this) Actuel = null;
            EffetsCombat.Eclats(transform.position, couleur, 8, 3f, 0.15f);
        }
    }
}
