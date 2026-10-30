using EchosEther.Combat;
using EchosEther.Jeu;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Un panneau abîmé qui crache des étincelles à intervalles irréguliers, parfois en rafale.
    /// Les étincelles partent vers l'avant (+Z) de l'objet, rebondissent au sol et laissent un peu de fumée.
    /// </summary>
    public class EtincellesPanne : MonoBehaviour
    {
        [Tooltip("Gerbes par seconde, en moyenne")] public float cadence = 0.6f;
        public Color couleur = new Color(1f, 0.76f, 0.42f);
        public bool actif = true;
        float attente;

        void OnEnable() => attente = Random.Range(0.2f, 1f);

        void Update()
        {
            if (!actif) return;
            attente -= Time.deltaTime;
            if (attente > 0f) return;
            EffetsColosse.Gerbe(transform.position, transform.forward, couleur, Random.Range(10, 30), Random.Range(0.6f, 1.2f));
            attente = Random.value < 0.35f ? Random.Range(0.05f, 0.15f) : Random.Range(0.6f, 2.6f) / Mathf.Max(0.05f, cadence);
        }
    }
}
