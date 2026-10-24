using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Combat
{
    /// <summary>
    /// L'arme tenue par l'Arpenteur (vue à la 3e personne) et toutes ses animations, calculées par le code :
    /// position prête / visée / course, recul à ressort, balancement du regard, roulis de la marche,
    /// rechargement (le chargeur sort et revient), changement d'arme, coup de pic, coup de crosse, lancer
    /// de grenade, vibration du fusil Sablier en charge. Donne les points de prise aux mains (<see cref="AnimateurArpenteur"/>).
    /// Modèles Kenney Blaster Kit si l'installateur les a branchés, sinon formes générées.
    /// </summary>
    [DefaultExecutionOrder(40)]
    public class VueArme : MonoBehaviour
    {
        struct Ressort
        {
            public Vector3 x, v;
            public void Pas(Vector3 cible, float raideur, float amorti, float dt)
            {
                Vector3 a = -raideur * (x - cible) - amorti * v;
                v += a * dt;
                x += v * dt;
            }
        }

        sealed class Modele
        {
            public Transform racine, chargeur, lueur;
            public Vector3 bouche, poigneeGauche, chargeurBase;
            public Material materiauLueur;
            public float longueur;
        }

        Arsenal arsenal;
        EtatArpenteur etat;
        Modele[] modeles;
        int actuel = -1;
        Ressort position, rotation;
        Vector3 posePosition;
        Quaternion poseRotation = Quaternion.identity;
        bool poseInitialisee;
        float phase, tRecharge = -1f, dureeRecharge = 1f, tChangement = -1f, tMelee = -1f, tLancer = -1f;
        bool meleePic;
        Transform grenadeEnMain;
        Vector3 mainLancer;

        public Vector3 Bouche => actuel >= 0 && modeles[actuel] != null ? modeles[actuel].racine.TransformPoint(modeles[actuel].bouche) : transform.position;
        public Vector3 PoigneeDroite => actuel >= 0 && modeles[actuel] != null ? modeles[actuel].racine.position : transform.position;
        public Vector3 PoigneeGauche => actuel >= 0 && modeles[actuel] != null ? modeles[actuel].racine.TransformPoint(modeles[actuel].poigneeGauche) : transform.position;
        public Quaternion RotationArme => actuel >= 0 && modeles[actuel] != null ? modeles[actuel].racine.rotation : transform.rotation;
        /// <summary>La main gauche tient l'arme (pas pendant un lancer, pas avec une grenade en main).</summary>
        public bool DeuxMains => Visible && tLancer < 0f && actuel >= 0 && Catalogue.Armes[actuel].mode != ModeTir.Lancer;
        public bool Visible { get; private set; }
        /// <summary>Position de la main droite pendant un lancer (départ de la grenade).</summary>
        public Vector3 MainLancer => tLancer >= 0f ? mainLancer : Bouche;
        public bool EnLancer => tLancer >= 0f;

        public void Preparer(Arsenal a)
        {
            arsenal = a;
            etat = GetComponent<EtatArpenteur>();
            modeles = new Modele[Catalogue.Armes.Length];
            for (int i = 0; i < modeles.Length; i++)
            {
                var prefab = a.modeles != null && i < a.modeles.Length ? a.modeles[i] : null;
                modeles[i] = Construire(Catalogue.Armes[i], prefab, a.canonVersMoinsZ);
                modeles[i].racine.gameObject.SetActive(false);
            }
            var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(g.GetComponent<Collider>());
            g.name = "Grenade en main";
            g.GetComponent<MeshRenderer>().sharedMaterial = Formes.Lueur(Catalogue.Armes[Catalogue.Index("echo")].couleur, 1f, 0.4f);
            g.transform.SetParent(transform, false);
            g.transform.localScale = Vector3.one * 0.12f;
            g.SetActive(false);
            grenadeEnMain = g.transform;
        }

        public void Montrer(int index)
        {
            if (modeles == null) return;
            for (int i = 0; i < modeles.Length; i++) modeles[i].racine.gameObject.SetActive(i == index);
            actuel = index;
        }

        // ------------------------------------------------------------------ déclencheurs d'animation

        public void Recul(float force)
        {
            rotation.v += new Vector3(-force * 160f, Random.Range(-1f, 1f) * force * 40f, Random.Range(-1f, 1f) * force * 60f);
            position.v += new Vector3(0f, force * 0.25f, -force * 1.2f);
            var m = actuel >= 0 ? modeles[actuel] : null;
            if (m != null && m.materiauLueur != null) m.materiauLueur.SetFloat("_Intensite", 3f);
        }

        public void Changer() { tChangement = 0f; tRecharge = -1f; }
        public void Recharger(float duree) { tRecharge = duree > 0f ? 0f : -1f; dureeRecharge = Mathf.Max(0.1f, duree); }
        public void Melee(bool pic) { tMelee = 0f; meleePic = pic; }
        public void Lancer() { tLancer = 0f; }

        // ------------------------------------------------------------------ boucle

        void LateUpdate()
        {
            var jeu = GestionnaireJeu.Instance;
            bool visible = jeu != null && (jeu.Etat == EtatJeu.Jeu || jeu.Etat == EtatJeu.Pause || jeu.Etat == EtatJeu.Extraction) && actuel >= 0;
            if (visible != Visible)
            {
                Visible = visible;
                if (actuel >= 0) modeles[actuel].racine.gameObject.SetActive(visible);
                poseInitialisee = false;
            }
            if (!Visible) { grenadeEnMain.gameObject.SetActive(false); return; }

            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            var m = modeles[actuel];
            var def = Catalogue.Armes[actuel];
            Vector3 haut = etat.Haut;
            var anim = GetComponent<AnimateurArpenteur>();
            Vector3 avant = anim != null ? anim.Avant : Vector3.ProjectOnPlane(transform.forward, haut).normalized;
            Vector3 droite = Vector3.Cross(haut, avant);
            var cam = Camera.main;
            Vector3 epaule = anim != null && anim.EpauleDroite != null ? anim.EpauleDroite.position - haut * 0.05f : transform.position + haut * 0.45f + droite * 0.2f;
            Vector3 viseeDir = avant;
            if (cam != null)
            {
                var r = new Ray(cam.transform.position, cam.transform.forward);
                Vector3 point = Visee.Rayon(r.origin, r.direction, 200f, out RaycastHit h) ? h.point : r.origin + r.direction * 200f;
                viseeDir = (point - epaule).normalized;
                if (Vector3.Dot(viseeDir, cam.transform.forward) < 0.2f) viseeDir = cam.transform.forward;
            }

            // pose de base
            bool tirRecent = Time.time - arsenal.DernierTir < 1.1f || arsenal.Charge > 0f;
            bool enJoue = (arsenal.Vise || tirRecent) && def.mode != ModeTir.Melee && def.mode != ModeTir.Lancer;
            bool court = def.longueur < 0.45f;
            Vector3 cibleP;
            Quaternion cibleR;
            if (def.mode == ModeTir.Melee)
            {
                cibleR = Quaternion.LookRotation(avant, haut) * Quaternion.Euler(-55f, -10f, 0f);
                cibleP = epaule + avant * 0.32f - haut * 0.3f - droite * 0.04f;
            }
            else if (enJoue)
            {
                cibleR = Quaternion.LookRotation(viseeDir, haut);
                cibleP = epaule + viseeDir * (court ? 0.52f : 0.3f) - haut * (arsenal.Vise ? 0.04f : 0.1f) - droite * (arsenal.Vise ? 0.08f : 0f);
            }
            else if (etat.Course)
            {
                cibleR = Quaternion.LookRotation(avant, haut) * Quaternion.Euler(38f, -48f, -15f);
                cibleP = epaule + avant * 0.26f - haut * 0.2f - droite * 0.16f;
            }
            else
            {
                Vector3 plat = Vector3.ProjectOnPlane(viseeDir, haut).normalized;
                cibleR = Quaternion.LookRotation(Vector3.Slerp(avant, plat, 0.4f), haut) * Quaternion.Euler(court ? 30f : 22f, -14f, 0f);
                cibleP = epaule + avant * (court ? 0.3f : 0.24f) - haut * 0.2f - droite * 0.06f;
            }
            // les poses se mélangent dans le repère du corps : l'arme suit l'Arpenteur sans traîner derrière lui
            Quaternion corps = Quaternion.LookRotation(avant, haut);
            Quaternion inverse = Quaternion.Inverse(corps);
            Vector3 localP = inverse * (cibleP - epaule);
            Quaternion localR = inverse * cibleR;
            if (!poseInitialisee) { posePosition = localP; poseRotation = localR; poseInitialisee = true; }
            float vitesse = enJoue ? 22f : 10f;
            posePosition = Vector3.Lerp(posePosition, localP, 1f - Mathf.Exp(-dt * vitesse));
            poseRotation = Quaternion.Slerp(poseRotation, localR, 1f - Mathf.Exp(-dt * vitesse));
            Vector3 poseMondeP = epaule + corps * posePosition;
            Quaternion poseMondeR = corps * poseRotation;

            // décalages animés (repère de l'arme : x droite, y haut, z canon)
            Vector3 cibleOffP = Vector3.zero, cibleOffR = Vector3.zero;
            Vector2 regard = Entrees.Regard();
            cibleOffR += new Vector3(Mathf.Clamp(regard.y * 1.2f, -6f, 6f), Mathf.Clamp(-regard.x * 1.2f, -8f, 8f), Mathf.Clamp(regard.x * 1.5f, -8f, 8f));
            if (etat.Bouge && etat.AuSol)
            {
                phase += dt * (etat.Course ? 11f : 7f);
                float a = etat.Course ? 1.8f : arsenal.Vise ? 0.35f : 1f;
                cibleOffP += new Vector3(Mathf.Sin(phase) * 0.012f, Mathf.Abs(Mathf.Cos(phase)) * 0.016f, 0f) * a;
                cibleOffR += new Vector3(Mathf.Abs(Mathf.Sin(phase)) * 1.5f, 0f, Mathf.Sin(phase) * 2.5f) * a;
            }
            else
            {
                phase += dt * 1.6f;   // respiration
                cibleOffP += new Vector3(0f, Mathf.Sin(phase) * 0.004f, 0f);
                cibleOffR += new Vector3(Mathf.Sin(phase * 0.5f) * 0.6f, Mathf.Sin(phase * 0.37f) * 0.5f, 0f);
            }
            if (!etat.AuSol) { cibleOffP += new Vector3(0f, -0.05f, 0f); cibleOffR += new Vector3(8f, 0f, 4f); }

            AnimerRechargement(dt, m, ref cibleOffP, ref cibleOffR);
            AnimerChangement(dt, ref cibleOffP, ref cibleOffR);
            AnimerMelee(dt, ref cibleOffP, ref cibleOffR);
            AnimerLancer(dt, epaule, avant, haut, droite, ref cibleOffP, ref cibleOffR);
            if (arsenal.Charge > 0f)
            {
                float c = arsenal.Charge;
                cibleOffP += Random.insideUnitSphere * 0.004f * c;
                cibleOffR += new Vector3(0f, 0f, Mathf.Sin(Time.time * 60f) * 1.5f * c);
            }

            position.Pas(cibleOffP, 220f, 22f, dt);
            rotation.Pas(cibleOffR, 260f, 24f, dt);
            Quaternion rot = poseMondeR * Quaternion.Euler(rotation.x);
            Vector3 pos = poseMondeP + rot * position.x;
            m.racine.SetPositionAndRotation(pos, rot);

            if (m.materiauLueur != null)
            {
                float base_ = def.mode == ModeTir.Charge ? 0.6f + arsenal.Charge * 3f : 0.8f;
                float actuelle = m.materiauLueur.GetFloat("_Intensite");
                m.materiauLueur.SetFloat("_Intensite", Mathf.Lerp(actuelle, base_, 1f - Mathf.Exp(-dt * 10f)));
            }
        }

        static float Cloche(float k, float a, float b) => Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(a, b, k) * Mathf.PI));
        static float Doux(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }

        void AnimerRechargement(float dt, Modele m, ref Vector3 p, ref Vector3 r)
        {
            if (tRecharge < 0f)
            {
                if (m.chargeur != null) m.chargeur.localPosition = m.chargeurBase;
                return;
            }
            tRecharge += dt;
            float k = tRecharge / dureeRecharge;
            float baisse = Cloche(k, 0f, 1f);
            p += new Vector3(-0.04f, -0.1f, -0.04f) * baisse;
            r += new Vector3(24f, -18f, -38f) * baisse;
            // chargeur : sort (25-45 %), reste dehors, revient (60-75 %) avec un à-coup
            float sortie = Doux(Mathf.InverseLerp(0.25f, 0.42f, k)) * (1f - Doux(Mathf.InverseLerp(0.6f, 0.75f, k)));
            if (m.chargeur != null)
            {
                float echelle = m.chargeur.parent != null ? Mathf.Max(0.001f, m.chargeur.parent.lossyScale.y) : 1f;
                m.chargeur.localPosition = m.chargeurBase + Vector3.down * (0.22f * sortie / echelle);
            }
            if (k > 0.75f && k - dt / dureeRecharge <= 0.75f) { position.v += new Vector3(0f, 0.5f, 0f); rotation.v += new Vector3(-60f, 0f, 30f); }
            if (k >= 1f) tRecharge = -1f;
        }

        void AnimerChangement(float dt, ref Vector3 p, ref Vector3 r)
        {
            if (tChangement < 0f) return;
            tChangement += dt;
            float k = tChangement / 0.45f;
            float bas = k < 0.5f ? Doux(k * 2f) : 1f - Doux((k - 0.5f) * 2f);
            p += new Vector3(0.05f, -0.32f, -0.1f) * bas;
            r += new Vector3(65f, 20f, 10f) * bas;
            if (k >= 1f) tChangement = -1f;
        }

        void AnimerMelee(float dt, ref Vector3 p, ref Vector3 r)
        {
            if (tMelee < 0f) return;
            tMelee += dt;
            float k = tMelee / 0.42f;
            if (meleePic)
            {
                // armé (0-30 %), frappe (30-55 %), retour
                float arme = Doux(k / 0.3f) * (1f - Doux(Mathf.InverseLerp(0.3f, 0.45f, k)));
                float frappe = Doux(Mathf.InverseLerp(0.3f, 0.45f, k)) * (1f - Doux(Mathf.InverseLerp(0.6f, 1f, k)));
                r += new Vector3(-75f, 35f, 20f) * arme + new Vector3(95f, -40f, -15f) * frappe;
                p += new Vector3(0.1f, 0.25f, -0.15f) * arme + new Vector3(-0.08f, -0.1f, 0.45f) * frappe;
            }
            else
            {
                float coup = Cloche(k, 0.1f, 0.7f);
                p += new Vector3(-0.05f, 0.04f, 0.38f) * coup;
                r += new Vector3(-12f, 25f, 40f) * coup;
            }
            if (k >= 1f) tMelee = -1f;
        }

        void AnimerLancer(float dt, Vector3 epaule, Vector3 avant, Vector3 haut, Vector3 droite, ref Vector3 p, ref Vector3 r)
        {
            if (tLancer < 0f) { grenadeEnMain.gameObject.SetActive(false); return; }
            tLancer += dt;
            float k = tLancer / 0.55f;
            float bas = Cloche(k, 0f, 1f);
            p += new Vector3(-0.12f, -0.22f, -0.05f) * bas;
            r += new Vector3(35f, 25f, 15f) * bas;
            // la main droite part en arrière puis lance vers l'avant
            float armer = Doux(k / 0.3f);
            float lancer = Doux(Mathf.InverseLerp(0.3f, 0.45f, k));
            Vector3 arriere = epaule + haut * 0.2f - avant * 0.3f + droite * 0.15f;
            Vector3 devant = epaule + haut * 0.3f + avant * 0.55f;
            mainLancer = Vector3.Lerp(Vector3.Lerp(epaule, arriere, armer), devant, lancer);
            grenadeEnMain.gameObject.SetActive(k < 0.44f);
            grenadeEnMain.position = mainLancer;
            if (k >= 1f) tLancer = -1f;
        }

        // ------------------------------------------------------------------ modèles

        Modele Construire(DefinitionArme def, GameObject prefab, bool canonVersMoinsZ)
        {
            var m = new Modele { longueur = def.longueur };
            var racine = new GameObject("Arme " + def.surnom).transform;
            racine.position = Vector3.zero;
            racine.rotation = Quaternion.identity;
            m.racine = racine;
            Transform modele;
            if (prefab != null)
            {
                modele = Instantiate(prefab, racine).transform;
                foreach (var c in modele.GetComponentsInChildren<Collider>()) Destroy(c);
                modele.localRotation = canonVersMoinsZ ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
                modele.localPosition = Vector3.zero;
                modele.localScale = Vector3.one;
                if (arsenal.teinteMetal) Assombrir(modele);
                m.chargeur = TrouverEnfant(modele, "magazine");
            }
            else modele = Generer(def, racine, m);

            // mise à l'échelle : longueur voulue, poignée à l'origine, canon vers +Z
            var b = Limites(racine);
            float s = def.longueur / Mathf.Max(0.01f, b.size.z);
            modele.localScale *= s;
            b = Limites(racine);
            Vector3 poignee = new Vector3(b.center.x, b.center.y - b.size.y * 0.25f, b.center.z - b.size.z * 0.06f);
            if (def.mode == ModeTir.Melee) poignee = new Vector3(b.center.x, b.center.y, b.min.z + b.size.z * 0.25f);
            modele.localPosition -= poignee;
            b = Limites(racine);
            m.bouche = new Vector3(b.center.x, b.center.y + b.size.y * 0.1f, b.max.z);
            m.poigneeGauche = def.longueur < 0.45f ? new Vector3(-0.03f, -0.06f, 0.02f) : new Vector3(0f, b.center.y - b.size.y * 0.2f, b.max.z * 0.55f);
            if (def.mode == ModeTir.Melee) m.poigneeGauche = new Vector3(0f, 0f, -0.18f);
            if (m.chargeur != null) m.chargeurBase = m.chargeur.localPosition;

            // petite lueur à la bouche (couleur de l'arme)
            if (def.mode != ModeTir.Lancer)
            {
                m.materiauLueur = Formes.Lueur(def.couleur, 1.2f, 0.3f);
                m.lueur = Formes.Primitive(PrimitiveType.Sphere, racine, m.materiauLueur, "Lueur").transform;
                m.lueur.localPosition = m.bouche - Vector3.forward * 0.02f;
                m.lueur.localScale = Vector3.one * (def.mode == ModeTir.Melee ? 0.07f : 0.045f);
                if (def.mode == ModeTir.Melee) m.lueur.localPosition = new Vector3(0f, 0.12f, b.max.z - 0.03f);
            }
            racine.SetParent(transform, true);
            foreach (var r in racine.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return m;
        }

        static Bounds Limites(Transform racine)
        {
            var rendus = racine.GetComponentsInChildren<Renderer>();
            if (rendus.Length == 0) return new Bounds(racine.position, Vector3.one * 0.1f);
            var b = rendus[0].bounds;
            foreach (var r in rendus) b.Encapsulate(r.bounds);
            return b;
        }

        static Transform TrouverEnfant(Transform t, string nom)
        {
            foreach (var c in t.GetComponentsInChildren<Transform>(true))
                if (c.name.ToLowerInvariant().Contains(nom)) return c;
            return null;
        }

        static readonly int IdBase = Shader.PropertyToID("_BaseColor");
        static readonly int IdBaseGltf = Shader.PropertyToID("baseColorFactor");

        /// <summary>Teinte métal sombre pour fondre les blasters Kenney (jouets colorés) dans un rendu réaliste.</summary>
        static void Assombrir(Transform modele)
        {
            var metal = new Color(0.5f, 0.52f, 0.56f);
            foreach (var r in modele.GetComponentsInChildren<Renderer>())
                foreach (var mat in r.materials)
                {
                    if (mat.HasProperty(IdBaseGltf)) mat.SetColor(IdBaseGltf, mat.GetColor(IdBaseGltf) * metal);
                    else if (mat.HasProperty(IdBase)) mat.SetColor(IdBase, mat.GetColor(IdBase) * metal);
                    if (mat.HasProperty("metallicFactor")) mat.SetFloat("metallicFactor", 0.55f);
                    if (mat.HasProperty("roughnessFactor")) mat.SetFloat("roughnessFactor", 0.45f);
                    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.55f);
                    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.55f);
                }
        }

        /// <summary>Arme générée en formes simples (si le Blaster Kit n'est pas téléchargé).</summary>
        static Transform Generer(DefinitionArme def, Transform racine, Modele m)
        {
            var modele = new GameObject("Forme").transform;
            modele.SetParent(racine, false);
            var metal = Formes.Opaque(new Color(0.2f, 0.21f, 0.23f), 0.75f, 0.55f);
            var clair = Formes.Opaque(new Color(0.55f, 0.56f, 0.58f), 0.6f, 0.45f);
            var lueur = Formes.Emissif(Color.black, def.couleur * 0.8f);
            Transform Bloc(PrimitiveType t, Material mat, Vector3 pos, Vector3 echelle, Vector3 rot, string nom = "Piece")
            {
                var p = Formes.Primitive(t, modele, mat, nom).transform;
                p.localPosition = pos; p.localScale = echelle; p.localRotation = Quaternion.Euler(rot);
                return p;
            }
            switch (def.mode)
            {
                case ModeTir.Melee:
                    Bloc(PrimitiveType.Cylinder, clair, new Vector3(0f, 0f, 0f), new Vector3(0.035f, 0.35f, 0.035f), new Vector3(90f, 0f, 0f), "Manche");
                    Bloc(PrimitiveType.Cube, metal, new Vector3(0f, 0.05f, 0.34f), new Vector3(0.05f, 0.3f, 0.07f), Vector3.zero, "Tete");
                    Bloc(PrimitiveType.Cube, lueur, new Vector3(0f, 0.2f, 0.34f), new Vector3(0.03f, 0.06f, 0.05f), new Vector3(0f, 0f, 0f), "Pointe");
                    Bloc(PrimitiveType.Cube, metal, new Vector3(0f, 0f, -0.3f), new Vector3(0.045f, 0.045f, 0.08f), Vector3.zero, "Pommeau");
                    break;
                case ModeTir.Lancer:
                    Bloc(PrimitiveType.Sphere, metal, Vector3.zero, Vector3.one * 0.12f, Vector3.zero, "Coque");
                    Bloc(PrimitiveType.Cylinder, lueur, Vector3.zero, new Vector3(0.125f, 0.01f, 0.125f), Vector3.zero, "Bande");
                    break;
                default:
                {
                    float l = def.mode == ModeTir.Charge ? 1f : def.mode == ModeTir.Coup ? 0.3f : 0.7f;
                    Bloc(PrimitiveType.Cube, metal, new Vector3(0f, 0.02f, 0f), new Vector3(0.07f, 0.1f, l * 0.55f), Vector3.zero, "Corps");
                    Bloc(PrimitiveType.Cube, metal, new Vector3(0f, -0.08f, -l * 0.08f), new Vector3(0.05f, 0.14f, 0.06f), new Vector3(-15f, 0f, 0f), "Crosse");
                    Bloc(PrimitiveType.Cylinder, clair, new Vector3(0f, 0.035f, l * 0.42f), new Vector3(0.03f, l * 0.2f, 0.03f), new Vector3(90f, 0f, 0f), "Canon");
                    Bloc(PrimitiveType.Cube, lueur, new Vector3(0.037f, 0.04f, l * 0.05f), new Vector3(0.006f, 0.02f, l * 0.35f), Vector3.zero, "Bande");
                    if (def.mode != ModeTir.Coup)
                        m.chargeur = Bloc(PrimitiveType.Cube, clair, new Vector3(0f, -0.09f, l * 0.08f), new Vector3(0.04f, 0.12f, 0.06f), new Vector3(10f, 0f, 0f), "magazine");
                    if (def.mode == ModeTir.Rafale || def.mode == ModeTir.Charge)
                        Bloc(PrimitiveType.Cube, metal, new Vector3(0f, 0.02f, -l * 0.42f), new Vector3(0.05f, 0.09f, l * 0.25f), Vector3.zero, "Crosse arriere");
                    if (def.mode == ModeTir.Charge)
                        Bloc(PrimitiveType.Cylinder, metal, new Vector3(0f, 0.1f, l * 0.05f), new Vector3(0.035f, 0.12f, 0.035f), new Vector3(90f, 0f, 0f), "Lunette");
                    if (def.mode == ModeTir.Eclats)
                        Bloc(PrimitiveType.Cylinder, clair, new Vector3(0f, -0.01f, l * 0.4f), new Vector3(0.03f, l * 0.2f, 0.03f), new Vector3(90f, 0f, 0f), "Canon 2");
                    break;
                }
            }
            return modele;
        }
    }
}
