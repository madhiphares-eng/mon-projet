using EchosEther.Combat;
using EchosEther.Jeu;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Une fuite de vapeur (conduite percée, purge d'une machine) : un jet serré vers l'avant (+Z), qui ralentit,
    /// s'élargit et monte en se diluant.
    /// </summary>
    public class FuiteVapeur : MonoBehaviour
    {
        [Tooltip("Bouffées par seconde")] public float debit = 18f;
        [Tooltip("Vitesse de sortie (m/s)")] public float vitesse = 2.2f;
        [Tooltip("Taille des bouffées en fin de vie (m)")] public float taille = 1.3f;
        [Range(0f, 1f)] public float opacite = 0.16f;
        public Color couleur = new Color(0.78f, 0.8f, 0.82f);
        public bool actif = true;
        ParticleSystem ps;

        void Awake()
        {
            ps = EffetsColosse.Systeme(transform, "Vapeur", EffetsColosse.Particules(EffetsColosse.TextureFumee, false, 0.4f), false, -0.045f,
                EffetsColosse.Degrade(Color.white, new Color(0.8f, 0.8f, 0.8f), 0.06f, 0.65f), AnimationCurve.Linear(0f, 0.06f, 1f, 1f), 400);
            EffetsColosse.JouerFlipbook(ps, EffetsColosse.Flipbook("fumee_volutes", 8, 8), 8, 8);     // des volutes simulées (Unity Labs, CC0)
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.4f, 2.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(vitesse * 0.85f, vitesse * 1.15f);
            main.startSize = taille;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(couleur.r, couleur.g, couleur.b, Mathf.Min(1f, opacite * 3f));
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = debit;
            var forme = ps.shape;
            forme.enabled = true;
            forme.shapeType = ParticleSystemShapeType.Cone;
            forme.angle = 8f;
            forme.radius = 0.02f;
            var frein = ps.limitVelocityOverLifetime;
            frein.enabled = true;
            frein.drag = 1.4f;
            var tourne = ps.rotationOverLifetime;
            tourne.enabled = true;
            tourne.z = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);
        }

        void Update()
        {
            var emission = ps.emission;
            emission.enabled = actif;
        }
    }
}
