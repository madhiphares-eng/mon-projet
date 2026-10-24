using System;
using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Combat
{
    /// <summary>Visée depuis le centre de l'écran (réticule), sans toucher l'Arpenteur lui-même.</summary>
    public static class Visee
    {
        static readonly RaycastHit[] touches = new RaycastHit[32];

        static Transform Joueur => GestionnaireJeu.Instance != null && GestionnaireJeu.Instance.joueur != null ? GestionnaireJeu.Instance.joueur.transform : null;

        public static Ray RayonCamera()
        {
            var cam = Camera.main;
            return cam != null ? new Ray(cam.transform.position, cam.transform.forward) : new Ray(Vector3.zero, Vector3.forward);
        }

        /// <summary>Premier obstacle le long d'un rayon (ignore les déclencheurs, l'Arpenteur et <paramref name="ignorer"/>).</summary>
        public static bool Rayon(Vector3 origine, Vector3 direction, float portee, out RaycastHit touche, Transform ignorer = null)
        {
            int n = Physics.RaycastNonAlloc(origine, direction, touches, portee, ~0, QueryTriggerInteraction.Ignore);
            int meilleur = -1;
            var joueur = Joueur;
            for (int i = 0; i < n; i++)
            {
                var t = touches[i].transform;
                if (joueur != null && t.IsChildOf(joueur)) continue;
                if (ignorer != null && t.IsChildOf(ignorer)) continue;
                if (meilleur < 0 || touches[i].distance < touches[meilleur].distance) meilleur = i;
            }
            touche = meilleur >= 0 ? touches[meilleur] : default;
            return meilleur >= 0;
        }

        /// <summary>Tous les obstacles le long d'un rayon, triés par distance (rayon qui traverse).</summary>
        public static int Traverser(Vector3 origine, Vector3 direction, float portee, RaycastHit[] resultats)
        {
            int n = Physics.RaycastNonAlloc(origine, direction, touches, portee, ~0, QueryTriggerInteraction.Ignore);
            var joueur = Joueur;
            int k = 0;
            for (int i = 0; i < n && k < resultats.Length; i++)
                if (joueur == null || !touches[i].transform.IsChildOf(joueur)) resultats[k++] = touches[i];
            Array.Sort(resultats, 0, k, ParDistance.Instance);
            return k;
        }

        sealed class ParDistance : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly ParDistance Instance = new ParDistance();
            public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
        }

        /// <summary>Point du sol visé au centre de l'écran (ou au bout de la portée).</summary>
        public static Vector3 PointAuSol(float portee)
        {
            var r = RayonCamera();
            Vector3 p = Rayon(r.origin, r.direction, portee, out RaycastHit h) ? h.point : r.origin + r.direction * portee;
            return Planete.Sol(p + Planete.Haut(p) * 2f, out RaycastHit sol) ? sol.point : p;
        }
    }

    /// <summary>
    /// Les armes de l'Arpenteur : sélection (1 à 6, molette, croix), tir selon le mode de chaque arme,
    /// visée (clic droit), rechargement (R), coup rapide (V), grenade rapide (G), munitions et ravitaillement
    /// près de la navette. Chaque tir fait du bruit : la Ruche et les créatures l'entendent.
    /// </summary>
    [RequireComponent(typeof(EtatArpenteur))]
    public class Arsenal : MonoBehaviour
    {
        public static Arsenal Instance { get; private set; }

        [Tooltip("Modèles des armes dans l'ordre du catalogue (pic, Ardent, Faucheuse, Grêle, Sablier, Écho). Vide = forme générée.")]
        public GameObject[] modeles = new GameObject[6];
        [Tooltip("Canon des modèles vers -Z (Kenney Blaster Kit importé par glTFast). Décocher si les armes tirent à l'envers.")]
        public bool canonVersMoinsZ = true;
        [Tooltip("Assombrit les blasters Kenney (jouets colorés) en métal, pour un rendu plus réaliste.")]
        public bool teinteMetal = true;
        [Tooltip("Distance de la navette en dessous de laquelle les munitions se rechargent.")]
        public float distanceRavitaillement = 14f;

        /// <summary>Un tir est parti (pour le didacticiel et l'interface).</summary>
        public static event Action<DefinitionArme> Tir;
        public static event Action<DefinitionArme> Changement;

        public int Index { get; private set; } = 1;
        public DefinitionArme Arme => Catalogue.Armes[Index];
        public int Chargeur => chargeurs != null ? chargeurs[Index] : 0;
        public int Reserve => reserves != null ? reserves[Index] : 0;
        public bool Vise { get; private set; }
        public bool EnRechargement => tRecharge >= 0f;
        public float ProgressionRechargement => EnRechargement ? Mathf.Clamp01(tRecharge / dureeRecharge) : 0f;
        public float Charge { get; private set; }
        /// <summary>Dispersion actuelle en degrés (taille du réticule).</summary>
        public float Dispersion { get; private set; }
        public float DernierTir { get; private set; } = -100f;
        public bool Occupe => tChangement >= 0f || tMelee >= 0f || tLancer >= 0f;
        public int Grenades => reserves != null ? reserves[IndexGrenade] + chargeurs[IndexGrenade] : 0;

        static readonly int IndexGrenade = Catalogue.Index("echo");
        static readonly int IndexPic = Catalogue.Index("pic");
        static readonly RaycastHit[] traversees = new RaycastHit[16];

        EtatArpenteur etat;
        VueArme vue;
        int[] chargeurs, reserves;
        float prochainTir, tRecharge = -1f, dureeRecharge = 1f, floraison;
        float tChangement = -1f, tMelee = -1f, tLancer = -1f, prochainRavitaillement;
        int indexSuivant;
        bool chargeEnCours, meleeAvecPic, lancerParti, frappe, auxAbords;

        void Awake()
        {
            etat = GetComponent<EtatArpenteur>();
            Instance = this;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        void Start()
        {
            int n = Catalogue.Armes.Length;
            chargeurs = new int[n];
            reserves = new int[n];
            RemplirTout();
            vue = GetComponent<VueArme>();
            if (vue == null) vue = gameObject.AddComponent<VueArme>();
            vue.Preparer(this);
            vue.Montrer(Index);
        }

        public void RemplirTout()
        {
            if (chargeurs == null) return;
            for (int i = 0; i < Catalogue.Armes.Length; i++)
            {
                chargeurs[i] = ChargeurMax(Catalogue.Armes[i]);
                reserves[i] = ReserveMax(Catalogue.Armes[i]);
            }
        }

        public int ChargeurMax(DefinitionArme d)
        {
            if (d.mode == ModeTir.Melee || d.mode == ModeTir.Lancer) return d.chargeur;
            return Mathf.RoundToInt(d.chargeur * (ProfilArpenteur.Actuel.A(Talent.ChargeursEtendus) ? 1.5f : 1f));
        }

        public int ReserveMax(DefinitionArme d)
        {
            if (d.mode == ModeTir.Melee) return 0;
            if (d.mode == ModeTir.Lancer) return d.reserveMax - d.chargeur + (ProfilArpenteur.Actuel.A(Talent.Artificier) ? 2 : 0);
            return Mathf.RoundToInt(d.reserveMax * (ProfilArpenteur.Actuel.A(Talent.ChargeursEtendus) ? 1.25f : 1f));
        }

        /// <summary>Ajoute une part des munitions maximales à toutes les armes (cellule d'énergie).</summary>
        public void AjouterMunitions(float part)
        {
            for (int i = 0; i < Catalogue.Armes.Length; i++)
            {
                var d = Catalogue.Armes[i];
                if (d.mode == ModeTir.Melee) continue;
                int max = ReserveMax(d);
                reserves[i] = Mathf.Min(max, reserves[i] + Mathf.Max(1, Mathf.RoundToInt(max * part)));
            }
        }

        float Rythme
        {
            get
            {
                bool chrono = ProfilArpenteur.Actuel.A(Talent.Chronophage) && BulleTemporelle.PresenceLente(transform.position) > 0.3f;
                return chrono ? 1.3f : 1f;
            }
        }

        // ------------------------------------------------------------------ boucle

        void Update()
        {
            if (chargeurs == null) return;
            var jeu = GestionnaireJeu.Instance;
            bool actif = jeu != null && jeu.Etat == EtatJeu.Jeu && etat.Actif && Cursor.lockState == CursorLockMode.Locked;
            float dt = Time.deltaTime;
            floraison = Mathf.MoveTowards(floraison, 0f, dt * 6f);

            Vise = actif && EntreesJeu.Viser() && !etat.Course && Arme.mode != ModeTir.Melee;
            CameraOrbitale.ViseeCible = Vise ? 1f : 0f;
            Dispersion = CalculerDispersion();

            if (actif) Ravitailler(jeu);
            Sequences(dt);
            if (!actif) { chargeEnCours = false; Charge = 0f; return; }

            // sélection
            int choix = EntreesJeu.ChoixArme();
            if (choix >= 0 && choix < Catalogue.Armes.Length) Changer(choix);
            int pas = EntreesJeu.DefilementArme();
            if (pas != 0) Changer((Index + pas + Catalogue.Armes.Length) % Catalogue.Armes.Length);

            if (Occupe) return;
            if (EntreesJeu.Grenade() && Index != IndexGrenade) { LancerGrenade(false); return; }
            if (EntreesJeu.Melee() && Index != IndexPic) { CoupRapide(); return; }
            if (EntreesJeu.Recharger()) Recharger();
            if (EnRechargement) return;

            var d = Arme;
            switch (d.mode)
            {
                case ModeTir.Coup:
                case ModeTir.Eclats:
                    if (EntreesJeu.TirerAppui()) Presser(d);
                    break;
                case ModeTir.Rafale:
                    if (EntreesJeu.Tirer()) Presser(d, false);
                    break;
                case ModeTir.Charge:
                    if (EntreesJeu.Tirer() && Chargeur > 0 && Time.time >= prochainTir)
                    {
                        if (!chargeEnCours) BanqueSons.Jouer(BanqueSons.Instance?.chargeSablier, 0.5f);
                        chargeEnCours = true;
                        Charge = Mathf.Min(1f, Charge + dt * Rythme / d.tempsCharge);
                    }
                    else if (chargeEnCours)
                    {
                        if (Charge >= 0.35f) Tirer(d, Charge);
                        chargeEnCours = false;
                        Charge = 0f;
                    }
                    else if (EntreesJeu.TirerAppui() && Chargeur <= 0) Vide(d);
                    break;
                case ModeTir.Lancer:
                    if (EntreesJeu.TirerAppui()) LancerGrenade(true);
                    break;
                case ModeTir.Melee:
                    if (EntreesJeu.Tirer() && Time.time >= prochainTir) CoupDePic(d);
                    break;
            }
        }

        float CalculerDispersion()
        {
            var d = Arme;
            float s = d.dispersion * (Vise ? 0.45f : 1f);
            if (etat.Bouge) s *= etat.Course ? 2.2f : 1.4f;
            if (!etat.AuSol) s *= 2f;
            s += floraison;
            if (ProfilArpenteur.Actuel.A(Talent.MainSure)) s *= 0.6f;
            return s;
        }

        void Presser(DefinitionArme d, bool appui = true)
        {
            if (Time.time < prochainTir) return;
            if (Chargeur <= 0) { if (appui || Time.time >= prochainTir) Vide(d); return; }
            Tirer(d, 1f);
        }

        void Vide(DefinitionArme d)
        {
            prochainTir = Time.time + 0.25f;
            BanqueSons.Jouer(BanqueSons.Instance?.vide, 0.5f);
            if (Reserve > 0) Recharger();
        }

        // ------------------------------------------------------------------ tir

        void Tirer(DefinitionArme d, float puissance)
        {
            chargeurs[Index]--;
            prochainTir = Time.time + 1f / (d.cadence * Rythme);
            DernierTir = Time.time;
            Vector3 bouche = vue.Bouche;
            var rayon = Visee.RayonCamera();
            // le rayon part de la caméra, mais pas derrière l'Arpenteur
            Vector3 origine = rayon.origin + rayon.direction * Mathf.Max(0f, Vector3.Dot(bouche - rayon.origin, rayon.direction) - 0.5f);
            bool mainSure = ProfilArpenteur.Actuel.A(Talent.MainSure);

            for (int p = 0; p < d.projectiles; p++)
            {
                Vector3 dir = Disperser(rayon.direction, Dispersion);
                if (d.mode == ModeTir.Charge) { TirerRayon(d, origine, dir, bouche, puissance); continue; }
                bool toucheDecor = Visee.Rayon(origine, dir, d.portee, out RaycastHit h);
                Vector3 fin = toucheDecor ? h.point : origine + dir * d.portee;
                if (d.vitesseProjectile > 0f)
                {
                    Projectile.Tirer(bouche, (fin - bouche).normalized, d.vitesseProjectile, d.degats, d.portee, d.couleur, true, transform, d.reculCible);
                    continue;
                }
                EffetsCombat.Tracante(bouche, fin, d.couleur, d.mode == ModeTir.Eclats ? 0.025f : 0.035f, 0.07f);
                if (!toucheDecor) continue;
                var cible = Combat.De(h.collider);
                if (cible != null) Combat.Frapper(cible, new Degats(d.degats, h.point, dir, d.reculCible, true, d.ralenti));
                EffetsCombat.Impact(h.point, h.normal, d.couleur, cible != null ? 6 : 3, 0.8f);
            }

            float bruit = d.bruit * (d.mode == ModeTir.Charge ? puissance : 1f);
            GestionnaireJeu.Instance?.jauge?.Ajouter(bruit);
            float recul = d.recul * (mainSure ? 0.6f : 1f) * (Vise ? 0.7f : 1f) * (d.mode == ModeTir.Charge ? puissance : 1f);
            CameraOrbitale.Recul(recul, UnityEngine.Random.Range(-0.35f, 0.35f) * recul);
            vue.Recul(recul);
            EffetsCombat.Eclair(bouche, rayon.direction, d.couleur, d.mode == ModeTir.Eclats ? 1.5f : d.mode == ModeTir.Charge ? 1.3f : 1f);
            BanqueSons.Jouer(SonTir(d), d.mode == ModeTir.Rafale ? 0.45f : 0.7f);
            floraison = Mathf.Min(floraison + d.floraison, 6f);
            Tir?.Invoke(d);
            if (Chargeur <= 0 && Reserve > 0) Invoke(nameof(Recharger), 0.2f);
        }

        void TirerRayon(DefinitionArme d, Vector3 origine, Vector3 dir, Vector3 bouche, float puissance)
        {
            int n = Visee.Traverser(origine, dir, d.portee, traversees);
            Vector3 fin = origine + dir * d.portee;
            int touchees = 0;
            for (int i = 0; i < n; i++)
            {
                var h = traversees[i];
                var cible = Combat.De(h.collider);
                if (cible == null) { fin = h.point; EffetsCombat.Impact(h.point, h.normal, d.couleur, 10, 1.2f); break; }
                if (!cible.Vivante) continue;
                Combat.Frapper(cible, new Degats(d.degats * puissance, h.point, dir, d.reculCible, true, d.ralenti * puissance));
                EffetsCombat.Impact(h.point, -dir, d.couleur, 12, 1.2f);
                if (++touchees >= d.traversee) { fin = h.point; break; }
            }
            EffetsCombat.Rayon(bouche, fin, d.couleur * (0.6f + 0.4f * puissance));
        }

        static Vector3 Disperser(Vector3 direction, float degres)
        {
            if (degres <= 0.001f) return direction;
            Vector2 c = UnityEngine.Random.insideUnitCircle * Mathf.Tan(degres * Mathf.Deg2Rad);
            Vector3 droite = Vector3.Cross(direction, Vector3.up);
            if (droite.sqrMagnitude < 1e-4f) droite = Vector3.Cross(direction, Vector3.right);
            droite.Normalize();
            Vector3 haut = Vector3.Cross(droite, direction);
            return (direction + droite * c.x + haut * c.y).normalized;
        }

        AudioClip SonTir(DefinitionArme d)
        {
            var s = BanqueSons.Instance;
            if (s == null) return null;
            switch (d.id)
            {
                case "ardent": return s.tirPistolet;
                case "faucheuse": return s.tirCarabine;
                case "grele": return s.tirEclats;
                case "sablier": return s.tirSablier;
                default: return s.tirPistolet;
            }
        }

        // ------------------------------------------------------------------ corps à corps, grenades

        void CoupDePic(DefinitionArme d)
        {
            prochainTir = Time.time + 1f / d.cadence;
            tMelee = 0f;
            meleeAvecPic = true;
            frappe = false;
            vue.Melee(true);
            BanqueSons.Jouer(BanqueSons.Instance?.balayage, 0.5f);
        }

        void CoupRapide()
        {
            tMelee = 0f;
            meleeAvecPic = false;
            frappe = false;
            vue.Melee(false);
            BanqueSons.Jouer(BanqueSons.Instance?.balayage, 0.4f);
        }

        void Frapper()
        {
            var d = Catalogue.Armes[IndexPic];
            bool coupDePic = ProfilArpenteur.Actuel.A(Talent.CoupDePic);
            float degats = d.degats * (meleeAvecPic ? 1f : 0.7f) * (coupDePic && meleeAvecPic ? 1.6f : 1f);
            Vector3 haut = etat.Haut;
            Vector3 avant = Camera.main != null ? Vector3.ProjectOnPlane(Camera.main.transform.forward, haut).normalized : transform.forward;
            bool touche = false;
            for (int i = Combat.Hostiles.Count - 1; i >= 0; i--)
            {
                if (i >= Combat.Hostiles.Count) continue;
                var c = Combat.Hostiles[i];
                if (c == null || !c.Vivante) continue;
                Vector3 ecart = c.Centre - transform.position;
                if (ecart.magnitude > d.portee + 0.9f) continue;
                if (Vector3.Dot(Vector3.ProjectOnPlane(ecart, haut).normalized, avant) < 0.3f) continue;
                Combat.Frapper(c, new Degats(degats, c.Centre, ecart.normalized, d.reculCible, true));
                EffetsCombat.Impact(c.Centre, -ecart.normalized, d.couleur, 10, 1f);
                touche = true;
                if (!c.Vivante && coupDePic && meleeAvecPic) GetComponent<VieArpenteur>()?.Soigner(10f);
            }
            if (touche)
            {
                BanqueSons.Jouer(BanqueSons.Instance?.impactMelee, 0.8f);
                CameraSecousse.Choc(0.2f);
            }
            GestionnaireJeu.Instance?.jauge?.Ajouter(d.bruit);
        }

        void LancerGrenade(bool armeEquipee)
        {
            if (Grenades <= 0) { BanqueSons.Jouer(BanqueSons.Instance?.vide, 0.5f); return; }
            if (Time.time < prochainTir && armeEquipee) return;
            tLancer = 0f;
            lancerParti = false;
            vue.Lancer();
        }

        void Sequences(float dt)
        {
            if (tChangement >= 0f)
            {
                float avant = tChangement;
                tChangement += dt;
                if (avant < 0.22f && tChangement >= 0.22f)
                {
                    Index = indexSuivant;
                    vue.Montrer(Index);
                    Changement?.Invoke(Arme);
                }
                if (tChangement >= 0.45f) tChangement = -1f;
            }
            if (tMelee >= 0f)
            {
                tMelee += dt;
                if (!frappe && tMelee >= 0.13f) { frappe = true; Frapper(); }
                if (tMelee >= 0.42f) tMelee = -1f;
            }
            if (tLancer >= 0f)
            {
                tLancer += dt;
                if (!lancerParti && tLancer >= 0.24f) { lancerParti = true; Lacher(); }
                if (tLancer >= 0.55f) tLancer = -1f;
            }
            if (tRecharge >= 0f)
            {
                tRecharge += dt * Rythme;
                if (tRecharge >= dureeRecharge) FinRechargement();
            }
        }

        void Lacher()
        {
            var d = Catalogue.Armes[IndexGrenade];
            if (chargeurs[IndexGrenade] > 0) chargeurs[IndexGrenade]--;
            else if (reserves[IndexGrenade] > 0) reserves[IndexGrenade]--;
            else return;
            if (chargeurs[IndexGrenade] == 0 && reserves[IndexGrenade] > 0) { reserves[IndexGrenade]--; chargeurs[IndexGrenade] = 1; }
            Vector3 haut = etat.Haut;
            var r = Visee.RayonCamera();
            Vector3 depart = vue.MainLancer;
            Vector3 dir = (r.direction + haut * 0.28f).normalized;
            var corps = GetComponent<Rigidbody>();
            Vector3 v = dir * 17f + (corps != null ? corps.linearVelocity * 0.5f : Vector3.zero);
            float bonus = ProfilArpenteur.Actuel.A(Talent.Artificier) ? 1.3f : 1f;
            Grenade.Lancer(depart, v, d, IndexGrenade < modeles.Length ? modeles[IndexGrenade] : null, transform, bonus);
            if (Index == IndexGrenade && Grenades <= 0) Invoke(nameof(ReprendrePistolet), 0.35f);
            BanqueSons.Jouer(BanqueSons.Instance?.balayage, 0.6f);
            GestionnaireJeu.Instance?.jauge?.Ajouter(d.bruit);
            prochainTir = Time.time + 1f / d.cadence;
            DernierTir = Time.time;
            Tir?.Invoke(d);
        }

        // ------------------------------------------------------------------ changement, rechargement, ravitaillement

        void ReprendrePistolet() => Changer(Catalogue.Index("ardent"));

        public void Changer(int i)
        {
            if (i == Index && tChangement < 0f) return;
            if (i == IndexGrenade && Grenades <= 0) { BanqueSons.Jouer(BanqueSons.Instance?.vide, 0.5f); return; }
            AnnulerRechargement();
            chargeEnCours = false;
            Charge = 0f;
            indexSuivant = i;
            tChangement = 0f;
            vue.Changer();
            BanqueSons.Jouer(BanqueSons.Instance?.changementArme, 0.6f);
        }

        public void Recharger()
        {
            var d = Arme;
            if (EnRechargement || d.mode == ModeTir.Melee || d.mode == ModeTir.Lancer) return;
            if (Chargeur >= ChargeurMax(d) || Reserve <= 0) return;
            dureeRecharge = d.rechargement * (ProfilArpenteur.Actuel.A(Talent.RechargementEclair) ? 0.65f : 1f);
            tRecharge = 0f;
            chargeEnCours = false;
            Charge = 0f;
            vue.Recharger(dureeRecharge);
            BanqueSons.Jouer(BanqueSons.Instance?.rechargement, 0.6f);
        }

        void FinRechargement()
        {
            tRecharge = -1f;
            var d = Arme;
            int manque = ChargeurMax(d) - Chargeur;
            int pris = Mathf.Min(manque, Reserve);
            chargeurs[Index] += pris;
            reserves[Index] -= pris;
            BanqueSons.Jouer(BanqueSons.Instance?.rechargementFin, 0.6f);
        }

        void AnnulerRechargement()
        {
            if (tRecharge < 0f) return;
            tRecharge = -1f;
            vue.Recharger(0f);
        }

        void Ravitailler(GestionnaireJeu jeu)
        {
            var n = jeu.navette;
            bool proche = n != null && !n.EnManoeuvre && Vector3.Distance(transform.position, n.transform.position) < distanceRavitaillement;
            if (proche && !auxAbords && Manque())
                Transmissions.Envoyer("Navette", "Ravitaillement en munitions et en grenades à proximité de la navette.", duree: 4f, noter: false);
            auxAbords = proche;
            if (!proche || Time.time < prochainRavitaillement || !Manque()) return;
            prochainRavitaillement = Time.time + 1.2f;
            AjouterMunitions(0.25f);
            if (Chargeur == 0 && Arme.mode == ModeTir.Lancer && reserves[Index] > 0) { reserves[Index]--; chargeurs[Index] = 1; }
            EffetsCombat.Etincelle(transform.position, new Color(0.4f, 1.6f, 2.2f), 0.2f);
        }

        bool Manque()
        {
            for (int i = 0; i < Catalogue.Armes.Length; i++)
                if (Catalogue.Armes[i].mode != ModeTir.Melee && reserves[i] < ReserveMax(Catalogue.Armes[i])) return true;
            return false;
        }
    }
}
