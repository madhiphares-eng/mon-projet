using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Un PNJ qui joue ses animations tout seul, sans Animator Controller : un androïde qui monte la garde puis balaie
    /// la salle, un détenu à genoux, un gris curieux… Les clips sont joués l'un après l'autre, en boucle, avec un fondu
    /// enchaîné. Un seul clip : il boucle. Le départ est décalé au hasard pour que deux PNJ ne bougent pas ensemble.
    /// Les clips viennent de la bibliothèque (UAL1, UAL2) et du studio maison (EE_Animations), importés en Humanoid.
    /// Chaque espèce a son attente et sa démarche (docs/prologue/ANIMATIONS.md) : clic droit sur le composant,
    /// « Prendre les animations de son espèce ».
    /// </summary>
    public class AnimationPNJ : MonoBehaviour
    {
        public enum Espece { Humain, Detenu, Androide, Gris, Saurien, Ichtyen, Lourd, Longiligne, Mutant }

        /// <summary>L'attente de chaque espèce (jouées l'une après l'autre), puis sa démarche.</summary>
        public static readonly Dictionary<Espece, (string[] attentes, string marche)> Animations = new Dictionary<Espece, (string[], string)>
        {
            { Espece.Humain, (new[] { "Idle_Loop", "Idle_Talking_Loop" }, "Walk_Loop") },
            { Espece.Detenu, (new[] { "Detenu_Fatigue", "Idle_Loop" }, "Walk_Loop") },
            { Espece.Androide, (new[] { "Androide_Garde", "Androide_Scan" }, "Androide_Marche") },
            { Espece.Gris, (new[] { "Gris_Curieux", "Idle_Loop" }, "Gris_Marche") },
            { Espece.Saurien, (new[] { "Saurien_Attente" }, "Saurien_Marche") },
            { Espece.Ichtyen, (new[] { "Ichtyen_Attente" }, "Ichtyen_Marche") },
            { Espece.Lourd, (new[] { "Lourd_Attente" }, "Lourd_Marche") },
            { Espece.Longiligne, (new[] { "Longiligne_Attente" }, "Longiligne_Marche") },
            { Espece.Mutant, (new[] { "Mutant_Attente" }, "Mutant_Marche") },
        };

        /// <summary>L'espèce d'un personnage du prologue, d'après le nom de son modèle (detenu-tshirt, ichtyen, androide-lourd…).</summary>
        public static Espece EspeceDe(string nom)
        {
            nom = nom.ToLowerInvariant();
            if (nom.Contains("saurien") || nom.Contains("crocodilien")) return Espece.Saurien;
            if (nom.Contains("ichtyen")) return Espece.Ichtyen;
            if (nom.Contains("ceratopsien")) return Espece.Lourd;
            if (nom.Contains("longiligne")) return Espece.Longiligne;
            if (nom.Contains("mutant") || nom.Contains("cephalopode")) return Espece.Mutant;
            if (nom.Contains("gris")) return Espece.Gris;
            if (nom.Contains("androide") || nom.Contains("droide")) return Espece.Androide;
            if (nom.Contains("detenu") || nom.Contains("tenue")) return Espece.Detenu;
            return Espece.Humain;
        }

        public Animator animateur;
        public Espece espece;
        public AnimationClip[] clips;
        [Tooltip("Combien de fois chaque clip est joué avant de passer au suivant")] public int repetitions = 2;
        public float vitesse = 1f;
        public float fondu = 0.3f;
        public bool decaler = true;
        [Tooltip("Ajoute l'ancrage des pieds (AncragePieds) s'il manque : les pieds suivent le sol et ne glissent plus")]
        public bool ancrerLesPieds = true;

        PlayableGraph graphe;
        AnimationMixerPlayable mixeur;
        int courant;
        float temps, duree;

        void Start()
        {
            if (animateur == null) animateur = GetComponentInChildren<Animator>();
            if (animateur == null || clips == null || clips.Length == 0) return;
            if (ancrerLesPieds && animateur.isHuman && GetComponent<AncragePieds>() == null)
                gameObject.AddComponent<AncragePieds>().animateur = animateur;
            graphe = PlayableGraph.Create("PNJ " + name);
            graphe.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            mixeur = AnimationMixerPlayable.Create(graphe, clips.Length);
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null) continue;
                var p = AnimationClipPlayable.Create(graphe, clips[i]);
                p.SetSpeed(vitesse);
                p.SetApplyFootIK(true);   // l'IK des pieds intégré aux Humanoid : les pieds de la cible retrouvent ceux de l'animation
                graphe.Connect(p, 0, mixeur, i);
                mixeur.SetInputWeight(i, i == 0 ? 1f : 0f);
            }
            AnimationPlayableOutput.Create(graphe, "Animation", animateur).SetSourcePlayable(mixeur);
            Passer(0);
            if (decaler && clips[0] != null)
            {
                temps = Random.Range(0f, clips[0].length);
                mixeur.GetInput(0).SetTime(temps);
            }
            graphe.Play();
        }

        void OnDestroy() { if (graphe.IsValid()) graphe.Destroy(); }

        void Passer(int i)
        {
            courant = i;
            temps = 0f;
            duree = clips[i] != null ? clips[i].length * Mathf.Max(1, repetitions) / Mathf.Max(0.01f, vitesse) : 1f;
            mixeur.GetInput(i).SetTime(0);
        }

        void Update()
        {
            if (!graphe.IsValid()) return;
            temps += Time.deltaTime;
            if (clips.Length > 1 && temps >= duree) Passer((courant + 1) % clips.Length);
            float pas = Time.deltaTime / Mathf.Max(0.01f, fondu);
            for (int i = 0; i < clips.Length; i++)
            {
                float w = mixeur.GetInputWeight(i);
                mixeur.SetInputWeight(i, Mathf.MoveTowards(w, i == courant ? 1f : 0f, pas));
            }
        }

#if UNITY_EDITOR
        void Reset() { espece = EspeceDe(name); }

        [ContextMenu("Prendre les animations de son espèce")]
        void PrendreAnimationsEspece()
        {
            var parNom = new Dictionary<string, AnimationClip>();
            foreach (string fichier in new[] { "UAL1_Standard", "UAL2_Standard", "EE_Animations" })
                foreach (string guid in UnityEditor.AssetDatabase.FindAssets(fichier + " t:Model"))
                    foreach (var o in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)))
                        if (o is AnimationClip c && !c.name.StartsWith("__preview__")) parNom[HerosPrologue.NomCourt(c.name)] = c;
            var trouves = new List<AnimationClip>();
            foreach (string n in Animations[espece].attentes)
                if (parNom.TryGetValue(n, out var c)) trouves.Add(c);
                else Debug.LogWarning($"{name} : l'animation {n} manque (importer EE_Animations.fbx en Humanoid).");
            clips = trouves.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"{name} ({espece}) : {clips.Length} animations d'attente.");
        }
#endif
    }
}
