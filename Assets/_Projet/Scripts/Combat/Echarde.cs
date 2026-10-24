using System;
using System.Collections.Generic;
using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Combat
{
    public enum TypeEcharde { Rodeuse, Cracheuse, Colosse }

    /// <summary>
    /// Écharde : créature de cristal née de l'Éther, qui garde les gisements. Construite et animée par le code :
    /// six pattes qui cherchent le sol et marchent en trépied, carapace qui respire, mandibules, éclats lumineux
    /// sur le dos. Elle entend le bruit (jauge, tirs, explosions), voit de près, et se jette sur le leurre holographique.
    /// Rôdeuse : bond et morsure. Cracheuse : garde ses distances et crache des éclats. Colosse : charge et frappe le sol.
    /// Ralentie dans les bulles de temps lent et par le fusil Sablier. Le ver l'avale comme le reste.
    /// </summary>
    public class Echarde : MonoBehaviour, ICible
    {
        public static readonly List<Echarde> Toutes = new List<Echarde>();
        /// <summary>Une créature est morte (true : tuée par l'Arpenteur).</summary>
        public static event Action<Echarde, bool> Morte;
        /// <summary>Une créature a repéré l'Arpenteur.</summary>
        public static event Action<Echarde> Alertee;

        public TypeEcharde type;

        public float Vie { get; private set; }
        public float VieMax { get; private set; }
        public bool Vivante => Vie > 0f;
        public Vector3 Centre => corps != null ? corps.position : transform.position;
        public bool EnChasse => etatIA == Etat.Chasse || etatIA == Etat.Attaque;
        /// <summary>Secondes pendant lesquelles un repère la montre (scanner avec Écho profond).</summary>
        public float Revele { get; set; }
        public int Experience => type == TypeEcharde.Colosse ? 50 : type == TypeEcharde.Cracheuse ? 22 : 15;
        public string NomType => type == TypeEcharde.Colosse ? "Colosse d'Éther" : type == TypeEcharde.Cracheuse ? "Écharde cracheuse" : "Écharde rôdeuse";

        enum Etat { Emergence, Errance, Enquete, Chasse, Attaque, Charge, Mort }

        sealed class Patte
        {
            public Vector3 hanche;            // dans le repère du corps
            public Vector3 pied, depart, arrivee;
            public float t = -1f;
            public int groupe;
            public Transform cuisse, tibia, griffe;
        }

        // caractéristiques
        float taille, vitesseMarche, vitesseCourse, hauteurCorps, porteeAttaque, degats, cadence, ouie, vue, longueurPatte;
        Color couleur;

        // état
        Etat etatIA = Etat.Emergence;
        float tEtat, prochainCoup, ralentiFin, tPause, flash, tMort;
        Vector3 nid, pointCible, vitesse, recul, dernierePositionVue;
        Vector3 haut = Vector3.up, avant = Vector3.forward;
        bool coupPorte;

        // corps
        Transform corps, tete, mandibuleG, mandibuleD;
        Material materiauNoyau, materiauCarapace;
        readonly List<Patte> pattes = new List<Patte>();
        readonly List<Transform> eclatsDos = new List<Transform>();
        float respiration;
        static readonly RaycastHit[] vision = new RaycastHit[8];

        // ------------------------------------------------------------------ création

        /// <summary>Fait sortir une créature du sable à cet endroit.</summary>
        public static Echarde Creer(TypeEcharde type, Vector3 sol, Vector3 nid)
        {
            var go = new GameObject(type == TypeEcharde.Colosse ? "Colosse d'Éther" : "Écharde " + (type == TypeEcharde.Cracheuse ? "cracheuse" : "rôdeuse"));
            go.transform.position = sol;
            var e = go.AddComponent<Echarde>();
            e.type = type;
            e.nid = nid;
            e.Construire();
            return e;
        }

        void Construire()
        {
            switch (type)
            {
                case TypeEcharde.Cracheuse:
                    taille = 1.05f; vitesseMarche = 2f; vitesseCourse = 6.5f; porteeAttaque = 18f; degats = 10f; cadence = 2.2f;
                    ouie = 1.1f; vue = 24f; VieMax = 46f; couleur = new Color(1.2f, 2.2f, 0.5f);
                    break;
                case TypeEcharde.Colosse:
                    taille = 2.3f; vitesseMarche = 1.8f; vitesseCourse = 5f; porteeAttaque = 3.6f; degats = 26f; cadence = 3.2f;
                    ouie = 0.9f; vue = 16f; VieMax = 190f; couleur = new Color(1.6f, 0.7f, 2.6f);
                    break;
                default:
                    taille = 0.85f; vitesseMarche = 2.6f; vitesseCourse = 8.8f; porteeAttaque = 2.2f; degats = 9f; cadence = 1.1f;
                    ouie = 1.3f; vue = 15f; VieMax = 34f; couleur = new Color(0.35f, 1.9f, 1.8f);
                    break;
            }
            Vie = VieMax;
            hauteurCorps = 0.75f * taille;
            longueurPatte = 1.35f * taille;
            haut = Planete.Haut(transform.position);
            avant = Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, haut).normalized;

            materiauCarapace = Formes.Opaque(new Color(0.09f, 0.085f, 0.1f), 0.35f, 0.75f);
            var patteMat = Formes.Opaque(new Color(0.14f, 0.13f, 0.15f), 0.4f, 0.6f);
            materiauNoyau = Formes.Lueur(couleur, 0.8f, 0.6f);
            var cristal = Formes.Emissif(new Color(0.05f, 0.1f, 0.1f), couleur * 0.9f, 0.9f);

            corps = new GameObject("Corps").transform;
            corps.SetParent(transform, false);
            var carapace = Formes.Primitive(PrimitiveType.Sphere, corps, materiauCarapace, "Carapace").transform;
            carapace.localScale = new Vector3(1f, 0.55f, 1.45f) * taille;
            var abdomen = Formes.Primitive(PrimitiveType.Sphere, corps, materiauCarapace, "Abdomen").transform;
            abdomen.localScale = new Vector3(0.85f, 0.6f, 1.1f) * taille;
            abdomen.localPosition = new Vector3(0f, 0.08f, -0.85f) * taille;
            var noyau = Formes.Primitive(PrimitiveType.Sphere, corps, materiauNoyau, "Noyau").transform;
            noyau.localScale = new Vector3(0.6f, 0.45f, 0.8f) * taille;
            noyau.localPosition = new Vector3(0f, 0.12f, -0.8f) * taille;
            tete = Formes.Primitive(PrimitiveType.Sphere, corps, materiauCarapace, "Tete").transform;
            tete.localScale = new Vector3(0.55f, 0.4f, 0.55f) * taille;
            tete.localPosition = new Vector3(0f, 0.05f, 0.82f) * taille;
            var yeux = Formes.Emissif(Color.black, couleur * 1.4f);
            foreach (float x in new[] { -0.13f, 0.13f })
            {
                var oeil = Formes.Primitive(PrimitiveType.Sphere, tete, yeux, "Oeil").transform;
                oeil.localScale = Vector3.one * 0.22f;
                oeil.localPosition = new Vector3(x / 0.55f, 0.25f, 0.38f);
            }
            mandibuleG = Mandibule(-1f, cristal);
            mandibuleD = Mandibule(1f, cristal);

            // éclats de cristal sur le dos
            int n = type == TypeEcharde.Colosse ? 9 : 5;
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject("Eclat");
                go.transform.SetParent(corps, false);
                go.AddComponent<MeshFilter>().sharedMesh = Formes.Eclat;
                go.AddComponent<MeshRenderer>().sharedMaterial = cristal;
                float z = Mathf.Lerp(0.5f, -1.2f, i / (float)Mathf.Max(1, n - 1));
                go.transform.localPosition = new Vector3(UnityEngine.Random.Range(-0.18f, 0.18f), 0.32f, z) * taille;
                go.transform.localRotation = Quaternion.Euler(UnityEngine.Random.Range(-35f, -10f), UnityEngine.Random.Range(-25f, 25f), UnityEngine.Random.Range(-20f, 20f));
                go.transform.localScale = new Vector3(0.12f, UnityEngine.Random.Range(0.35f, 0.7f), 0.12f) * taille;
                eclatsDos.Add(go.transform);
            }

            // six pattes : deux groupes en trépied (avant-gauche, milieu-droit, arrière-gauche) et l'inverse
            for (int i = 0; i < 6; i++)
            {
                float cote = i % 2 == 0 ? -1f : 1f;
                int rang = i / 2;   // 0 avant, 1 milieu, 2 arrière
                var p = new Patte
                {
                    hanche = new Vector3(cote * 0.42f, -0.05f, 0.45f - rang * 0.45f) * taille,
                    groupe = (rang + (cote < 0f ? 0 : 1)) % 2,
                };
                p.cuisse = Formes.Primitive(PrimitiveType.Cube, transform, patteMat, "Cuisse").transform;
                p.tibia = Formes.Primitive(PrimitiveType.Cube, transform, patteMat, "Tibia").transform;
                p.griffe = new GameObject("Griffe").transform;
                p.griffe.SetParent(transform, false);
                var mf = p.griffe.gameObject.AddComponent<MeshFilter>();
                mf.sharedMesh = Formes.Eclat;
                p.griffe.gameObject.AddComponent<MeshRenderer>().sharedMaterial = cristal;
                pattes.Add(p);
            }

            // collision pour les tirs (sphère sur le corps), corps cinématique
            var col = corps.gameObject.AddComponent<SphereCollider>();
            col.radius = 0.75f * taille;
            col.center = new Vector3(0f, 0.05f, -0.15f) * taille;
            var rb = corps.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // émergence : sous le sable
            Vector3 sol = transform.position;
            corps.position = sol - haut * hauteurCorps;
            corps.rotation = Quaternion.LookRotation(avant, haut);
            foreach (var p in pattes) p.pied = PiedIdeal(p, Vector3.zero);
            EffetsCombat.Impact(sol, haut, new Color(0.55f, 0.45f, 0.32f), 14, 1.2f);
            EffetsCombat.Anneau(sol, haut, 2.5f * taille, new Color(0.5f, 0.42f, 0.3f), 0.6f);
            JaugeBruit.Signaler(sol, 5f);
        }

        Transform Mandibule(float cote, Material m)
        {
            var go = new GameObject("Mandibule");
            go.transform.SetParent(tete, false);
            go.AddComponent<MeshFilter>().sharedMesh = Formes.Eclat;
            go.AddComponent<MeshRenderer>().sharedMaterial = m;
            go.transform.localPosition = new Vector3(cote * 0.3f, -0.15f, 0.42f);
            go.transform.localScale = new Vector3(0.18f, 0.7f, 0.18f);
            go.transform.localRotation = Quaternion.Euler(80f, cote * -20f, 0f);
            return go.transform;
        }

        void OnEnable()
        {
            Toutes.Add(this);
            Combat.Hostiles.Add(this);
            JaugeBruit.BruitEmis += Entendre;
        }

        void OnDisable()
        {
            Toutes.Remove(this);
            Combat.Hostiles.Remove(this);
            JaugeBruit.BruitEmis -= Entendre;
        }

        // ------------------------------------------------------------------ perception

        void Entendre(Vector3 position, float intensite)
        {
            if (!Vivante || etatIA == Etat.Emergence || EnChasse) return;
            float d = Vector3.Distance(position, transform.position);
            if (d > intensite * 2.4f * ouie + 4f) return;
            if (d < 10f && JoueurPres(position)) Chasser();
            else { etatIA = Etat.Enquete; pointCible = position; tEtat = 0f; }
        }

        static bool JoueurPres(Vector3 p)
        {
            var j = GestionnaireJeu.Instance != null ? GestionnaireJeu.Instance.joueur : null;
            return j != null && Vector3.Distance(j.transform.position, p) < 3f;
        }

        Vector3? Proie(out bool hologramme)
        {
            hologramme = false;
            var h = Hologramme.Actuel;
            if (h != null && Vector3.Distance(h.transform.position, transform.position) < 40f) { hologramme = true; return h.transform.position; }
            var jeu = GestionnaireJeu.Instance;
            if (jeu == null || jeu.Etat != EtatJeu.Jeu || jeu.joueur == null || !jeu.joueur.gameObject.activeInHierarchy) return null;
            var vie = jeu.joueur.GetComponent<VieArpenteur>();
            if (vie != null && (!vie.Vivante || vie.EnRapatriement)) return null;
            return jeu.joueur.transform.position;
        }

        void Chasser()
        {
            if (etatIA == Etat.Chasse || etatIA == Etat.Attaque || etatIA == Etat.Charge || !Vivante) return;
            etatIA = Etat.Chasse;
            tEtat = 0f;
            Alertee?.Invoke(this);
            BanqueSons.JouerA(BanqueSons.Instance?.cri, Centre, 0.6f);
            flash = 1f;
        }

        // ------------------------------------------------------------------ dégâts

        public void Subir(Degats d)
        {
            if (!Vivante) return;
            Vie -= d.quantite;
            flash = 1f;
            float masse = type == TypeEcharde.Colosse ? 0.25f : 1f;
            recul += Vector3.ProjectOnPlane(d.direction, haut).normalized * d.recul * masse;
            if (d.ralenti > 0f) ralentiFin = Mathf.Max(ralentiFin, Time.time + d.ralenti);
            BanqueSons.JouerA(BanqueSons.Instance?.toucheCreature, d.point, 0.5f);
            if (Vie <= 0f) { Mourir(d.parJoueur, d.direction); return; }
            if (!EnChasse && etatIA != Etat.Charge) Chasser();
        }

        void Mourir(bool parJoueur, Vector3 direction)
        {
            Vie = 0f;
            etatIA = Etat.Mort;
            tMort = 0f;
            EffetsCombat.Eclats(Centre, couleur, type == TypeEcharde.Colosse ? 26 : 14, 6f * Mathf.Sqrt(taille), 0.22f * taille);
            EffetsCombat.Flash(Centre, couleur, 25000f * taille, 0.3f, 10f * taille);
            BanqueSons.JouerA(BanqueSons.Instance?.mortCreature, Centre, 0.8f);
            int cellules = type == TypeEcharde.Colosse ? 3 : UnityEngine.Random.value < 0.6f ? 1 : 0;
            if (ProfilArpenteur.Actuel.A(Talent.Pillard)) cellules *= 2;
            if (cellules == 0 && UnityEngine.Random.value < 0.3f && ProfilArpenteur.Actuel.A(Talent.Pillard)) cellules = 1;
            for (int i = 0; i < cellules; i++) CelluleEnergie.Deposer(Centre + Vector3.ProjectOnPlane(UnityEngine.Random.insideUnitSphere, haut) * taille);
            var col = corps.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            Combat.Hostiles.Remove(this);
            Morte?.Invoke(this, parJoueur);
        }

        // ------------------------------------------------------------------ boucle

        void Update()
        {
            float dtReel = Time.deltaTime;
            float dt = dtReel * BulleTemporelle.Ecoulement(Centre) * (Time.time < ralentiFin ? 0.3f : 1f);
            Revele = Mathf.Max(0f, Revele - dtReel);
            flash = Mathf.Max(0f, flash - dtReel * 3f);
            respiration += dt * (EnChasse ? 7f : 2.5f);

            if (etatIA == Etat.Mort)
            {
                tMort += dtReel;
                corps.localScale = Vector3.one * Mathf.Max(0.01f, 1f - tMort * 2.5f);
                foreach (var p in pattes) { p.cuisse.localScale *= 0.9f; p.tibia.localScale *= 0.9f; }
                if (tMort > 0.45f) Destroy(gameObject);
                return;
            }

            haut = Planete.Haut(transform.position);
            Vector3 voulu = Penser(dt);
            Deplacer(voulu, dt);
            AnimerPattes(dt);
            AnimerCorps(dt);
        }

        /// <summary>Intelligence : renvoie la vitesse voulue (monde, tangente au sol).</summary>
        Vector3 Penser(float dt)
        {
            tEtat += dt;
            Vector3 ici = transform.position;
            var proie = Proie(out bool leurre);

            switch (etatIA)
            {
                case Etat.Emergence:
                    if (tEtat > 1.2f) { etatIA = Etat.Errance; tEtat = 0f; NouveauPoint(); }
                    return Vector3.zero;

                case Etat.Errance:
                case Etat.Enquete:
                {
                    if (proie.HasValue)
                    {
                        float d = Vector3.Distance(proie.Value, ici);
                        var jauge = GestionnaireJeu.Instance != null ? GestionnaireJeu.Instance.jauge : null;
                        float bruit = jauge != null ? jauge.Bruit : 0f;
                        bool voile = CompetencesArpenteur.Instance != null && CompetencesArpenteur.Instance.VoileActif;
                        float vueEff = vue * (voile ? 0.5f : 1f) * (GestionnaireJeu.Instance != null && GestionnaireJeu.Instance.etatJoueur != null && GestionnaireJeu.Instance.etatJoueur.Silence ? 0.65f : 1f);
                        if (leurre || d < vueEff && Visible(proie.Value) || d < (6f + bruit * 0.45f) * ouie * (voile ? 0.2f : 1f)) { Chasser(); return Vector3.zero; }
                    }
                    if (tPause > 0f) { tPause -= dt; return Vector3.zero; }
                    Vector3 vers = Vector3.ProjectOnPlane(pointCible - ici, haut);
                    if (vers.magnitude < 1.5f)
                    {
                        if (etatIA == Etat.Enquete) { etatIA = Etat.Errance; tPause = 3f; }
                        else tPause = UnityEngine.Random.Range(1f, 3.5f);
                        NouveauPoint();
                        return Vector3.zero;
                    }
                    return vers.normalized * (etatIA == Etat.Enquete ? (vitesseMarche + vitesseCourse) * 0.5f : vitesseMarche);
                }

                case Etat.Chasse:
                {
                    if (!proie.HasValue) { etatIA = Etat.Errance; NouveauPoint(); return Vector3.zero; }
                    Vector3 p = proie.Value;
                    dernierePositionVue = p;
                    Vector3 vers = Vector3.ProjectOnPlane(p - ici, haut);
                    float d = vers.magnitude;
                    if (d > vue * 3f && !leurre) { etatIA = Etat.Enquete; pointCible = dernierePositionVue; tEtat = 0f; return Vector3.zero; }
                    Vector3 dir = d > 0.01f ? vers / d : avant;
                    switch (type)
                    {
                        case TypeEcharde.Cracheuse:
                        {
                            if (Time.time >= prochainCoup && d < porteeAttaque * 1.4f && Visible(p)) { Cracher(p); }
                            Vector3 cote = Vector3.Cross(haut, dir) * Mathf.Sin(Time.time * 0.7f + GetInstanceID());
                            if (d < 9f) return (-dir + cote).normalized * vitesseCourse * 0.7f;
                            if (d > porteeAttaque) return dir * vitesseCourse;
                            return cote * vitesseMarche;
                        }
                        case TypeEcharde.Colosse:
                            if (Time.time >= prochainCoup && d > 6f && d < 18f && !leurre) { etatIA = Etat.Charge; tEtat = 0f; coupPorte = false; avant = dir; BanqueSons.JouerA(BanqueSons.Instance?.cri, Centre, 1f); return Vector3.zero; }
                            if (d < porteeAttaque && Time.time >= prochainCoup) { etatIA = Etat.Attaque; tEtat = 0f; coupPorte = false; return Vector3.zero; }
                            return dir * vitesseCourse;
                        default:
                            if (d < porteeAttaque + 1.2f && Time.time >= prochainCoup) { etatIA = Etat.Attaque; tEtat = 0f; coupPorte = false; avant = dir; return Vector3.zero; }
                            // approche en zigzag, plus dure à viser
                            Vector3 zig = Vector3.Cross(haut, dir) * Mathf.Sin(Time.time * 3f + GetInstanceID()) * 0.45f;
                            return (dir + zig).normalized * vitesseCourse;
                    }
                }

                case Etat.Attaque:
                {
                    // armer (cabrée), bondir, mordre, se remettre
                    float armer = type == TypeEcharde.Colosse ? 0.6f : 0.35f;
                    if (tEtat < armer) return Vector3.zero;
                    if (tEtat < armer + 0.18f)
                    {
                        if (!coupPorte && tEtat > armer + 0.08f) { coupPorte = true; Mordre(proie); }
                        return avant * (type == TypeEcharde.Colosse ? 4f : 14f);
                    }
                    if (tEtat > armer + 0.7f) { etatIA = Etat.Chasse; prochainCoup = Time.time + cadence; }
                    return Vector3.zero;
                }

                case Etat.Charge:
                {
                    if (tEtat < 0.7f) return Vector3.zero;   // gratte le sol
                    if (tEtat < 2f)
                    {
                        var j = GestionnaireJeu.Instance != null ? GestionnaireJeu.Instance.joueur : null;
                        if (!coupPorte && j != null && Vector3.Distance(j.transform.position, Centre) < 2.4f * taille * 0.8f) { coupPorte = true; Mordre(proie, 1.4f); }
                        return avant * 15f;
                    }
                    EffetsCombat.Anneau(transform.position, haut, 5f, new Color(0.55f, 0.45f, 0.32f), 0.5f);
                    CameraSecousse.Choc(0.25f);
                    etatIA = Etat.Chasse;
                    prochainCoup = Time.time + cadence;
                    return Vector3.zero;
                }
            }
            return Vector3.zero;
        }

        void NouveauPoint()
        {
            Vector3 h = Planete.Haut(nid);
            Vector3 dir = Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, h).normalized;
            pointCible = nid + dir * UnityEngine.Random.Range(3f, 14f);
        }

        bool Visible(Vector3 cible)
        {
            Vector3 depuis = Centre + haut * 0.4f * taille;
            Vector3 vers = cible - depuis;
            float d = vers.magnitude;
            if (d < 0.1f) return true;
            int n = Physics.RaycastNonAlloc(depuis, vers / d, vision, d - 0.6f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var t = vision[i].transform;
                if (t.IsChildOf(transform)) continue;
                if (Combat.De(vision[i].collider) != null) continue;
                return false;
            }
            return true;
        }

        void Mordre(Vector3? proie, float bonus = 1f)
        {
            prochainCoup = Time.time + cadence;
            if (!proie.HasValue) return;
            var jeu = GestionnaireJeu.Instance;
            if (Hologramme.Actuel != null && Vector3.Distance(Hologramme.Actuel.transform.position, Centre) < porteeAttaque + 2f)
            {
                EffetsCombat.Impact(Hologramme.Actuel.transform.position, haut, couleur, 8);
                return;
            }
            if (jeu == null || jeu.joueur == null) return;
            Vector3 j = jeu.joueur.transform.position;
            if (Vector3.Distance(j, Centre + avant * 0.6f * taille) > porteeAttaque + 0.9f * taille) return;
            var vie = jeu.joueur.GetComponent<VieArpenteur>();
            Vector3 dir = Vector3.ProjectOnPlane(j - Centre, haut).normalized;
            vie?.Subir(new Degats(degats * bonus, j, dir, type == TypeEcharde.Colosse ? 13f : 5f, false));
            EffetsCombat.Impact(j, -dir, couleur, 8, 0.8f);
            BanqueSons.JouerA(BanqueSons.Instance?.morsure, j, 0.8f);
        }

        void Cracher(Vector3 cible)
        {
            prochainCoup = Time.time + cadence * UnityEngine.Random.Range(0.85f, 1.25f);
            Vector3 bouche = tete.position + avant * 0.3f * taille;
            Vector3 vise = cible + Planete.Haut(cible) * 0.3f;
            float d = Vector3.Distance(vise, bouche);
            Vector3 dir = ((vise - bouche) / d + haut * (d * 0.006f)).normalized;
            Projectile.Tirer(bouche, dir, 28f, degats, 60f, couleur, false, transform, 3f, 0f, 1.3f, 0.35f);
            EffetsCombat.Eclair(bouche, dir, couleur, 1.2f);
            BanqueSons.JouerA(BanqueSons.Instance?.crachat, bouche, 0.6f);
            flash = 0.6f;
        }

        // ------------------------------------------------------------------ mouvement et animation

        void Deplacer(Vector3 voulu, float dt)
        {
            float acceleration = etatIA == Etat.Attaque || etatIA == Etat.Charge ? 30f : 8f;
            vitesse = Vector3.MoveTowards(vitesse, voulu, acceleration * dt);
            recul = Vector3.MoveTowards(recul, Vector3.zero, 18f * dt);
            Vector3 deplacement = (vitesse + recul) * dt;
            Vector3 p = transform.position + deplacement;
            Vector3 h = Planete.Haut(p);
            if (Planete.Sol(p + h * 2f, out RaycastHit sol, transform, 6f, 30f)) p = sol.point;
            transform.position = p;
            haut = h;
            Vector3 plat = Vector3.ProjectOnPlane(vitesse, haut);
            if (plat.sqrMagnitude > 0.05f && etatIA != Etat.Charge && etatIA != Etat.Attaque) avant = Vector3.Slerp(avant, plat.normalized, 1f - Mathf.Exp(-dt * 6f));
            else if (EnChasse && etatIA == Etat.Chasse)
            {
                var proie = Proie(out _);
                if (proie.HasValue)
                {
                    Vector3 vers = Vector3.ProjectOnPlane(proie.Value - transform.position, haut);
                    if (vers.sqrMagnitude > 0.01f) avant = Vector3.Slerp(avant, vers.normalized, 1f - Mathf.Exp(-dt * 5f));
                }
            }
            avant = Vector3.ProjectOnPlane(avant, haut).normalized;
            if (avant.sqrMagnitude < 0.01f) avant = Vector3.ProjectOnPlane(Vector3.forward, haut).normalized;
        }

        Vector3 PiedIdeal(Patte p, Vector3 anticipation)
        {
            Quaternion repere = Quaternion.LookRotation(avant, haut);
            Vector3 dehors = new Vector3(Mathf.Sign(p.hanche.x) * 1.05f * taille, 0f, p.hanche.z * 1.3f);
            Vector3 approx = transform.position + repere * dehors + anticipation;
            return Planete.Sol(approx + haut * 2f * taille, out RaycastHit sol, transform, 4f * taille, 10f) ? sol.point : approx;
        }

        void AnimerPattes(float dt)
        {
            float v = vitesse.magnitude;
            float seuil = 0.45f * taille * (v > vitesseMarche * 1.5f ? 1.6f : 1f);
            float vitessePas = Mathf.Lerp(4f, 11f, Mathf.InverseLerp(0f, vitesseCourse, v));
            bool groupeEnPas0 = false, groupeEnPas1 = false;
            foreach (var p in pattes) if (p.t >= 0f) { if (p.groupe == 0) groupeEnPas0 = true; else groupeEnPas1 = true; }

            Vector3 anticipation = vitesse * 0.18f;
            foreach (var p in pattes)
            {
                if (p.t >= 0f)
                {
                    p.t += dt * vitessePas;
                    float k = Mathf.Clamp01(p.t);
                    p.pied = Vector3.Lerp(p.depart, p.arrivee, k) + haut * Mathf.Sin(k * Mathf.PI) * 0.35f * taille;
                    if (p.t >= 1f) { p.t = -1f; p.pied = p.arrivee; }
                }
                else
                {
                    Vector3 ideal = PiedIdeal(p, anticipation);
                    bool autreGroupe = p.groupe == 0 ? groupeEnPas1 : groupeEnPas0;
                    float ecart = Vector3.Distance(p.pied, ideal);
                    if ((ecart > seuil && !autreGroupe) || ecart > seuil * 3f || etatIA == Etat.Emergence && ecart > 0.2f)
                    {
                        p.depart = p.pied;
                        p.arrivee = ideal;
                        p.t = 0f;
                        if (p.groupe == 0) groupeEnPas0 = true; else groupeEnPas1 = true;
                    }
                }
            }
        }

        void AnimerCorps(float dt)
        {
            Vector3 solMoyen = Vector3.zero;
            foreach (var p in pattes) solMoyen += p.pied;
            solMoyen /= pattes.Count;
            // le corps se penche selon le relief (pieds avant / arrière)
            Vector3 avantPieds = (pattes[0].pied + pattes[1].pied) * 0.5f - (pattes[4].pied + pattes[5].pied) * 0.5f;
            Vector3 cotePieds = (pattes[1].pied + pattes[3].pied + pattes[5].pied) / 3f - (pattes[0].pied + pattes[2].pied + pattes[4].pied) / 3f;
            Vector3 normale = Vector3.Cross(avantPieds, cotePieds).normalized;
            if (Vector3.Dot(normale, haut) < 0f) normale = -normale;
            if (normale.sqrMagnitude < 0.5f || Vector3.Dot(normale, haut) < 0.5f) normale = haut;
            normale = Vector3.Slerp(haut, normale, 0.6f);

            float hauteur = hauteurCorps + Mathf.Sin(respiration) * 0.03f * taille;
            float cabre = 0f;
            if (etatIA == Etat.Emergence) hauteur *= Mathf.SmoothStep(-0.6f, 1f, tEtat / 1.2f);
            if (etatIA == Etat.Attaque)
            {
                float armer = type == TypeEcharde.Colosse ? 0.6f : 0.35f;
                cabre = tEtat < armer ? Mathf.SmoothStep(0f, 1f, tEtat / armer) : Mathf.Max(0f, 1f - (tEtat - armer) * 4f);
            }
            if (etatIA == Etat.Charge && tEtat < 0.7f) cabre = -0.4f + Mathf.Sin(tEtat * 30f) * 0.1f;
            Vector3 avantCorps = Vector3.ProjectOnPlane(avant, normale).normalized;
            Quaternion rot = Quaternion.LookRotation(avantCorps, normale) * Quaternion.Euler(-cabre * 35f, 0f, Mathf.Sin(respiration * 0.5f) * 2f);
            Vector3 cible = Vector3.Lerp(transform.position, solMoyen, 0.5f) + normale * (hauteur + cabre * 0.3f * taille);
            corps.SetPositionAndRotation(Vector3.Lerp(corps.position, cible, 1f - Mathf.Exp(-dt * 14f)), Quaternion.Slerp(corps.rotation, rot, 1f - Mathf.Exp(-dt * 10f)));

            // mandibules qui claquent en chasse, éclats qui frémissent, noyau qui bat
            float claque = EnChasse ? Mathf.Abs(Mathf.Sin(respiration * 2.2f)) * 25f + cabre * 30f : 5f;
            mandibuleG.localRotation = Quaternion.Euler(80f, 20f + claque, 0f);
            mandibuleD.localRotation = Quaternion.Euler(80f, -20f - claque, 0f);
            float pouls = 0.8f + Mathf.Sin(respiration * 2f) * 0.25f + flash * 2.5f + (EnChasse ? 0.5f : 0f);
            materiauNoyau.SetFloat("_Intensite", pouls);
            materiauNoyau.SetColor("_Couleur", Time.time < ralentiFin ? new Color(1.5f, 0.7f, 2.8f) : couleur);
            if (flash > 0f) Formes.DefinirEmission(materiauCarapace, couleur * flash * 0.6f);

            // pattes : hanche sur le corps, genou relevé vers l'extérieur, pied au sol
            foreach (var p in pattes)
            {
                Vector3 hanche = corps.TransformPoint(p.hanche);
                Vector3 pied = p.pied;
                Vector3 milieu = (hanche + pied) * 0.5f;
                Vector3 dehors = Vector3.ProjectOnPlane(hanche - corps.position, normale).normalized;
                float l = Vector3.Distance(hanche, pied);
                float plie = Mathf.Sqrt(Mathf.Max(0f, longueurPatte * longueurPatte - l * l)) * 0.5f;
                Vector3 genou = milieu + normale * (plie + 0.25f * taille) + dehors * 0.35f * taille;
                Segment(p.cuisse, hanche, genou, 0.13f * taille);
                Segment(p.tibia, genou, pied, 0.09f * taille);
                p.griffe.position = pied + normale * 0.25f * taille;
                p.griffe.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(pied - genou, normale) + normale * 0.001f, normale) * Quaternion.Euler(180f, 0f, 0f);
                p.griffe.localScale = new Vector3(0.08f, 0.25f, 0.08f) * taille;
            }
        }

        static void Segment(Transform t, Vector3 a, Vector3 b, float epaisseur)
        {
            Vector3 d = b - a;
            float l = d.magnitude;
            if (l < 1e-4f) return;
            t.SetPositionAndRotation((a + b) * 0.5f, Quaternion.LookRotation(d / l, Vector3.Cross(d, Vector3.right).sqrMagnitude > 1e-4f ? Vector3.Cross(d, Vector3.right) : Vector3.up));
            t.localScale = new Vector3(epaisseur, epaisseur, l);
        }

        void OnDestroy()
        {
            if (materiauNoyau != null) Destroy(materiauNoyau);
            if (materiauCarapace != null) Destroy(materiauCarapace);
        }
    }
}
