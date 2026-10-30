using System.Collections.Generic;
using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Le héros du prologue, à pied : marche par rapport à la caméra (CameraTroisiemePersonne), course, accroupi,
    /// visée par-dessus l'épaule, tir, rechargement. Il joue lui-même ses animations (graphe Playables, fondus enchaînés) :
    /// pas d'Animator Controller à monter. Il suffit de lui donner les clips de la bibliothèque (UAL1, UAL2, Humanoid)
    /// et du studio maison (EE_Animations) : clic droit sur le composant, « Trouver les animations du prologue ».
    /// Le fusil (facultatif) est un enfant de la main droite ; sa bouche sert de départ aux tirs.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class HerosPrologue : MonoBehaviour
    {
        public CameraTroisiemePersonne vue;
        public Animator animateur;
        [Tooltip("Les animations, trouvées par leur nom (Idle_Loop, Walk_Loop, Fusil_Visee…)")] public AnimationClip[] clips;
        [Header("Vitesses (m/s), accordées aux animations")]
        public float vitesseMarche = 1.5f, vitesseCourse = 3.1f, vitesseSprint = 5.6f, vitesseAccroupi = 1.25f, vitesseVisee = 1.5f;
        public float gravite = 20f;
        [Header("Le fusil")]
        public bool arme = true;
        public GameObject fusil;
        public Transform bouche;
        public Color couleurTir = new Color(0.31f, 0.85f, 1f);
        [Tooltip("Coups par seconde")] public float cadence = 7f;
        public LayerMask cibles = ~0;

        public bool Accroupi { get; private set; }
        public bool Visee { get; private set; }
        public float Vitesse { get; private set; }

        CharacterController corps;
        FusilDeuxMains tenue;
        float vy, attenteTir, rechargement, hauteurDebout;
        Vector3 direction;
        PlayableGraph graphe;
        AnimationMixerPlayable mixeur;
        readonly Dictionary<string, int> entrees = new Dictionary<string, int>();
        readonly List<float> poids = new List<float>();
        int actif = -1;

        void Awake()
        {
            corps = GetComponent<CharacterController>();
            hauteurDebout = corps.height;
            if (animateur == null) animateur = GetComponentInChildren<Animator>();
            if (fusil != null) tenue = fusil.GetComponent<FusilDeuxMains>();
        }

        void Start()
        {
            if (animateur == null || clips == null || clips.Length == 0) return;
            graphe = PlayableGraph.Create("Héros du prologue");
            graphe.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            mixeur = AnimationMixerPlayable.Create(graphe, clips.Length);
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null) continue;
                var p = AnimationClipPlayable.Create(graphe, clips[i]);
                graphe.Connect(p, 0, mixeur, i);
                mixeur.SetInputWeight(i, 0f);
                entrees[clips[i].name] = i;
                while (poids.Count <= i) poids.Add(0f);
            }
            var sortie = AnimationPlayableOutput.Create(graphe, "Animation", animateur);
            sortie.SetSourcePlayable(mixeur);
            graphe.Play();
        }

        void OnDestroy() { if (graphe.IsValid()) graphe.Destroy(); }

        void Update()
        {
            float dt = Time.deltaTime;
            if (EntreesJeu.Accroupir()) Accroupi = !Accroupi;
            if (EntreesJeu.ChoixArme() == 0) arme = !arme;
            Visee = arme && EntreesJeu.Viser();
            if (arme && EntreesJeu.Recharger() && rechargement <= 0f) rechargement = 2.4f;
            rechargement -= dt;

            // le déplacement, par rapport à la caméra
            Vector2 d = Entrees.Deplacement();
            Vector3 avant = vue != null ? vue.AvantAPlat : transform.forward;
            Vector3 droite = Vector3.Cross(Vector3.up, avant);
            Vector3 voulu = avant * d.y + droite * d.x;
            bool bouge = voulu.sqrMagnitude > 0.01f;
            bool sprint = Entrees.Course() && !Visee && !Accroupi && d.y > 0.1f;
            float vmax = Accroupi ? vitesseAccroupi : Visee ? vitesseVisee : sprint ? vitesseSprint : vitesseCourse;
            Vitesse = Mathf.Lerp(Vitesse, bouge ? vmax * Mathf.Clamp01(d.magnitude) : 0f, 1f - Mathf.Exp(-9f * dt));
            if (bouge) direction = voulu.normalized;
            vy = corps.isGrounded ? -1f : vy - gravite * dt;
            corps.Move((direction * Vitesse + Vector3.up * vy) * dt);

            // accroupi : la capsule se tasse
            corps.height = Mathf.Lerp(corps.height, Accroupi ? hauteurDebout * 0.65f : hauteurDebout, 1f - Mathf.Exp(-10f * dt));
            corps.center = new Vector3(0f, corps.height / 2f, 0f);

            // il se tourne vers sa marche, ou vers la visée
            float capVoulu = Visee && vue != null ? vue.Lacet : bouge ? Quaternion.LookRotation(direction).eulerAngles.y : transform.eulerAngles.y;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, capVoulu, 0f), 1f - Mathf.Exp(-(Visee ? 18f : 10f) * dt));

            if (vue != null) { vue.Visee = Visee; vue.Accroupi = Accroupi; vue.Course = sprint && Vitesse > vitesseCourse; }
            if (fusil != null) fusil.SetActive(arme);
            if (tenue != null) tenue.rechargement = rechargement > 0f;

            // le tir
            attenteTir -= dt;
            if (Visee && EntreesJeu.Tirer() && attenteTir <= 0f && rechargement <= 0f) Tirer();

            Animer(dt);
        }

        void Tirer()
        {
            attenteTir = 1f / Mathf.Max(0.1f, cadence);
            Vector3 cible = vue != null ? vue.PointVise(400f, cibles, out RaycastHit touche) : transform.position + transform.forward * 50f;
            Transform b = bouche != null ? bouche : tenue != null ? tenue.bouche : null;
            Vector3 depart = b != null ? b.position : transform.position + Vector3.up * 1.4f;
            var champ = touche.collider != null ? touche.collider.GetComponentInParent<ChampDeForce>() : null;
            EffetsColosse.Tir(depart, cible, couleurTir, 90f, touche.collider != null ? touche.normal : Vector3.zero,
                p => { if (champ != null) champ.Toucher(p); });
            CameraTroisiemePersonne.Recul(0.7f);
        }

        // ------------------------------------------------------------------ les animations : la même logique que vitrine/vues.html
        void Animer(float dt)
        {
            if (!graphe.IsValid()) return;
            float v = Vitesse;
            if (arme && rechargement > 0f) Jouer("Fusil_Rechargement");
            else if (arme && Visee)
            {
                if (Accroupi) Jouer("Fusil_Accroupi_Visee");
                else if (v > 0.3f) Jouer("Fusil_Marche_Visee", v / vitesseVisee);
                else Jouer(attenteTir > 0f ? "Fusil_Tir" : "Fusil_Visee");
            }
            else if (Accroupi) Jouer(v > 0.3f ? "Crouch_Fwd_Loop" : arme ? "Fusil_Accroupi_Visee" : "Crouch_Idle_Loop", v > 0.3f ? v / vitesseAccroupi : 1f);
            else if (v > (vitesseCourse + vitesseSprint) / 2f) Jouer(arme ? "Fusil_Course" : "Sprint_Loop", arme ? v / 3.6f : v / vitesseSprint);
            else if (v > 2f) Jouer(arme ? "Fusil_Course" : "Jog_Fwd_Loop", v / vitesseCourse);
            else if (v > 0.3f) Jouer(arme ? "Fusil_Marche" : "Walk_Loop", v / vitesseMarche);
            else Jouer(arme ? "Fusil_Repos" : "Idle_Loop");

            // les fondus enchaînés : l'animation active monte, les autres s'effacent
            float pas = dt / 0.22f;
            for (int i = 0; i < poids.Count; i++)
            {
                poids[i] = Mathf.MoveTowards(poids[i], i == actif ? 1f : 0f, pas);
                mixeur.SetInputWeight(i, poids[i]);
            }
        }

        void Jouer(string nom, float vitesse = 1f)
        {
            if (!entrees.TryGetValue(nom, out int i)) return;
            var p = mixeur.GetInput(i);
            p.SetSpeed(vitesse);
            if (i == actif) return;
            p.SetTime(0);
            actif = i;
        }

#if UNITY_EDITOR
        [ContextMenu("Trouver les animations du prologue")]
        void TrouverAnimations()
        {
            var trouves = new List<AnimationClip>();
            foreach (string fichier in new[] { "UAL1_Standard", "UAL2_Standard", "EE_Animations" })
                foreach (string guid in UnityEditor.AssetDatabase.FindAssets(fichier + " t:Model"))
                    foreach (var o in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)))
                        if (o is AnimationClip c && !c.name.StartsWith("__preview__")) trouves.Add(c);
            clips = trouves.ToArray();
            Debug.Log($"Héros du prologue : {clips.Length} animations trouvées.");
        }
#endif
    }
}
