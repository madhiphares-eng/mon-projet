using EchosEther.Combat;
using EchosEther.Jeu;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Le propulseur d'une navette ou d'un vaisseau : une flamme de plasma (deux cônes turbulents), sa lueur,
    /// sa lumière et des grains de plasma. Le jet part vers l'arrière (-Z) ; l'objet est placé sur la tuyère.
    /// </summary>
    public class Propulseur : MonoBehaviour
    {
        [Range(0f, 1.5f)] public float puissance = 1f;
        public Color couleur = new Color(0.44f, 0.72f, 1f);
        public Color coeur = new Color(0.92f, 0.96f, 1f);
        public float longueur = 1.6f, rayon = 0.28f;
        [Tooltip("Lumière à pleine puissance (lumens)")] public float lumens = 20000f;
        Transform[] flammes;
        Material[] materiaux;
        Light lumiere;
        ParticleSystem plasma;
        float reste;

        void Start()
        {
            flammes = new Transform[2];
            materiaux = new Material[2];
            for (int i = 0; i < 2; i++)
            {
                float r = i == 0 ? rayon : rayon * 0.55f, l = i == 0 ? longueur : longueur * 0.6f;
                var go = new GameObject(i == 0 ? "Flamme" : "Cœur de la flamme");
                go.transform.SetParent(transform, false);
                go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);    // l'axe du cône (+Y) devient +Z ; la pointe part vers -Z
                go.AddComponent<MeshFilter>().sharedMesh = EffetsColosse.Tronc(r * 0.12f, r, l, 28, true);
                var rendu = go.AddComponent<MeshRenderer>();
                rendu.shadowCastingMode = ShadowCastingMode.Off;
                materiaux[i] = EffetsColosse.Volume(couleur * 2f, coeur * 2f, 1.6f, 0.55f, 0.02f, 0.8f, 16f, 34f);
                rendu.sharedMaterial = materiaux[i];
                flammes[i] = go.transform;
            }
            var halo = Formes.Primitive(PrimitiveType.Sphere, transform, Formes.Lueur(couleur * 1.5f, 2.5f, 0.3f), "Lueur de la tuyère");
            halo.transform.localScale = Vector3.one * rayon * 2.2f;
            lumiere = Formes.Lumiere(transform, "Lumière du propulseur", couleur, lumens, 6f);
            lumiere.transform.localPosition = new Vector3(0f, 0f, -0.5f);
            plasma = EffetsColosse.Systeme(transform, "Plasma", EffetsColosse.Particules(EffetsColosse.TextureLueur, true, 0f), false, 0f,
                EffetsColosse.Degrade(coeur, couleur, 0.05f, 0.4f), AnimationCurve.Linear(0f, 1f, 1f, 0.2f), 300);
        }

        void Update()
        {
            float p = Mathf.Max(0f, puissance), t = Time.time;
            float vacille = 0.92f + 0.08f * Mathf.Sin(t * 61f) * Mathf.Sin(t * 17.3f);
            for (int i = 0; i < 2; i++)
            {
                flammes[i].localScale = new Vector3(1f, Mathf.Max(0.05f, p) * vacille, 1f);
                materiaux[i].SetFloat("_Intensite", p * vacille * (i == 0 ? 1f : 1.6f));
            }
            lumiere.intensity = lumens * p * vacille;
            // les grains de plasma qui s'échappent
            reste += Time.deltaTime * 40f * p;
            while (reste >= 1f)
            {
                reste -= 1f;
                var e = new ParticleSystem.EmitParams
                {
                    position = transform.position - transform.forward * longueur * 0.2f,
                    velocity = EffetsColosse.DansLeCone(-transform.forward, 7f) * Random.Range(6f, 11f) * p,
                    startLifetime = Random.Range(0.12f, 0.3f),
                    startSize = rayon * 0.9f,
                    startColor = new Color(1f, 1f, 1f, 0.35f),
                    applyShapeToPosition = false,
                };
                plasma.Emit(e, 1);
            }
        }
    }
}
