using EchosEther.Interface;
using EchosEther.Joueur;
using EchosEther.Physique;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchosEther.Jeu
{
    public enum EtatJeu { Menu, Enrolement, Jeu, Vol, Pause, Extraction, Fin }
    public enum TypePartie { Mission, Didacticiel }

    /// <summary>
    /// Déroulé d'une partie : menu principal, enrôlement (aperçu de l'Arpenteur), jeu à pied, vol en navette,
    /// pause, extraction et écran de fin. Active ou coupe le contrôle du joueur et les caméras.
    /// Recommencer ou revenir au menu recharge la scène : tout repart proprement.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GestionnaireJeu : MonoBehaviour
    {
        public static GestionnaireJeu Instance { get; private set; }

        [Header("Joueur")]
        public ControleurArpenteur joueur;
        public EtatArpenteur etatJoueur;
        public JaugeBruit jauge;
        public ApparenceArpenteur apparence;

        [Header("Caméras")]
        public CameraOrbitale cameraOrbitale;
        public CameraNavette cameraNavette;

        [Header("Systèmes")]
        public InterfaceJeu interfaceJeu;
        public Horloges horloges;
        public Transmissions transmissions;
        public Mission mission;
        public Didacticiel didacticiel;
        public Navette navette;
        [Tooltip("Trou noir visé par la caméra du menu principal.")]
        public Transform trouNoir;

        [Header("Extraction")]
        public float dureeExtraction = 3.5f;

        public EtatJeu Etat { get; private set; } = EtatJeu.Menu;
        public TypePartie Type { get; private set; }
        public Navette NavetteEnVol { get; private set; }

        /// <summary>Position de l'Arpenteur, ou de la navette pendant le vol.</summary>
        public Vector3 PositionObservateur =>
            NavetteEnVol != null ? NavetteEnVol.transform.position : joueur != null ? joueur.transform.position : transform.position;

        // survit au rechargement de la scène : partie à lancer directement
        static TypePartie? partieDemandee;

        Vector3 departPosition;
        Quaternion departRotation;
        Rigidbody corpsJoueur;
        EtatJeu etatAvantPause;
        float tExtraction, angleApercu;
        Camera cam;

        void Awake()
        {
            Instance = this;
            if (joueur == null) joueur = FindFirstObjectByType<ControleurArpenteur>();
            if (joueur != null)
            {
                if (etatJoueur == null) etatJoueur = joueur.GetComponent<EtatArpenteur>();
                if (jauge == null) jauge = joueur.GetComponent<JaugeBruit>();
                if (apparence == null) apparence = joueur.GetComponent<ApparenceArpenteur>();
                corpsJoueur = joueur.GetComponent<Rigidbody>();
                departPosition = joueur.transform.position;
                departRotation = joueur.transform.rotation;
            }
            if (cameraOrbitale == null) cameraOrbitale = FindFirstObjectByType<CameraOrbitale>();
            if (cameraNavette == null && cameraOrbitale != null) cameraNavette = cameraOrbitale.GetComponent<CameraNavette>();
            cam = cameraOrbitale != null ? cameraOrbitale.GetComponent<Camera>() : Camera.main;
            if (trouNoir == null)
            {
                var tn = FindFirstObjectByType<Rendu.TrouNoir>();
                if (tn != null) trouNoir = tn.transform;
            }
            // l'affichage provisoire du temps est remplacé par l'interface
            var provisoire = FindFirstObjectByType<AffichageTemps>();
            if (provisoire != null) provisoire.enabled = false;

            Reglages.Appliquer();
            CameraOrbitale.MultiplicateurSensibilite = Reglages.Sensibilite;
            CameraOrbitale.InverserY = Reglages.InverserY;
            Controle(false);
            if (cameraOrbitale != null) cameraOrbitale.enabled = false;
            if (cameraNavette != null) cameraNavette.enabled = false;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        void Start()
        {
            if (partieDemandee.HasValue)
            {
                var type = partieDemandee.Value;
                partieDemandee = null;
                Lancer(type);
            }
            else AllerAuMenu();
        }

        // ------------------------------------------------------------------ états

        void AllerAuMenu()
        {
            Etat = EtatJeu.Menu;
            LibererCurseur();
            interfaceJeu?.MontrerMenu();
        }

        public void OuvrirEnrolement(TypePartie type)
        {
            Type = type;
            Etat = EtatJeu.Enrolement;
            angleApercu = 0f;
            interfaceJeu?.MontrerEnrolement(type);
        }

        public void RetourMenu()
        {
            Etat = EtatJeu.Menu;
            interfaceJeu?.MontrerMenu();
        }

        /// <summary>Démarre une partie (après l'enrôlement, ou directement après un rechargement).</summary>
        public void Lancer(TypePartie type)
        {
            Type = type;
            var profil = ProfilArpenteur.Actuel;
            profil.enrole = true;
            profil.Sauver();
            if (apparence != null) apparence.Appliquer();

            if (joueur != null)
            {
                joueur.transform.SetPositionAndRotation(departPosition, departRotation);
                if (corpsJoueur != null) { corpsJoueur.linearVelocity = Vector3.zero; corpsJoueur.position = departPosition; }
            }
            if (jauge != null) jauge.Reinitialiser();
            horloges?.Reinitialiser();
            if (horloges != null) horloges.EnMarche = true;
            transmissions?.Demarrer(type == TypePartie.Mission);

            Etat = EtatJeu.Jeu;
            Controle(true);
            if (cameraOrbitale != null) cameraOrbitale.enabled = true;
            CapturerCurseur();
            interfaceJeu?.MontrerJeu(type);

            if (type == TypePartie.Didacticiel && didacticiel != null) didacticiel.Demarrer();
            else Invoke(nameof(PremierEcho), 1.5f);
        }

        void PremierEcho()
        {
            if (Etat == EtatJeu.Jeu) Transmissions.Echo("Arrivée", "Le sable est tiède. Au-dessus de vous, la lumière tourne autour de rien.");
        }

        public void Pause()
        {
            if (Etat != EtatJeu.Jeu && Etat != EtatJeu.Vol) return;
            etatAvantPause = Etat;
            Etat = EtatJeu.Pause;
            Time.timeScale = 0f;
            Controle(false);
            if (cameraOrbitale != null) cameraOrbitale.enabled = false;
            if (cameraNavette != null) cameraNavette.enabled = false;
            LibererCurseur();
            interfaceJeu?.MontrerPause(Type == TypePartie.Didacticiel);
        }

        public void Reprendre()
        {
            if (Etat != EtatJeu.Pause) return;
            Etat = etatAvantPause;
            Time.timeScale = 1f;
            if (Etat == EtatJeu.Vol) { if (cameraNavette != null) cameraNavette.enabled = true; }
            else
            {
                Controle(true);
                if (cameraOrbitale != null) cameraOrbitale.enabled = true;
            }
            CapturerCurseur();
            interfaceJeu?.MontrerJeu(Type);
        }

        public void EntrerVol(Navette n)
        {
            NavetteEnVol = n;
            Etat = EtatJeu.Vol;
            Controle(false);
            if (joueur != null) joueur.gameObject.SetActive(false);
            if (cameraOrbitale != null) cameraOrbitale.enabled = false;
            if (cameraNavette != null)
            {
                cameraNavette.cible = n.transform;
                cameraNavette.enabled = true;
                cameraNavette.Recaler();
            }
            interfaceJeu?.ModeVol(true);
        }

        /// <summary>Fin du vol : l'Arpenteur réapparaît au sol à côté de la navette.</summary>
        public void SortirVol(Vector3 pointSol, Vector3 avant)
        {
            NavetteEnVol = null;
            if (joueur != null)
            {
                Vector3 haut = Planete.Haut(pointSol);
                Vector3 position = pointSol + haut * (joueur.demiHauteur + 0.1f);
                joueur.transform.SetPositionAndRotation(position, Planete.Debout(position, avant));
                joueur.gameObject.SetActive(true);
                if (corpsJoueur != null) { corpsJoueur.position = position; corpsJoueur.linearVelocity = Vector3.zero; }
            }
            if (cameraNavette != null) cameraNavette.enabled = false;
            if (cameraOrbitale != null) cameraOrbitale.enabled = true;
            Etat = EtatJeu.Jeu;
            Controle(true);
            interfaceJeu?.ModeVol(false);
        }

        /// <summary>Remontée vers le vaisseau-mère (balise à pied ou navette arrimée).</summary>
        public void Extraction()
        {
            if (Etat != EtatJeu.Jeu && Etat != EtatJeu.Vol) return;
            Etat = EtatJeu.Extraction;
            tExtraction = 0f;
            Controle(false);
            if (corpsJoueur != null && NavetteEnVol == null) corpsJoueur.isKinematic = true;   // aspiré par le faisceau, sans gravité
            Ruche.Instance?.Annuler();
            Transmissions.Envoyer("Vaisseau-mère", "Signal capté. Remontée vers le vaisseau-mère.", duree: 4f);
        }

        public void TerminerMission()
        {
            Etat = EtatJeu.Fin;
            if (horloges != null) horloges.EnMarche = false;
            Controle(false);
            if (cameraOrbitale != null) cameraOrbitale.enabled = false;
            if (cameraNavette != null) cameraNavette.enabled = false;
            LibererCurseur();
            interfaceJeu?.MontrerFinMission();
        }

        public void TerminerFormation(int reussies, int total)
        {
            Etat = EtatJeu.Fin;
            if (horloges != null) horloges.EnMarche = false;
            Controle(false);
            if (cameraOrbitale != null) cameraOrbitale.enabled = false;
            if (cameraNavette != null) cameraNavette.enabled = false;
            LibererCurseur();
            BanqueSons.Jouer(BanqueSons.Instance?.finFormation);
            interfaceJeu?.MontrerFinFormation(reussies, total);
        }

        // ------------------------------------------------------------------ rechargements

        public void Recommencer() { partieDemandee = Type; Recharger(); }
        public void LancerMission() { partieDemandee = TypePartie.Mission; Recharger(); }
        public void MenuPrincipal() { partieDemandee = null; Recharger(); }

        void Recharger()
        {
            Time.timeScale = 1f;
            var scene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
            if (scene.buildIndex < 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scene.path, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            SceneManager.LoadScene(scene.buildIndex);
        }

        public void Quitter()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ------------------------------------------------------------------ boucle

        void Update()
        {
            switch (Etat)
            {
                case EtatJeu.Jeu:
                case EtatJeu.Vol:
                    if (EntreesJeu.Pause()) { Pause(); return; }
                    if (EntreesJeu.Journal()) interfaceJeu?.BasculerJournal();
                    if (EntreesJeu.Son()) { Reglages.Son = !Reglages.Son; Reglages.Appliquer(); Reglages.Sauver(); }
                    if (Cursor.lockState != CursorLockMode.Locked && Etat == EtatJeu.Vol && Entrees.CapturerCurseur()) CapturerCurseur();
                    break;
                case EtatJeu.Pause:
                    if (EntreesJeu.Pause() && interfaceJeu != null && !interfaceJeu.SousEcranOuvert) Reprendre();
                    break;
                case EtatJeu.Extraction:
                    tExtraction += Time.deltaTime;
                    interfaceJeu?.Fondu(Color.white, Mathf.Clamp01((tExtraction - dureeExtraction * 0.4f) / (dureeExtraction * 0.6f)));
                    if (NavetteEnVol != null) NavetteEnVol.transform.position += Planete.Haut(NavetteEnVol.transform.position) * (Time.deltaTime * (6f + tExtraction * tExtraction * 14f));
                    else if (joueur != null) joueur.transform.position += Planete.Haut(joueur.transform.position) * (Time.deltaTime * 4f);
                    if (tExtraction >= dureeExtraction) TerminerMission();
                    break;
            }
        }

        void LateUpdate()
        {
            if (cam == null || joueur == null) return;
            if (Etat == EtatJeu.Menu) PoseMenu();
            else if (Etat == EtatJeu.Enrolement) PoseApercu();
        }

        /// <summary>Caméra du menu : debout près de l'Arpenteur, le regard vers le trou noir.</summary>
        void PoseMenu()
        {
            Vector3 haut = Planete.Haut(departPosition);
            Vector3 cible = trouNoir != null ? trouNoir.position : departPosition + (departRotation * Vector3.forward) * 100f + haut * 30f;
            Vector3 vers = (cible - departPosition).normalized;
            Vector3 plat = Vector3.ProjectOnPlane(vers, haut).normalized;
            if (plat.sqrMagnitude < 0.01f) plat = Vector3.ProjectOnPlane(departRotation * Vector3.forward, haut).normalized;
            float balancement = Mathf.Sin(Time.unscaledTime * 0.07f) * 12f;
            Vector3 position = departPosition - plat * 6f + haut * 2.4f;
            cam.transform.position = position;
            Vector3 regard = Quaternion.AngleAxis(balancement, haut) * Vector3.Slerp(plat, vers, 0.75f);
            cam.transform.rotation = Quaternion.LookRotation(regard, haut);
        }

        /// <summary>Caméra de l'enrôlement : l'Arpenteur à droite de l'écran, qui tourne lentement.</summary>
        void PoseApercu()
        {
            Vector3 haut = Planete.Haut(joueur.transform.position);
            if (interfaceJeu == null || !interfaceJeu.GlisserApercu(out float glisse)) angleApercu += Time.unscaledDeltaTime * 25f;
            else angleApercu += glisse;
            Quaternion face = Planete.Debout(departPosition, departRotation * Vector3.forward);
            joueur.transform.rotation = Quaternion.AngleAxis(angleApercu, haut) * face;

            Vector3 avant = face * Vector3.forward, droite = face * Vector3.right;
            Vector3 centre = joueur.transform.position + haut * 0.1f;
            float decalage = cam.aspect > 1.15f ? 1.3f : 0f;
            cam.transform.position = centre + avant * 4.2f + haut * 0.55f;
            cam.transform.rotation = Quaternion.LookRotation(centre - droite * decalage - cam.transform.position, haut);
        }

        // ------------------------------------------------------------------ outils

        void Controle(bool actif)
        {
            if (joueur == null) return;
            joueur.enabled = actif;
            if (!actif && corpsJoueur != null && !corpsJoueur.isKinematic && joueur.gameObject.activeInHierarchy)
                corpsJoueur.linearVelocity = Vector3.Project(corpsJoueur.linearVelocity, Planete.Haut(joueur.transform.position));
        }

        static void CapturerCurseur()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        static void LibererCurseur()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
