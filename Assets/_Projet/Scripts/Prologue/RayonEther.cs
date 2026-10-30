using EchosEther.Combat;
using EchosEther.Jeu;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Le rayon d'éther de la relique : une colonne de lumière cyan, des grains qui montent en spirale,
    /// une onde au sol toutes les 1,6 s. Il monte selon +Y depuis l'objet.
    /// </summary>
    public class RayonEther : MonoBehaviour
    {
        public float hauteur = 6f, rayon = 0.45f;
        public Color couleur = new Color(0.37f, 0.95f, 1f);
        [Range(0f, 1.5f)] public float intensite = 1f;
        [Tooltip("Lumière (lumens)")] public float lumens = 15000f;
        Material faisceau, coeur;
        Light lumiere;
        ParticleSystem grains;
        float reste, onde;

        void Start()
        {
            faisceau = Ajouter("Faisceau", EffetsColosse.Tronc(rayon * 1.25f, rayon, hauteur, 32),
                EffetsColosse.Volume(couleur * 1.2f, Color.white * 1.5f, 2.5f, 0.04f, 0.25f, 0.7f, 1.6f, 22f));
            coeur = Ajouter("Cœur", EffetsColosse.Tronc(rayon * 0.35f, rayon * 0.25f, hauteur, 16),
                EffetsColosse.Volume(couleur * 1.6f, Color.white * 2f, 2.5f, 0.04f, 0.25f, 0.7f, 2.1f, 22f));
            lumiere = Formes.Lumiere(transform, "Lumière de la relique", couleur, lumens, 9f);
            lumiere.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            grains = EffetsColosse.Systeme(transform, "Grains d'éther", EffetsColosse.Particules(EffetsColosse.TextureLueur, true, 0f), false, 0f,
                EffetsColosse.Degrade(new Color(0.9f, 1f, 1f), couleur, 0.2f, 0.55f), AnimationCurve.Linear(0f, 1f, 1f, 0.3f), 400);
            var frein = grains.limitVelocityOverLifetime;
            frein.enabled = true;
            frein.drag = 0.2f;
        }

        Material Ajouter(string nom, Mesh maillage, Material m)
        {
            var go = new GameObject(nom);
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = maillage;
            var r = go.AddComponent<MeshRenderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.sharedMaterial = m;
            return m;
        }

        void Update()
        {
            float t = Time.time, k = intensite * (0.85f + 0.15f * Mathf.Sin(t * 2.1f));
            faisceau.SetFloat("_Intensite", k);
            coeur.SetFloat("_Intensite", 1.6f * k);
            lumiere.intensity = lumens * k;
            onde += Time.deltaTime / 1.6f;
            if (onde >= 1f && intensite > 0.05f)
            {
                onde -= 1f;
                EffetsCombat.Anneau(transform.position + transform.up * 0.02f, transform.up, rayon + 2.2f, couleur * intensite, 1.6f);
            }
            reste += Time.deltaTime * 18f * intensite;
            while (reste >= 1f)
            {
                reste -= 1f;
                float a = Random.Range(0f, Mathf.PI * 2f), r = rayon * Random.Range(0.6f, 2.2f);
                var e = new ParticleSystem.EmitParams
                {
                    position = transform.TransformPoint(new Vector3(Mathf.Cos(a) * r, Random.Range(0f, 0.3f), Mathf.Sin(a) * r)),
                    velocity = transform.TransformDirection(new Vector3(-Mathf.Sin(a) * 0.5f, Random.Range(0.8f, 2.2f), Mathf.Cos(a) * 0.5f)),
                    startLifetime = Random.Range(1.5f, 3f),
                    startSize = 0.07f,
                    startColor = Color.white,
                    applyShapeToPosition = false,
                };
                grains.Emit(e, 1);
            }
        }
    }
}
