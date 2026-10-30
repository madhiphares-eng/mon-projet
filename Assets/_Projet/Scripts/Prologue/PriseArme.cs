using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Le bot de prise des armes, sur un personnage Humanoid : les mains vont exactement sur l'arme, quel que soit le corps.
    /// C'est l'arme qui commande :
    /// - en visée (mode Epaule), la crosse est posée au creux de l'épaule droite et le canon pointe devant le personnage ;
    /// - arme basse (mode Main), l'arme suit la main droite de l'animation, orientée par les deux mains ;
    /// - pistolet (mode Attache), le canon dans l'axe de l'avant-bras droit.
    /// Puis les bras se replient par cinématique inverse (deux os, le coude garde sa direction) et chaque paume se pose
    /// sur sa prise (<see cref="ArmeTenue"/>) ; la main gauche glisse sur le rail du garde-main si le bras est court,
    /// et l'arme s'écarte du ventre si elle le frôle. La paume est mesurée sur le squelette (main, doigts) : rien à régler.
    /// Au démarrage, le bot cherche l'appui de crosse qui donne les plus belles visées pour ce corps (CalibrerAppui),
    /// puis vérifie que la peau des mains suit leurs os ; sinon, le personnage ne prend pas d'arme (PeauSuit).
    /// « Contrôler les prises (bot) » écrit un rapport de toutes les animations d'arme.
    /// Même calcul que le navigateur (vitrine/prise-armes.js). S'exécute après l'animation (LateUpdate).
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class PriseArme : MonoBehaviour
    {
        public enum Mode { Epaule, Main, Attache }

        public Animator animateur;
        public ArmeTenue arme;
        public Mode mode = Mode.Main;
        [Range(0f, 1f)] public float poidsGauche = 1f;
        [Tooltip("En visée : de combien le canon se relève (degrés), pour le recul")] public float tangage;
        [Tooltip("L'appui de la crosse par rapport à l'épaule (gauche, bas, avant), pour un bras de 0,547 m")]
        public Vector3 appui = new Vector3(0.055f, 0.01f, 0.06f);
        public bool actif = true;
        [Tooltip("Les animations de visée à essayer au démarrage pour calibrer l'appui (Fusil_Visee, Fusil_Marche_Visee…)")]
        public AnimationClip[] clipsCalibrage;

        const float LongueurBrasMannequin = 0.547f;

        struct Paume { public Vector3 centre, doigts, normale; public bool aNormale; public float longueur; }
        public struct Bras { public float etirement, coude, ecart, glisse; }

        Transform brasD, avantD, mainD, brasG, avantG, mainG, hanches, cou;
        Paume paumeD, paumeG;
        Vector3 devantLocal;
        float k = 1f;
        Quaternion memoire;
        bool memoireOk, pret;

        /// <summary>Le résultat de la dernière image (pour l'interface ou le bot).</summary>
        public Bras DerniereDroite { get; private set; }
        public Bras DerniereGauche { get; private set; }

        void Start()
        {
            if (animateur == null) animateur = GetComponentInChildren<Animator>();
            if (animateur == null || !animateur.isHuman) { enabled = false; return; }
            brasD = animateur.GetBoneTransform(HumanBodyBones.RightUpperArm);
            avantD = animateur.GetBoneTransform(HumanBodyBones.RightLowerArm);
            mainD = animateur.GetBoneTransform(HumanBodyBones.RightHand);
            brasG = animateur.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            avantG = animateur.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            mainG = animateur.GetBoneTransform(HumanBodyBones.LeftHand);
            hanches = animateur.GetBoneTransform(HumanBodyBones.Hips);
            cou = animateur.GetBoneTransform(HumanBodyBones.Neck) ?? animateur.GetBoneTransform(HumanBodyBones.Head);
            paumeD = Calibrer(mainD, avantD, HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightIndexProximal, HumanBodyBones.RightLittleProximal, true);
            paumeG = Calibrer(mainG, avantG, HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftLittleProximal, false);
            float longueur = Vector3.Distance(brasD.position, avantD.position) + Vector3.Distance(avantD.position, mainD.position);
            k = longueur / LongueurBrasMannequin;
            // le regard du personnage, dans son repère (de la cuisse droite à la gauche, puis vers l'avant)
            var cd = animateur.GetBoneTransform(HumanBodyBones.RightUpperLeg); var cg = animateur.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Vector3 face = cd != null && cg != null ? Vector3.Cross(cd.position - cg.position, Vector3.up) : transform.forward;
            face.y = 0f;
            devantLocal = transform.InverseTransformDirection(face.sqrMagnitude > 1e-8f ? face.normalized : transform.forward);
            NoterPeau();
            pret = true;
            if (clipsCalibrage != null && clipsCalibrage.Length > 0 && arme != null) CalibrerAppui();
        }

        // la paume d'une main : centre, direction des doigts et normale, dans le repère de l'os de la main
        Paume Calibrer(Transform main, Transform avantBras, HumanBodyBones majeur, HumanBodyBones index, HumanBodyBones petit, bool droite)
        {
            var p = new Paume();
            var tMaj = animateur.GetBoneTransform(majeur) ?? animateur.GetBoneTransform(index);
            Vector3 doigts; float longueur;
            if (tMaj != null) { doigts = tMaj.position - main.position; longueur = doigts.magnitude; doigts /= Mathf.Max(longueur, 1e-5f); }
            else { doigts = (main.position - avantBras.position).normalized; longueur = Vector3.Distance(main.position, avantBras.position) * 0.35f; }
            var tIdx = animateur.GetBoneTransform(index); var tPet = animateur.GetBoneTransform(petit);
            Vector3 centre = main.position + doigts * longueur * 0.5f;
            if (tIdx != null && tPet != null)
            {
                Vector3 travers = (tIdx.position - tPet.position).normalized;         // vers le pouce
                // Unity est en main gauche : la paume de la main droite est Cross(doigts, travers), celle de la gauche l'inverse
                Vector3 n = droite ? Vector3.Cross(doigts, travers) : Vector3.Cross(travers, doigts);
                n = (n - doigts * Vector3.Dot(n, doigts)).normalized;
                centre += n * longueur * 0.12f;
                p.normale = Quaternion.Inverse(main.rotation) * n;
                p.aNormale = true;
            }
            p.centre = main.InverseTransformPoint(centre);
            p.doigts = Quaternion.Inverse(main.rotation) * doigts;
            p.longueur = longueur;
            return p;
        }

        Vector3 CentrePaume(Transform main, Paume p) => main.TransformPoint(p.centre);
        Vector3 Devant() { var f = transform.TransformDirection(devantLocal); f.y = 0f; return f.sqrMagnitude > 1e-8f ? f.normalized : Vector3.forward; }

        // l'orientation de l'os de la main pour que sa paume épouse la prise
        Quaternion RotationMain(Transform main, Paume p, Transform prise)
        {
            if (!p.aNormale) return Quaternion.FromToRotation(main.rotation * p.doigts, prise.forward) * main.rotation;
            return Quaternion.LookRotation(prise.forward, prise.up) * Quaternion.Inverse(Quaternion.LookRotation(p.doigts, p.normale));
        }

        static Bras Ik(Transform haut, Transform milieu, Transform bout, Vector3 cible, Vector3 pole)
        {
            Vector3 S = haut.position, E = milieu.position, W = bout.position;
            float a = Vector3.Distance(S, E), b = Vector3.Distance(E, W);
            Vector3 vers = cible - S;
            float dist = vers.magnitude, d = Mathf.Clamp(dist, Mathf.Abs(a - b) + 1e-4f, a + b - 1e-4f);
            vers /= Mathf.Max(dist, 1e-6f);
            float x = (a * a - b * b + d * d) / (2f * d), h = Mathf.Sqrt(Mathf.Max(a * a - x * x, 0f));
            Vector3 p = pole - S; p -= vers * Vector3.Dot(p, vers);
            if (p.sqrMagnitude < 1e-10f) { p = Vector3.up - vers * Vector3.Dot(Vector3.up, vers); }
            p.Normalize();
            Vector3 E2 = S + vers * x + p * h;
            haut.rotation = Quaternion.FromToRotation(E - S, E2 - S) * haut.rotation;
            Vector3 E3 = milieu.position;
            milieu.rotation = Quaternion.FromToRotation(bout.position - E3, S + vers * d - E3) * milieu.rotation;
            return new Bras { etirement = dist / (a + b), coude = Mathf.Acos(Mathf.Clamp((a * a + b * b - d * d) / (2f * a * b), -1f, 1f)) * Mathf.Rad2Deg };
        }

        Bras EtatBras(Transform haut, Transform milieu, Transform bout)
        {
            float a = Vector3.Distance(haut.position, milieu.position), b = Vector3.Distance(milieu.position, bout.position), d = Vector3.Distance(haut.position, bout.position);
            return new Bras { etirement = d / (a + b), coude = Mathf.Acos(Mathf.Clamp((a * a + b * b - d * d) / (2f * a * b), -1f, 1f)) * Mathf.Rad2Deg };
        }

        Bras Poser(Transform haut, Transform milieu, Transform main, Paume p, Transform prise, Vector3 position)
        {
            Quaternion rot = RotationMain(main, p, prise);
            Vector3 poignet = position - rot * Vector3.Scale(p.centre, main.lossyScale);
            var r = Ik(haut, milieu, main, poignet, milieu.position);
            main.rotation = rot;
            r.ecart = Vector3.Distance(CentrePaume(main, p), position);
            return r;
        }

        // la main gauche glisse le long du rail si le bras ne l'atteint pas
        Bras PoserGauche()
        {
            Vector3 local = arme.transform.InverseTransformPoint(arme.priseGauche.position);
            float meilleur = local.z, score = float.MaxValue, portee = (Vector3.Distance(brasG.position, avantG.position) + Vector3.Distance(avantG.position, mainG.position)) * 0.97f;
            for (int i = 0; i <= 12; i++)
            {
                float z = Mathf.Lerp(arme.railMin, arme.railMax, i / 12f);
                float dd = Vector3.Distance(arme.transform.TransformPoint(new Vector3(local.x, local.y, z)), brasG.position);
                float s = Mathf.Max(0f, dd - portee) * 10f + Mathf.Abs(z - local.z);
                if (s < score) { score = s; meilleur = z; }
            }
            var r = Poser(brasG, avantG, mainG, paumeG, arme.priseGauche, arme.transform.TransformPoint(new Vector3(local.x, local.y, meilleur)));
            r.glisse = meilleur - local.z;
            return r;
        }

        // la distance d'un point au torse (une colonne de section ovale, de la hanche au cou), négative dedans
        float DistanceTorse(Vector3 p)
        {
            Vector3 travers = brasG.position - brasD.position; float largeur = travers.magnitude; travers /= largeur;
            Vector3 face = Devant(), axe = cou.position - hanches.position;
            float t = Mathf.Clamp01(Vector3.Dot(p - hanches.position, axe) / axe.sqrMagnitude);
            Vector3 r = p - (hanches.position + axe * t);
            float ra = largeur * 0.36f, rb = largeur * 0.28f;
            return (Mathf.Sqrt(Mathf.Pow(Vector3.Dot(r, travers) / ra, 2f) + Mathf.Pow(Vector3.Dot(r, face) / rb, 2f)) - 1f) * rb;
        }

        float DistanceArme()
        {
            Vector3 debut = arme.priseDroite.position, fin = arme.bouche != null ? arme.bouche.position : debut + arme.transform.forward * 0.6f;
            float d = float.MaxValue;
            for (int i = 0; i <= 10; i++) d = Mathf.Min(d, DistanceTorse(Vector3.Lerp(debut, fin, i / 10f)));
            return d;
        }

        void LateUpdate()
        {
            if (!pret || !actif || arme == null) return;
            Tenir(appui);
        }

        /// <summary>Tenir l'arme avec cet appui de crosse (gauche, bas, avant). À appeler après l'animation.</summary>
        public void Tenir(Vector3 appuiCrosse)
        {
            Vector3 Rp = CentrePaume(mainD, paumeD), Lp = CentrePaume(mainG, paumeG);
            Vector3 d;
            Vector3 ecart = arme.transform.InverseTransformPoint(arme.priseGauche.position) - arme.transform.InverseTransformPoint(arme.priseDroite.position);
            if (mode == Mode.Epaule)
            {
                float t = tangage * Mathf.Deg2Rad;
                d = Devant() * Mathf.Cos(t) + Vector3.up * Mathf.Sin(t);
            }
            else if (mode == Mode.Attache) d = (mainD.position - avantD.position).normalized;
            else
            {
                d = (Lp - Rp).normalized;
                for (int i = 0; i < 3; i++)
                {
                    // les deux paumes ne sont pas exactement sur l'axe du canon : on corrige la direction
                    Vector3 droiteArme = Vector3.Cross(Vector3.up, d).normalized, hautArme = Vector3.Cross(d, droiteArme).normalized;
                    d = (Lp - Rp - droiteArme * ecart.x - hautArme * ecart.y).normalized;
                }
            }
            Quaternion qArme = Quaternion.LookRotation(d, Vector3.up);
            var t0 = arme.transform;
            Vector3 decalagePrise = t0.InverseTransformPoint(arme.priseDroite.position);
            Vector3 decalageCrosse = arme.crosse != null ? t0.InverseTransformPoint(arme.crosse.position) : new Vector3(0f, 0.02f, -0.38f);

            if (mode == Mode.Epaule)
            {
                Vector3 droiteArme = qArme * Vector3.right, hautArme = qArme * Vector3.up;
                Vector3 creux = brasD.position - droiteArme * appuiCrosse.x * k - hautArme * appuiCrosse.y * k + d * appuiCrosse.z * k;
                t0.SetPositionAndRotation(creux - qArme * Vector3.Scale(decalageCrosse, t0.lossyScale), qArme);
                memoireOk = false;
            }
            else if (mode == Mode.Attache)
            {
                t0.rotation = qArme;
                mainD.rotation = RotationMain(mainD, paumeD, arme.priseDroite);
                t0.position = CentrePaume(mainD, paumeD) - qArme * Vector3.Scale(decalagePrise, t0.lossyScale);
            }
            else
            {
                if (poidsGauche > 0.99f || !memoireOk) { memoire = Quaternion.Inverse(mainD.rotation) * qArme; memoireOk = true; }
                Quaternion q = poidsGauche > 0.99f ? qArme : mainD.rotation * memoire;
                t0.SetPositionAndRotation(Rp - q * Vector3.Scale(decalagePrise, t0.lossyScale), q);
                float marge = 0.03f * k, dedans = DistanceArme();
                if (dedans < marge) t0.position += Devant() * (marge - dedans);       // l'arme s'écarte du ventre
            }
            DerniereDroite = mode == Mode.Attache ? EtatBras(brasD, avantD, mainD) : Poser(brasD, avantD, mainD, paumeD, arme.priseDroite, arme.priseDroite.position);

            bool proche = arme.gaucheFacultative <= 0f || Vector3.Distance(Lp, arme.priseGauche.position) < arme.gaucheFacultative * k;
            if (poidsGauche > 0.01f && proche)
            {
                if (poidsGauche > 0.99f) DerniereGauche = PoserGauche();
                else
                {
                    Quaternion a = brasG.localRotation, b = avantG.localRotation, c = mainG.localRotation;
                    DerniereGauche = PoserGauche();
                    brasG.localRotation = Quaternion.Slerp(a, brasG.localRotation, poidsGauche);
                    avantG.localRotation = Quaternion.Slerp(b, avantG.localRotation, poidsGauche);
                    mainG.localRotation = Quaternion.Slerp(c, mainG.localRotation, poidsGauche);
                }
            }
        }

        // ------------------------------------------------------------------ le bot

        float Cout()
        {
            float c = 0f;
            c += Mathf.Max(0f, DerniereDroite.etirement - 0.95f) * 40f + Mathf.Max(0f, DerniereGauche.etirement - 0.95f) * 40f;
            c += Mathf.Max(0f, 35f - DerniereDroite.coude) * 0.08f + Mathf.Max(0f, 35f - DerniereGauche.coude) * 0.08f;
            c += Mathf.Max(0f, 0.02f - DistanceArme() / k) * 60f;
            c += Mathf.Abs(DerniereGauche.glisse) * 4f;
            return c;
        }

        // pose le personnage sur une animation, à un instant (un petit graphe à part, évalué tout de suite)
        void Echantillonner(AnimationClip clip, float u)
        {
            var g = PlayableGraph.Create("Calibrage de la prise");
            var p = AnimationClipPlayable.Create(g, clip);
            AnimationPlayableOutput.Create(g, "Calibrage", animateur).SetSourcePlayable(p);
            p.SetTime(clip.length * u);
            g.Evaluate(0f);
            g.Destroy();
        }

        /// <summary>Le bot : cherche l'appui de crosse qui donne les plus belles visées pour ce corps.</summary>
        public void CalibrerAppui()
        {
            var ancien = mode;
            mode = Mode.Epaule;
            float meilleur = float.MaxValue;
            Vector3 choix = appui;
            foreach (float gauche in new[] { 0.02f, 0.04f, 0.06f, 0.08f })
                foreach (float avant in new[] { 0f, 0.03f, 0.06f, 0.09f, 0.12f })
                    foreach (float bas in new[] { -0.03f, 0f, 0.03f, 0.06f })
                    {
                        var essai = new Vector3(gauche, bas, avant);
                        float total = (Mathf.Abs(gauche - 0.055f) + Mathf.Abs(avant - 0.06f) + Mathf.Abs(bas - 0.01f)) * 0.5f;
                        foreach (var clip in clipsCalibrage)
                        {
                            if (clip == null) continue;
                            foreach (float u in new[] { 0.2f, 0.7f })
                            {
                                Echantillonner(clip, u);
                                Tenir(essai);
                                total += Cout();
                            }
                        }
                        if (total < meilleur) { meilleur = total; choix = essai; }
                    }
            appui = choix;
            // la peau : les os tiennent l'arme, mais la peau des mains suit-elle ?
            float pirePeau = 0f;
            foreach (var clip in clipsCalibrage)
            {
                if (clip == null) continue;
                foreach (float u in new[] { 0.2f, 0.7f }) { Echantillonner(clip, u); Tenir(appui); pirePeau = Mathf.Max(pirePeau, EcartPeau()); }
            }
            mode = ancien;
            Debug.Log($"Prise de {name} : appui de la crosse (gauche {appui.x:0.00}, bas {appui.y:0.00}, avant {appui.z:0.00}), coût {meilleur:0.00}, peau des mains à {pirePeau * 100f:0} cm de leurs os.");
            if (pirePeau > SeuilPeau)
            {
                PeauSuit = false;
                actif = false;
                if (arme != null) arme.gameObject.SetActive(false);
                Debug.LogWarning($"Prise de {name} : la peau de ses mains ne suit pas ses os ({pirePeau * 100f:0} cm) ; il ne prend pas d'arme. Il faut refaire les poids de ses mains dans Blender.");
            }
        }

        // ------------------------------------------------------------------ la peau suit-elle les os ? (le bot)
        // Des os parfaitement posés ne suffisent pas : chez certains personnages, la peau ne suit pas le squelette (une main
        // liée à d'autres os, des poids posés automatiquement), et l'arme flotte devant des mains restées ailleurs. Au repos,
        // on note les sommets de chaque main visible ; dans une pose, on mesure de combien ils se sont écartés de leur os.
        // Même mesure que le navigateur : sous 5 cm chez un personnage bien lié, de 29 à 61 cm chez les autres.
        const float SeuilPeau = 0.1f;
        struct SommetMain { public int peau, indice; public bool droite; public Vector3 local; }
        SkinnedMeshRenderer[] peaux;
        readonly List<SommetMain> sommetsMains = new List<SommetMain>();
        readonly List<Vector3> sommets = new List<Vector3>();
        Mesh cuisson;

        /// <summary>Faux si le bot a vu que la peau des mains ne suit pas les os : le personnage ne prend pas d'arme.</summary>
        public bool PeauSuit { get; private set; } = true;

        // la peau déformée par les os d'aujourd'hui, en coordonnées du monde
        void Cuire(SkinnedMeshRenderer r)
        {
            r.BakeMesh(cuisson, true);
            cuisson.GetVertices(sommets);
            var t = r.transform;
            for (int i = 0; i < sommets.Count; i++) sommets[i] = t.position + t.rotation * sommets[i];
        }

        void NoterPeau()
        {
            peaux = animateur.GetComponentsInChildren<SkinnedMeshRenderer>();
            cuisson = new Mesh();
            sommetsMains.Clear();
            Vector3 pd = CentrePaume(mainD, paumeD), pg = CentrePaume(mainG, paumeG);
            float rd = paumeD.longueur * 0.75f, rg = paumeG.longueur * 0.75f;
            for (int p = 0; p < peaux.Length; p++)
            {
                Cuire(peaux[p]);
                for (int i = 0; i < sommets.Count; i++)
                {
                    if (Vector3.Distance(sommets[i], pd) < rd) sommetsMains.Add(new SommetMain { peau = p, indice = i, droite = true, local = mainD.InverseTransformPoint(sommets[i]) });
                    else if (Vector3.Distance(sommets[i], pg) < rg) sommetsMains.Add(new SommetMain { peau = p, indice = i, droite = false, local = mainG.InverseTransformPoint(sommets[i]) });
                }
            }
        }

        /// <summary>Le plus grand écart moyen entre la peau d'une main et son os, en mètres du mannequin (9 : aucune peau autour).</summary>
        public float EcartPeau()
        {
            if (peaux == null || peaux.Length == 0) return 0f;
            float sd = 0f, sg = 0f; int nd = 0, ng = 0;
            for (int p = 0; p < peaux.Length; p++)
            {
                Cuire(peaux[p]);
                foreach (var m in sommetsMains)
                {
                    if (m.peau != p || m.indice >= sommets.Count) continue;
                    float e = Vector3.Distance(sommets[m.indice], (m.droite ? mainD : mainG).TransformPoint(m.local));
                    if (m.droite) { sd += e; nd++; } else { sg += e; ng++; }
                }
            }
            if (nd == 0 || ng == 0) return 9f;
            return Mathf.Max(sd / nd, sg / ng) / k;
        }

        /// <summary>Le bot : chaque animation d'arme du héros, huit instants ; le pire de chaque mesure, dans la console.</summary>
        [ContextMenu("Contrôler les prises (bot)")]
        public void Controler()
        {
            var heros = GetComponent<HerosPrologue>();
            if (heros == null || heros.clips == null || !pret) { Debug.LogWarning("Contrôle des prises : en jeu (Play), sur le héros."); return; }
            var texte = new StringBuilder("Contrôle des prises de " + name + "\n");
            var ancien = mode;
            foreach (var clip in heros.clips)
            {
                if (clip == null) continue;
                string nom = HerosPrologue.NomCourt(clip.name);
                if (!nom.StartsWith("Fusil_")) continue;
                mode = nom.Contains("Visee") || nom == "Fusil_Tir" ? Mode.Epaule : Mode.Main;
                float pireEtirement = 0f, pireCoude = 180f, pireEcart = 0f, pireTorse = 9f, pirePeau = 0f;
                for (int i = 0; i < 8; i++)
                {
                    Echantillonner(clip, i / 8f);
                    poidsGauche = nom == "Fusil_Rechargement" && i >= 1 && i <= 6 ? 0f : 1f;
                    Tenir(appui);
                    pireEtirement = Mathf.Max(pireEtirement, Mathf.Max(DerniereDroite.etirement, DerniereGauche.etirement));
                    pireCoude = Mathf.Min(pireCoude, Mathf.Min(DerniereDroite.coude, DerniereGauche.coude));
                    pireEcart = Mathf.Max(pireEcart, Mathf.Max(DerniereDroite.ecart, poidsGauche > 0.99f ? DerniereGauche.ecart : 0f));
                    pireTorse = Mathf.Min(pireTorse, DistanceArme() / k);
                    pirePeau = Mathf.Max(pirePeau, EcartPeau());
                }
                bool bon = pireEtirement < 0.995f && pireCoude > 20f && pireEcart < 0.01f && pireTorse > -0.01f && pirePeau <= SeuilPeau;
                texte.AppendLine($"{(bon ? "OK " : "À VOIR")} {nom,-22} bras tendu {pireEtirement:0.00}  coude {pireCoude:0}°  main à {pireEcart * 100f:0.0} cm  arme/torse {pireTorse * 100f:0} cm  peau des mains à {pirePeau * 100f:0} cm");
            }
            mode = ancien;
            poidsGauche = 1f;
            Debug.Log(texte.ToString());
        }
    }
}
