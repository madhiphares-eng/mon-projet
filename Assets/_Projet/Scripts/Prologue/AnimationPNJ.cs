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
    /// </summary>
    public class AnimationPNJ : MonoBehaviour
    {
        public Animator animateur;
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
    }
}
