using System.Collections.Generic;
using EchosEther.Combat;
using EchosEther.Jeu;
using UnityEngine;

namespace EchosEther.Joueur
{
    /// <summary>
    /// Anime le modèle de l'Arpenteur, quel qu'il soit :
    /// 1. s'il a des clips (composant Animation importé par glTFast, ou Animator avec un contrôleur), il les joue
    ///    (repos, marche, course, saut) à la bonne vitesse ;
    /// 2. sinon, s'il a un squelette (os trouvés par nom ou par l'avatar Humanoid), il l'anime par le code :
    ///    foulées synchronisées sur la vitesse, balancement des bras, bassin, inclinaison en course, marche accroupie
    ///    (silence), jambes repliées en l'air, amorti à l'atterrissage, regard vers la visée ;
    /// 3. sinon, il fait vivre le modèle entier (rebond, inclinaison, écrasement à l'atterrissage).
    /// Dans tous les cas : le corps se tourne vers la marche, ou vers la visée quand une arme est en joue,
    /// et les bras tiennent l'arme (cinématique inverse à deux os vers les poignées de <see cref="VueArme"/>).
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class AnimateurArpenteur : MonoBehaviour
    {
        [Tooltip("Racine du modèle visible. Vide : premier enfant qui porte des rendus (hors armes et effets).")]
        public Transform modele;
        [Tooltip("Animer les os par le code quand le modèle n'a pas de clips.")]
        public bool procedural = true;
        [Tooltip("Bras qui tiennent l'arme par cinématique inverse.")]
        public bool brasSurArme = true;
        public float vitesseRotation = 600f;
        [Tooltip("Longueur d'une foulée de marche (m) : règle la cadence des pas.")]
        public float foulee = 1.5f;

        /// <summary>Direction vers laquelle le corps est tourné (monde).</summary>
        public Vector3 Avant { get; private set; } = Vector3.forward;
        /// <summary>Épaule droite du modèle (null sans squelette) : la vue de l'arme s'y accroche.</summary>
        public Transform EpauleDroite => Mode == "os" || Mode == "clips" ? brasD : null;
        /// <summary>Mode d'animation retenu : "clips", "os", "corps" ou "aucun".</summary>
        public string Mode { get; private set; } = "aucun";

        // os
        Transform hanches, colonne, poitrine, cou, tete;
        Transform brasG, avantBrasG, mainG, brasD, avantBrasD, mainD;
        Transform cuisseG, tibiaG, piedG, cuisseD, tibiaD, piedD;
        readonly List<Transform> os = new List<Transform>();
        readonly List<Quaternion> repos = new List<Quaternion>();
        Vector3 hanchesRepos;

        // clips
        Animation clips;
        Animator animateur;
        string clipRepos, clipMarche, clipCourse, clipSaut;
        readonly List<int> paramsVitesse = new List<int>();
        int paramSol = -1;

        EtatArpenteur etat;
        Rigidbody corps;
        VueArme vue;
        Quaternion decalageModele = Quaternion.identity;
        Vector3 positionModele, echelleModele = Vector3.one;
        float phase, accroupi, air, amorti, lacetModele, vitesseLisse;
        bool pret;

        void Awake()
        {
            etat = GetComponent<EtatArpenteur>();
            corps = GetComponent<Rigidbody>();
            if (etat != null) etat.Atterrissage += v => amorti = Mathf.Clamp01(v / 10f);
        }

        void Start() => Preparer();

        void Preparer()
        {
            if (modele == null) modele = ChercherModele();
            if (modele == null || modele == transform) { Mode = "aucun"; pret = true; return; }
            decalageModele = modele.localRotation;
            positionModele = modele.localPosition;
            echelleModele = modele.localScale;

            clips = modele.GetComponentInChildren<Animation>();
            animateur = modele.GetComponentInChildren<Animator>();
            if (clips != null && PreparerClips()) Mode = "clips";
            else if (animateur != null && animateur.runtimeAnimatorController != null) { PreparerAnimator(); Mode = "clips"; }

            TrouverOs();
            if (Mode != "clips") Mode = procedural && hanches != null && cuisseG != null && cuisseD != null ? "os" : "corps";
            foreach (var o in os) repos.Add(o.localRotation);
            if (hanches != null) hanchesRepos = hanches.localPosition;
            pret = true;
        }

        Transform ChercherModele()
        {
            Transform meilleur = null;
            int max = 0;
            foreach (Transform enfant in transform)
            {
                string n = enfant.name;
                if (n.StartsWith("Arme ") || n == "Coquille" || n == "Grenade en main" || n.StartsWith("Onde")) continue;
                if (enfant.GetComponentInChildren<SkinnedMeshRenderer>(true) != null) return enfant;
                int r = enfant.GetComponentsInChildren<Renderer>(true).Length;
                if (r > max) { max = r; meilleur = enfant; }
            }
            return meilleur;
        }

        // ------------------------------------------------------------------ clips

        bool PreparerClips()
        {
            var noms = new List<string>();
            foreach (AnimationState s in clips) noms.Add(s.name);
            if (noms.Count == 0) return false;
            clipRepos = Trouver(noms, "idle", "stand", "breath", "repos", "rest");
            clipMarche = Trouver(noms, "walk", "marche");
            clipCourse = Trouver(noms, "run", "sprint", "jog", "course");
            clipSaut = Trouver(noms, "jump", "fall", "air", "saut");
            if (clipMarche == null) clipMarche = clipCourse ?? noms[0];
            if (clipCourse == null) clipCourse = clipMarche;
            if (clipRepos == null) clipRepos = clipMarche;
            foreach (AnimationState s in clips) s.wrapMode = WrapMode.Loop;
            clips.Play(clipRepos);
            return true;
        }

        static string Trouver(List<string> noms, params string[] mots)
        {
            foreach (var mot in mots)
                foreach (var n in noms)
                    if (n.ToLowerInvariant().Contains(mot)) return n;
            return null;
        }

        void PreparerAnimator()
        {
            foreach (var p in animateur.parameters)
            {
                string n = p.name.ToLowerInvariant();
                if (p.type == AnimatorControllerParameterType.Float && (n == "speed" || n == "vitesse" || n == "movespeed" || n == "forward" || n == "velocity"))
                    paramsVitesse.Add(p.nameHash);
                if (p.type == AnimatorControllerParameterType.Bool && (n == "grounded" || n == "isgrounded" || n == "ausol" || n == "onground"))
                    paramSol = p.nameHash;
            }
        }

        void AnimerClips(float vitesse, bool auSol)
        {
            if (clips != null)
            {
                string voulu = !auSol && clipSaut != null ? clipSaut : vitesse < 0.4f ? clipRepos : vitesse > 7.5f ? clipCourse : clipMarche;
                if (!clips.IsPlaying(voulu)) clips.CrossFade(voulu, 0.2f);
                var s = clips[voulu];
                if (s != null) s.speed = voulu == clipMarche ? Mathf.Clamp(vitesse / 3.5f, 0.5f, 2f) : voulu == clipCourse ? Mathf.Clamp(vitesse / 9f, 0.7f, 1.6f) : 1f;
            }
            else if (animateur != null)
            {
                foreach (int h in paramsVitesse) animateur.SetFloat(h, vitesse);
                if (paramSol != -1) animateur.SetBool(paramSol, auSol);
            }
        }

        // ------------------------------------------------------------------ squelette

        void TrouverOs()
        {
            var a = animateur;
            if (a != null && a.isHuman && a.avatar != null)
            {
                hanches = a.GetBoneTransform(HumanBodyBones.Hips);
                colonne = a.GetBoneTransform(HumanBodyBones.Spine);
                poitrine = a.GetBoneTransform(HumanBodyBones.Chest);
                cou = a.GetBoneTransform(HumanBodyBones.Neck);
                tete = a.GetBoneTransform(HumanBodyBones.Head);
                brasG = a.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                avantBrasG = a.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                mainG = a.GetBoneTransform(HumanBodyBones.LeftHand);
                brasD = a.GetBoneTransform(HumanBodyBones.RightUpperArm);
                avantBrasD = a.GetBoneTransform(HumanBodyBones.RightLowerArm);
                mainD = a.GetBoneTransform(HumanBodyBones.RightHand);
                cuisseG = a.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                tibiaG = a.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                piedG = a.GetBoneTransform(HumanBodyBones.LeftFoot);
                cuisseD = a.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                tibiaD = a.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                piedD = a.GetBoneTransform(HumanBodyBones.RightFoot);
            }
            else
            {
                var tous = modele.GetComponentsInChildren<Transform>(true);
                hanches = Os(tous, 0, new[] { "hips", "pelvis", "hip", "bassin" }, new[] { "thigh", "upleg", "upperleg" });
                colonne = Os(tous, 0, new[] { "spine", "spine1", "spine01", "torso", "abdomen" }, new[] { "spine2", "spine3", "spine02", "spine03" });
                poitrine = Os(tous, 0, new[] { "chest", "upperchest", "spine2", "spine02", "spine3" }, null);
                cou = Os(tous, 0, new[] { "neck", "cou" }, null);
                tete = Os(tous, 0, new[] { "head", "tete" }, new[] { "end", "top", "nub" });
                brasG = Os(tous, -1, new[] { "upperarm", "arm" }, new[] { "forearm", "lowerarm", "hand" });
                avantBrasG = Os(tous, -1, new[] { "forearm", "lowerarm", "elbow" }, null);
                mainG = Os(tous, -1, new[] { "hand", "wrist" }, new[] { "thumb", "index", "middle", "ring", "pinky", "finger", "end" });
                brasD = Os(tous, 1, new[] { "upperarm", "arm" }, new[] { "forearm", "lowerarm", "hand" });
                avantBrasD = Os(tous, 1, new[] { "forearm", "lowerarm", "elbow" }, null);
                mainD = Os(tous, 1, new[] { "hand", "wrist" }, new[] { "thumb", "index", "middle", "ring", "pinky", "finger", "end" });
                cuisseG = Os(tous, -1, new[] { "upleg", "upperleg", "thigh" }, null);
                tibiaG = Os(tous, -1, new[] { "lowerleg", "calf", "shin", "knee", "leg" }, new[] { "upleg", "upperleg", "thigh" });
                piedG = Os(tous, -1, new[] { "foot", "ankle" }, new[] { "toe", "end" });
                cuisseD = Os(tous, 1, new[] { "upleg", "upperleg", "thigh" }, null);
                tibiaD = Os(tous, 1, new[] { "lowerleg", "calf", "shin", "knee", "leg" }, new[] { "upleg", "upperleg", "thigh" });
                piedD = Os(tous, 1, new[] { "foot", "ankle" }, new[] { "toe", "end" });
            }
            // une chaîne incohérente (os pas enfants les uns des autres) est abandonnée
            if (!Chaine(brasG, avantBrasG, mainG)) brasG = avantBrasG = mainG = null;
            if (!Chaine(brasD, avantBrasD, mainD)) brasD = avantBrasD = mainD = null;
            if (!Chaine(cuisseG, tibiaG, piedG)) cuisseG = tibiaG = piedG = null;
            if (!Chaine(cuisseD, tibiaD, piedD)) cuisseD = tibiaD = piedD = null;
            foreach (var o in new[] { hanches, colonne, poitrine, cou, tete, brasG, avantBrasG, mainG, brasD, avantBrasD, mainD, cuisseG, tibiaG, piedG, cuisseD, tibiaD, piedD })
                if (o != null && !os.Contains(o)) os.Add(o);
        }

        static bool Chaine(Transform a, Transform b, Transform c) =>
            a != null && b != null && c != null && b.IsChildOf(a) && c.IsChildOf(b) && a != b && b != c;

        /// <summary>Os dont le nom contient un des mots (côté : -1 gauche, 1 droit, 0 central).</summary>
        static Transform Os(Transform[] tous, int cote, string[] mots, string[] exclus)
        {
            foreach (var mot in mots)
                foreach (var t in tous)
                {
                    string n = Normaliser(t.name);
                    string compact = n.Replace(" ", "");
                    if (!compact.Contains(mot)) continue;
                    if (exclus != null && System.Array.Exists(exclus, e => compact.Contains(e))) continue;
                    int c = Cote(n);
                    if (c != cote) continue;
                    if (t.GetComponent<Renderer>() != null) continue;
                    return t;
                }
            return null;
        }

        static string Normaliser(string nom)
        {
            string n = nom.ToLowerInvariant();
            int deuxPoints = n.LastIndexOf(':');
            if (deuxPoints >= 0) n = n.Substring(deuxPoints + 1);
            int barre = n.LastIndexOf('|');
            if (barre >= 0) n = n.Substring(barre + 1);
            n = n.Replace("mixamorig", "").Replace("def-", "").Replace("org-", "").Replace("bip01", "").Replace("bip001", "");
            var sb = new System.Text.StringBuilder();
            foreach (char ch in n) sb.Append(char.IsLetterOrDigit(ch) ? ch : ' ');
            return " " + sb.ToString().Trim() + " ";
        }

        static int Cote(string n)
        {
            if (n.Contains("left") || n.Contains("gauche") || n.Contains(" l ") || n.EndsWith(" l ") || n.StartsWith(" l ")) return -1;
            if (n.Contains("right") || n.Contains("droit") || n.Contains(" r ") || n.EndsWith(" r ") || n.StartsWith(" r ")) return 1;
            return 0;
        }

        // ------------------------------------------------------------------ boucle

        void Update()
        {
            if (!pret) return;
            var jeu = GestionnaireJeu.Instance;
            Vector3 haut = etat != null ? etat.Haut : transform.up;
            Vector3 avantRacine = Vector3.ProjectOnPlane(transform.forward, haut).normalized;

            // orientation voulue
            Vector3 voulu = Avant;
            if (jeu != null && jeu.Etat == EtatJeu.Jeu)
            {
                var arsenal = Arsenal.Instance;
                bool enJoue = arsenal != null && (arsenal.Vise || Time.time - arsenal.DernierTir < 1.1f || arsenal.Charge > 0f || arsenal.Occupe);
                Vector3 vitesse = corps != null ? Vector3.ProjectOnPlane(corps.linearVelocity, haut) : Vector3.zero;
                if (enJoue && Camera.main != null) voulu = Vector3.ProjectOnPlane(Camera.main.transform.forward, haut).normalized;
                else if (vitesse.magnitude > 0.6f) voulu = vitesse.normalized;
            }
            else voulu = avantRacine;   // menu, aperçu, cinématiques : face à l'avant de l'Arpenteur
            voulu = Vector3.ProjectOnPlane(voulu, haut);
            if (voulu.sqrMagnitude < 1e-4f) voulu = avantRacine;
            voulu.Normalize();

            float cible = Vector3.SignedAngle(avantRacine, voulu, haut);
            lacetModele = Mathf.MoveTowardsAngle(lacetModele, cible, vitesseRotation * Time.deltaTime);
            Avant = Quaternion.AngleAxis(lacetModele, haut) * avantRacine;
            if (modele == null || modele == transform) return;   // sans modèle visible : seule la direction sert (armes)
            modele.localRotation = Quaternion.AngleAxis(lacetModele, Vector3.up) * decalageModele;

            if (Mode == "clips") AnimerClips(etat != null && etat.AuSol ? vitesseLisse : 0f, etat == null || etat.AuSol);
        }

        void LateUpdate()
        {
            if (!pret || modele == null || modele == transform) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            Vector3 haut = etat != null ? etat.Haut : transform.up;
            float vitesse = etat != null ? etat.Vitesse : 0f;
            bool auSol = etat == null || etat.AuSol;
            vitesseLisse = Mathf.Lerp(vitesseLisse, vitesse, 1f - Mathf.Exp(-dt * 8f));
            accroupi = Mathf.MoveTowards(accroupi, etat != null && etat.Silence ? 1f : 0f, dt * 4f);
            air = Mathf.MoveTowards(air, auSol ? 0f : 1f, dt * 5f);
            amorti = Mathf.MoveTowards(amorti, 0f, dt * 3f);
            float course = Mathf.InverseLerp(6.5f, 11f, vitesseLisse);
            float pasLong = Mathf.Lerp(foulee, foulee * 1.6f, course);
            if (auSol) phase += dt * vitesseLisse / pasLong * Mathf.PI;
            float k = Mathf.Clamp01(vitesseLisse / 5f);

            Vector3 F = Avant, U = haut, R = Vector3.Cross(U, F);

            if (Mode == "os") AnimerOs(dt, F, U, R, k, course);
            else if (Mode == "corps") AnimerCorps(F, U, k, course);

            if (brasSurArme && (Mode == "os" || Mode == "clips")) TenirArme(F, U, R);
        }

        void AnimerOs(float dt, Vector3 F, Vector3 U, Vector3 R, float k, float course)
        {
            for (int i = 0; i < os.Count; i++) os[i].localRotation = repos[i];
            if (hanches != null) hanches.localPosition = hanchesRepos;

            float s = Mathf.Sin(phase), c = Mathf.Cos(phase);
            // bassin : rebond à chaque pas, descente accroupie, amorti d'atterrissage
            if (hanches != null)
            {
                float bas = (Mathf.Abs(c) * 0.035f * k + accroupi * 0.18f + amorti * 0.12f + air * 0.02f);
                hanches.position -= U * bas;
                hanches.rotation = Quaternion.AngleAxis(s * 5f * k, U) * Quaternion.AngleAxis(-s * 3f * k, F) * hanches.rotation;
            }
            float penche = 3f * k + 10f * course + accroupi * 14f + amorti * 10f;
            float visee = 0f;
            if (Camera.main != null)
            {
                var arsenal = Arsenal.Instance;
                if (arsenal != null && (arsenal.Vise || Time.time - arsenal.DernierTir < 1.1f))
                    visee = Mathf.Clamp(Vector3.SignedAngle(Vector3.ProjectOnPlane(Camera.main.transform.forward, U), Camera.main.transform.forward, R), -50f, 50f);
            }
            if (colonne != null) colonne.rotation = Quaternion.AngleAxis(-s * 6f * k, U) * Quaternion.AngleAxis(penche * 0.5f + visee * 0.3f, R) * colonne.rotation;
            if (poitrine != null) poitrine.rotation = Quaternion.AngleAxis(penche * 0.3f + visee * 0.35f, R) * poitrine.rotation;
            if (tete != null && Camera.main != null)
            {
                Vector3 regard = Camera.main.transform.forward;
                float lacet = Mathf.Clamp(Vector3.SignedAngle(F, Vector3.ProjectOnPlane(regard, U), U), -55f, 55f);
                float tangage = Mathf.Clamp(Vector3.SignedAngle(Vector3.ProjectOnPlane(regard, U), regard, R), -35f, 35f);
                tete.rotation = Quaternion.AngleAxis(lacet * 0.7f, U) * Quaternion.AngleAxis(tangage * 0.6f - penche * 0.6f, R) * tete.rotation;
            }

            // jambes : cuisse en avant = angle négatif autour de la droite du corps, genou qui plie = positif
            Jambe(cuisseD, tibiaD, piedD, s, Mathf.Sin(phase + 1.2f), k, course, R);
            Jambe(cuisseG, tibiaG, piedG, -s, Mathf.Sin(phase + Mathf.PI + 1.2f), k, course, R);

            // bras : pendants (depuis la pose en T ou en A), un peu écartés, qui balancent à l'opposé des jambes
            Bras(brasD, avantBrasD, mainD, -s, k, course, F, U, R, 1f);
            Bras(brasG, avantBrasG, mainG, s, k, course, F, U, R, -1f);
        }

        void Jambe(Transform cuisse, Transform tibia, Transform pied, float balance, float genou, float k, float course, Vector3 R)
        {
            if (cuisse == null) return;
            float amplitude = Mathf.Lerp(26f, 42f, course) * k;
            float cuisseAngle = balance * amplitude + air * 35f + accroupi * 30f + amorti * 25f;
            float genouAngle = Mathf.Max(0f, genou) * Mathf.Lerp(35f, 75f, course) * k + 4f + air * 60f + accroupi * 55f + amorti * 45f;
            cuisse.rotation = Quaternion.AngleAxis(-cuisseAngle, R) * cuisse.rotation;
            if (tibia != null) tibia.rotation = Quaternion.AngleAxis(genouAngle, R) * tibia.rotation;
            if (pied != null) pied.rotation = Quaternion.AngleAxis(cuisseAngle - genouAngle, R) * pied.rotation;
        }

        void Bras(Transform bras, Transform avantBras, Transform main, float balance, float k, float course, Vector3 F, Vector3 U, Vector3 R, float cote)
        {
            if (bras == null || avantBras == null) return;
            Vector3 actuel = (avantBras.position - bras.position).normalized;
            Vector3 voulu = (-U * 0.94f + R * cote * 0.22f + F * 0.04f + U * air * 0.5f + R * cote * air * 0.5f).normalized;
            bras.rotation = Quaternion.FromToRotation(actuel, voulu) * bras.rotation;
            float amplitude = Mathf.Lerp(20f, 45f, course) * k;
            bras.rotation = Quaternion.AngleAxis(-balance * amplitude, R) * bras.rotation;
            avantBras.rotation = Quaternion.AngleAxis(-(12f + course * 70f + Mathf.Max(0f, -balance) * 25f * k), R) * avantBras.rotation;
        }

        void AnimerCorps(Vector3 F, Vector3 U, float k, float course)
        {
            float s = Mathf.Sin(phase), c = Mathf.Cos(phase);
            Vector3 hauteur = Vector3.up * (Mathf.Abs(c) * 0.05f * k - accroupi * 0.12f - amorti * 0.1f);
            modele.localPosition = positionModele + hauteur;
            modele.localRotation = modele.localRotation * Quaternion.Euler(6f * k + 10f * course + accroupi * 8f, 0f, s * 3f * k);
            float ecrase = 1f - amorti * 0.08f;
            modele.localScale = new Vector3(echelleModele.x * (2f - ecrase), echelleModele.y * ecrase, echelleModele.z * (2f - ecrase));
        }

        // ------------------------------------------------------------------ bras sur l'arme

        void TenirArme(Vector3 F, Vector3 U, Vector3 R)
        {
            if (vue == null) vue = GetComponent<VueArme>();
            if (vue == null || !vue.Visible) return;
            if (vue.EnLancer)
            {
                if (brasD != null) DeuxOs(brasD, avantBrasD, mainD, vue.MainLancer, brasD.position - U * 0.5f + R * 0.4f - F * 0.3f);
                return;
            }
            if (brasD != null) DeuxOs(brasD, avantBrasD, mainD, vue.PoigneeDroite, brasD.position - U * 0.6f + R * 0.45f - F * 0.2f);
            if (brasG != null && vue.DeuxMains) DeuxOs(brasG, avantBrasG, mainG, vue.PoigneeGauche, brasG.position - U * 0.6f - R * 0.45f - F * 0.1f);
            // les mains suivent l'orientation de l'arme (le poignet ne se tord pas)
        }

        /// <summary>
        /// Cinématique inverse à deux os : plie le coude pour que la main atteigne la cible,
        /// oriente le bras vers elle et tourne le coude vers le point d'appui <paramref name="pole"/>.
        /// Indépendant des axes du squelette (tout se fait dans l'espace du monde).
        /// </summary>
        static void DeuxOs(Transform haut, Transform milieu, Transform bout, Vector3 cible, Vector3 pole)
        {
            Vector3 a = haut.position, b = milieu.position, c = bout.position;
            float lab = (b - a).magnitude, lbc = (c - b).magnitude;
            if (lab < 1e-4f || lbc < 1e-4f) return;
            float lat = Mathf.Clamp((cible - a).magnitude, 0.01f, lab + lbc - 0.001f);

            // 1. angle du coude
            float cosVoulu = Mathf.Clamp((lab * lab + lbc * lbc - lat * lat) / (2f * lab * lbc), -1f, 1f);
            float interieurVoulu = Mathf.Acos(cosVoulu) * Mathf.Rad2Deg;
            float interieurActuel = Vector3.Angle(a - b, c - b);
            Vector3 axe = Vector3.Cross(b - a, c - b);
            if (axe.sqrMagnitude < 1e-8f) axe = Vector3.Cross(b - a, pole - a);
            if (axe.sqrMagnitude < 1e-8f) return;
            milieu.rotation = Quaternion.AngleAxis(interieurActuel - interieurVoulu, axe.normalized) * milieu.rotation;

            // 2. viser la cible
            c = bout.position;
            haut.rotation = Quaternion.FromToRotation(c - a, cible - a) * haut.rotation;

            // 3. coude vers le point d'appui
            Vector3 axeCible = (cible - a).normalized;
            Vector3 coude = Vector3.ProjectOnPlane(milieu.position - a, axeCible);
            Vector3 appui = Vector3.ProjectOnPlane(pole - a, axeCible);
            if (coude.sqrMagnitude > 1e-6f && appui.sqrMagnitude > 1e-6f)
                haut.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(coude, appui, axeCible), axeCible) * haut.rotation;
        }
    }
}
