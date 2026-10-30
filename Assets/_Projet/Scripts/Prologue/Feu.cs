using EchosEther.Jeu;
using UnityEngine;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Un feu (débris qui brûlent, conduite de gaz percée) : des flammes simulées qui se chevauchent (flipbook Unity Labs,
    /// en boucle), des braises qui montent, une fumée sombre au-dessus, une lumière orange qui vacille.
    /// Le feu monte selon +Y depuis l'objet. Même rendu que vitrine/effets-colosse.js (fx.feu).
    /// </summary>
    public class Feu : MonoBehaviour
    {
        public float taille = 1f;
        public bool actif = true;
        [Tooltip("Lumière du feu (lumens)")] public float lumens = 12000f;
        Light lumiere;
        float reste, braises, fumee;

        void Start()
        {
            lumiere = Formes.Lumiere(transform, "Lumière du feu", new Color(1f, 0.54f, 0.23f), lumens, 7f * taille);
            lumiere.transform.localPosition = Vector3.up * 0.6f * taille;
        }

        void Update()
        {
            float t = Time.time, dt = Time.deltaTime;
            lumiere.intensity = actif ? lumens * taille * (0.75f + 0.25f * Mathf.Sin(t * 23f) * Mathf.Sin(t * 7.1f)) : 0f;
            if (!actif) return;
            var p = EffetsColosse.Pilote.I;
            reste += dt * 5f;
            while (reste >= 1f)
            {
                reste -= 1f;
                p.Emettre(p.flammes, transform.TransformPoint(new Vector3(Random.Range(-0.12f, 0.12f), 0.45f, Random.Range(-0.12f, 0.12f)) * taille),
                    transform.up * 0.1f, Random.Range(0.8f, 1.1f), 1.4f * taille, new Color(1f, 0.85f, 0.65f, 0.9f));
            }
            braises += dt * 14f;
            while (braises >= 1f)
            {
                braises -= 1f;
                p.Emettre(p.lueurs, transform.TransformPoint(new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.2f, 0.6f), Random.Range(-0.2f, 0.2f)) * taille),
                    new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(0.8f, 2f), Random.Range(-0.4f, 0.4f)), Random.Range(1f, 2f), 0.035f, new Color(1f, 0.48f, 0.16f));
            }
            fumee += dt * 3f;
            while (fumee >= 1f)
            {
                fumee -= 1f;
                p.Emettre(p.fumeeFine, transform.position + transform.up * taille + new Vector3(Random.Range(-0.1f, 0.1f), 0f, Random.Range(-0.1f, 0.1f)),
                    new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0.6f, 1f), Random.Range(-0.1f, 0.1f)), Random.Range(2.5f, 3.5f), 2.2f * taille, new Color(0.35f, 0.33f, 0.31f, 0.55f));
            }
        }
    }
}
