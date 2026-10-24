using System;
using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Combat
{
    /// <summary>
    /// Intégrité de la combinaison de l'Arpenteur. Se répare seule après quelques secondes sans coup.
    /// Le bouclier (compétence) encaisse en premier. À zéro : rapatriement d'urgence près de la navette,
    /// avec la moitié de l'Éther perdue.
    /// </summary>
    [RequireComponent(typeof(EtatArpenteur))]
    public class VieArpenteur : MonoBehaviour, ICible
    {
        public float integriteBase = 100f;
        [Tooltip("Secondes sans dégâts avant la réparation automatique.")]
        public float delaiReparation = 5f;
        public float vitesseReparation = 4f;
        public float dureeRapatriement = 2.6f;

        /// <summary>Dégâts reçus (quantité après bouclier, position de la source).</summary>
        public static event Action<float, Vector3> Blesse;
        public static event Action Effondre;

        public float Integrite { get; private set; }
        public float IntegriteMax => integriteBase + (ProfilArpenteur.Actuel.A(Talent.CombinaisonRenforcee) ? 40f : 0f);
        public float Bouclier { get; private set; }
        public float BouclierMax { get; private set; }
        public bool Vivante => Integrite > 0f;
        public Vector3 Centre => transform.position;
        public bool EnRapatriement => tRapatriement >= 0f;
        /// <summary>Temps depuis le dernier coup (pour l'interface).</summary>
        public float DepuisCoup => Time.time - dernierCoup;

        float dernierCoup = -100f, finBouclier, tRapatriement = -1f;
        EtatArpenteur etat;
        Rigidbody corps;

        void Awake()
        {
            etat = GetComponent<EtatArpenteur>();
            corps = GetComponent<Rigidbody>();
            Integrite = IntegriteMax;
        }

        public void Reinitialiser()
        {
            Integrite = IntegriteMax;
            Bouclier = 0f;
            tRapatriement = -1f;
        }

        /// <summary>Active le bouclier d'Éther.</summary>
        public void ActiverBouclier(float quantite, float duree)
        {
            Bouclier = BouclierMax = quantite;
            finBouclier = Time.time + duree;
        }

        public void Soigner(float quantite)
        {
            if (!Vivante) return;
            Integrite = Mathf.Min(IntegriteMax, Integrite + quantite);
        }

        public void Subir(Degats d)
        {
            var jeu = GestionnaireJeu.Instance;
            if (jeu == null || jeu.Etat != EtatJeu.Jeu || !Vivante || EnRapatriement) return;
            float q = d.quantite;
            if (Bouclier > 0f)
            {
                float absorbe = Mathf.Min(Bouclier, q);
                Bouclier -= absorbe;
                q -= absorbe;
                EffetsCombat.Impact(d.point, -d.direction, new Color(0.4f, 1.8f, 2.4f), 6, 0.6f);
            }
            dernierCoup = Time.time;
            if (d.recul > 0f && corps != null && !corps.isKinematic && jeu.joueur != null)
                jeu.joueur.Propulser(Vector3.ProjectOnPlane(d.direction, Planete.Haut(transform.position)).normalized * d.recul + Planete.Haut(transform.position) * d.recul * 0.25f);
            if (q <= 0f) return;
            Integrite = Mathf.Max(0f, Integrite - q);
            CameraSecousse.Choc(Mathf.Clamp01(q / 30f) * 0.6f + 0.15f);
            jeu.interfaceJeu?.Impact();
            BanqueSons.Jouer(BanqueSons.Instance?.blessure, Mathf.Clamp01(0.35f + q / 40f));
            Blesse?.Invoke(q, d.point - d.direction * 3f);
            if (Integrite <= 0f) Effondrement();
        }

        void Effondrement()
        {
            tRapatriement = 0f;
            Effondre?.Invoke();
            int perte = Mission.Instance != null ? Mission.Instance.PerdreMoitie() : 0;
            Transmissions.Envoyer("Alerte combinaison", perte > 0
                ? $"Intégrité nulle. Rapatriement d'urgence vers la navette. {perte} {(perte > 1 ? "cristaux perdus" : "cristal perdu")}."
                : "Intégrité nulle. Rapatriement d'urgence vers la navette.", duree: 7f);
            var jeu = GestionnaireJeu.Instance;
            if (jeu != null && jeu.joueur != null) jeu.joueur.enabled = false;
        }

        void Update()
        {
            var jeu = GestionnaireJeu.Instance;
            if (Bouclier > 0f && Time.time > finBouclier) Bouclier = 0f;

            if (EnRapatriement)
            {
                tRapatriement += Time.deltaTime;
                float k = tRapatriement / dureeRapatriement;
                jeu?.interfaceJeu?.Fondu(Color.black, Mathf.Clamp01(k < 0.5f ? k * 2.4f : (1f - k) * 2.4f));
                if (k >= 0.5f && k - Time.deltaTime / dureeRapatriement < 0.5f) Rapatrier(jeu);
                if (k >= 1f)
                {
                    tRapatriement = -1f;
                    jeu?.interfaceJeu?.Fondu(Color.black, 0f);
                    if (jeu != null && jeu.joueur != null && jeu.Etat == EtatJeu.Jeu) jeu.joueur.enabled = true;
                }
                return;
            }

            if (jeu == null || jeu.Etat != EtatJeu.Jeu || !Vivante) return;
            bool regen = ProfilArpenteur.Actuel.A(Talent.Regeneration);
            float delai = regen ? delaiReparation * 0.5f : delaiReparation;
            if (Time.time - dernierCoup > delai && Integrite < IntegriteMax)
                Integrite = Mathf.Min(IntegriteMax, Integrite + vitesseReparation * (regen ? 3f : 1f) * Time.deltaTime);
        }

        void Rapatrier(GestionnaireJeu jeu)
        {
            Integrite = IntegriteMax * 0.6f;
            Bouclier = 0f;
            if (jeu == null || jeu.joueur == null) return;
            Vector3 cible = jeu.navette != null ? jeu.navette.transform.position + jeu.navette.transform.right * 8f : transform.position;
            if (Planete.Sol(cible, out RaycastHit sol, jeu.navette != null ? jeu.navette.transform : null)) cible = sol.point;
            Vector3 position = cible + Planete.Haut(cible) * (jeu.joueur.demiHauteur + 0.2f);
            transform.SetPositionAndRotation(position, Planete.Debout(position, transform.forward));
            if (corps != null) { corps.position = position; corps.linearVelocity = Vector3.zero; }
            if (jeu.jauge != null) jeu.jauge.ApresAttaque(0f, 8f);
            EffetsCombat.Anneau(cible, Planete.Haut(cible), 4f, new Color(0.4f, 1.8f, 2.4f), 0.8f);
        }
    }
}
