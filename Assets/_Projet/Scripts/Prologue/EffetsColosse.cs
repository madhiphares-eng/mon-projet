using System;
using System.Collections.Generic;
using EchosEther.Combat;
using EchosEther.Jeu;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Les effets visuels du Colosse (le prologue). Tout est fait par le code : textures, matériaux, maillages.
    /// Ici, les effets ponctuels (gerbe d'étincelles, tir d'énergie, impact), sur des systèmes de particules partagés.
    /// Les effets qui durent sont des composants à poser dans la scène (EmetteursColosse.cs) : EtincellesPanne,
    /// FuiteVapeur, Propulseur, ChampDeForce, RayonEther, Gyrophare.
    /// Même rendu que l'aperçu du navigateur (vitrine/effets-colosse.js, vitrine/banc-effets.html).
    /// Shaders : Resources/Particule.shader, Volume.shader, ChampDeForce.shader (HDRP, additifs ou fondus).
    /// </summary>
    public static class EffetsColosse
    {
        // ------------------------------------------------------------------ textures

        static Texture2D lueur, fumee, etincelle, eclair;

        /// <summary>Une lueur ronde, très douce.</summary>
        public static Texture2D TextureLueur => lueur != null ? lueur : lueur = Dessiner("Lueur", 64, (u, v) =>
        {
            float d = Distance(u, v) * 2f;
            return new Color(1f, 1f, 1f, Mathf.Max(0f, Mathf.Exp(-d * d * 4.5f) - 0.012f));
        });

        /// <summary>Une bouffée de fumée : un bruit fractal dans un disque aux bords doux.</summary>
        public static Texture2D TextureFumee => fumee != null ? fumee : fumee = Dessiner("Fumee", 128, (u, v) =>
        {
            float d = Distance(u, v) * 2f, n = Fbm(u * 5f + 3.1f, v * 5f + 1.7f, 5);
            float a = Lisse(1f, 0.35f, d + (n - 0.5f) * 0.6f) * (0.45f + 0.75f * n);
            float g = 0.75f + 0.25f * n;
            return new Color(g, g, g, Mathf.Min(1f, a));
        });

        /// <summary>Une étincelle : un trait effilé (u : le long du trait, pour le mode étiré des particules).</summary>
        public static Texture2D TextureEtincelle => etincelle != null ? etincelle : etincelle = Dessiner("Etincelle", 64, (u, v) =>
        {
            float travers = Mathf.Exp(-Mathf.Pow((v - 0.5f) * 7f, 2f));
            float le_long = Mathf.Pow(Mathf.Sin(Mathf.PI * u), 0.6f) * (0.4f + 0.6f * u);
            return new Color(1f, 1f, 1f, Mathf.Min(1f, travers * le_long * 1.3f));
        });

        /// <summary>L'éclair de bouche : une étoile à branches inégales et un cœur.</summary>
        public static Texture2D TextureEclair => eclair != null ? eclair : eclair = Dessiner("Eclair", 128, (u, v) =>
        {
            float x = u - 0.5f, y = v - 0.5f, d = Distance(u, v) * 2f, angle = Mathf.Atan2(y, x);
            float branches = Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 3f + 0.4f)), 18f) * 0.9f + Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 2.5f + 1.3f)), 30f) * 0.6f;
            return new Color(1f, 1f, 1f, Mathf.Min(1f, Mathf.Exp(-d * d * 10f) + branches * Mathf.Max(0f, 1f - d)));
        });

        static float Distance(float u, float v) => Mathf.Sqrt((u - 0.5f) * (u - 0.5f) + (v - 0.5f) * (v - 0.5f));

        static float Lisse(float a, float b, float x)
        {
            float t = Mathf.Clamp01((x - a) / (b - a));
            return t * t * (3f - 2f * t);
        }

        static float Hasard(float x, float y)
        {
            float s = Mathf.Sin(x * 127.1f + y * 311.7f) * 43758.5453f;
            return s - Mathf.Floor(s);
        }

        static float BruitLisse(float x, float y)
        {
            float ix = Mathf.Floor(x), iy = Mathf.Floor(y), fx = x - ix, fy = y - iy;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);
            float a = Hasard(ix, iy), b = Hasard(ix + 1f, iy), c = Hasard(ix, iy + 1f), d = Hasard(ix + 1f, iy + 1f);
            return a + (b - a) * fx + (c - a) * fy + (a - b - c + d) * fx * fy;
        }

        static float Fbm(float x, float y, int octaves)
        {
            float s = 0f, amplitude = 0.5f, f = 1f;
            for (int o = 0; o < octaves; o++) { s += amplitude * BruitLisse(x * f, y * f); f *= 2.03f; amplitude *= 0.5f; }
            return s;
        }

        static Texture2D Dessiner(string nom, int taille, Func<float, float, Color> dessin)
        {
            var t = new Texture2D(taille, taille, TextureFormat.RGBA32, true) { name = nom, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[taille * taille];
            for (int y = 0; y < taille; y++)
                for (int x = 0; x < taille; x++)
                    pixels[y * taille + x] = dessin((x + 0.5f) / taille, (y + 0.5f) / taille);
            t.SetPixels(pixels);
            t.Apply(true, true);
            return t;
        }

        // ------------------------------------------------------------------ matériaux

        static Shader shaderParticule, shaderVolume, shaderChamp;

        static Shader Trouver(ref Shader cache, string nom)
        {
            if (cache == null) cache = Shader.Find(nom);
            if (cache == null) Debug.LogWarning($"Shader {nom} introuvable (Resources) : effet remplacé par une lueur.");
            return cache;
        }

        /// <summary>Matériau de particules : additif (étincelles, lueurs) ou fondu (fumée, vapeur).</summary>
        public static Material Particules(Texture2D texture, bool additif, float douceur = 0.3f)
        {
            var s = Trouver(ref shaderParticule, "EchosEther/Particule");
            if (s == null) return Formes.Lueur(Color.white, 0f, 1f);
            var m = new Material(s) { name = "Particules " + texture.name, renderQueue = additif ? 3010 : 3000 };
            m.SetTexture("_MainTex", texture);
            m.SetFloat("_SrcBlend", additif ? (float)BlendMode.One : (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", additif ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_Douceur", douceur);
            return m;
        }

        /// <summary>Volume lumineux additif (flamme, rayon, faisceau), sur un maillage fait par <see cref="Tronc"/>.</summary>
        public static Material Volume(Color couleur, Color coeur, float puissanceCoeur, float fonduDebut, float fonduFin,
            float bruit = 0.8f, float defilement = 16f, float bandes = 0f)
        {
            var s = Trouver(ref shaderVolume, "EchosEther/Volume");
            if (s == null) return Formes.Lueur(couleur, 0f, 1f);
            var m = new Material(s) { name = "Volume", renderQueue = 3010 };
            m.SetColor("_Couleur", couleur);
            m.SetColor("_CouleurCoeur", coeur);
            m.SetFloat("_Coeur", puissanceCoeur);
            m.SetFloat("_FonduBas", fonduDebut);
            m.SetFloat("_FonduHaut", fonduFin);
            m.SetFloat("_Bruit", bruit);
            m.SetFloat("_Defilement", defilement);
            m.SetFloat("_Bandes", bandes);
            m.SetFloat("_Intensite", 1f);
            return m;
        }

        /// <summary>Matériau du champ de force (shader EchosEther/ChampDeForce).</summary>
        public static Material Champ(Color couleur, float largeur, float hauteur)
        {
            var s = Trouver(ref shaderChamp, "EchosEther/ChampDeForce");
            if (s == null) return Formes.Lueur(couleur, 2f, 0.1f);
            var m = new Material(s) { name = "Champ de force", renderQueue = 3010 };
            m.SetColor("_Couleur", couleur);
            m.SetVector("_Taille", new Vector4(largeur, hauteur, 0f, 0f));
            m.SetFloat("_Intensite", 1f);
            return m;
        }

        // ------------------------------------------------------------------ maillages

        /// <summary>
        /// Un tronc de cône ouvert, le long de +Y : rayonDebut en y = 0 (uv.y = 0), rayonFin en y = longueur (uv.y = 1).
        /// Avec <paramref name="origineAuBout"/>, il va de y = -longueur à 0 : son origine est à la fin (la tuyère d'une flamme).
        /// </summary>
        public static Mesh Tronc(float rayonDebut, float rayonFin, float longueur, int segments = 24, bool origineAuBout = false)
        {
            var sommets = new List<Vector3>();
            var normales = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            float decalage = origineAuBout ? -longueur : 0f;
            float pente = (rayonDebut - rayonFin) / Mathf.Max(longueur, 0.0001f);
            for (int k = 0; k <= segments; k++)
            {
                float a = k * Mathf.PI * 2f / segments;
                var radial = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var normale = (radial + Vector3.up * pente).normalized;
                sommets.Add(radial * rayonDebut + Vector3.up * decalage);
                sommets.Add(radial * rayonFin + Vector3.up * (decalage + longueur));
                normales.Add(normale); normales.Add(normale);
                uvs.Add(new Vector2((float)k / segments, 0f)); uvs.Add(new Vector2((float)k / segments, 1f));
                if (k < segments)
                {
                    int i = k * 2;
                    triangles.AddRange(new[] { i, i + 1, i + 3, i, i + 3, i + 2 });
                }
            }
            var m = new Mesh { name = "Tronc" };
            m.SetVertices(sommets);
            m.SetNormals(normales);
            m.SetUVs(0, uvs);
            m.SetTriangles(triangles, 0);
            m.RecalculateBounds();
            return m;
        }

        /// <summary>Un plan vertical : x de -largeur/2 à +largeur/2, y de 0 à hauteur, tourné vers -Z.</summary>
        public static Mesh Plan(float largeur, float hauteur)
        {
            float l = largeur / 2f;
            var m = new Mesh { name = "Plan" };
            m.SetVertices(new List<Vector3> { new Vector3(-l, 0f, 0f), new Vector3(l, 0f, 0f), new Vector3(-l, hauteur, 0f), new Vector3(l, hauteur, 0f) });
            m.SetNormals(new List<Vector3> { Vector3.back, Vector3.back, Vector3.back, Vector3.back });
            m.SetUVs(0, new List<Vector2> { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) });
            m.SetTriangles(new List<int> { 0, 2, 1, 1, 2, 3 }, 0);
            m.RecalculateBounds();
            return m;
        }

        /// <summary>Un système de particules piloté par le code (Emit), en espace monde.</summary>
        public static ParticleSystem Systeme(Transform parent, string nom, Material materiau, bool etire, float gravite,
            Gradient couleurs, AnimationCurve taille, int maximum = 1000)
        {
            var go = new GameObject(nom);
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true;                                  // toujours en marche : il ne fait qu'animer ce qu'on lui donne
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = maximum;
            main.gravityModifier = gravite;
            var emission = ps.emission;
            emission.enabled = false;
            var forme = ps.shape;
            forme.enabled = false;
            var couleur = ps.colorOverLifetime;
            couleur.enabled = true;
            couleur.color = new ParticleSystem.MinMaxGradient(couleurs);
            var tailles = ps.sizeOverLifetime;
            tailles.enabled = true;
            tailles.size = new ParticleSystem.MinMaxCurve(1f, taille);
            var rendu = go.GetComponent<ParticleSystemRenderer>();
            rendu.sharedMaterial = materiau;
            rendu.renderMode = etire ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard;
            if (etire) { rendu.velocityScale = 0.045f; rendu.lengthScale = 1f; }
            rendu.shadowCastingMode = ShadowCastingMode.Off;
            rendu.receiveShadows = false;
            ps.Play();
            return ps;
        }

        /// <summary>Un dégradé de couleur et d'opacité : apparition rapide, disparition à partir de <paramref name="sortie"/>.</summary>
        public static Gradient Degrade(Color debut, Color fin, float entree = 0.08f, float sortie = 0.55f)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(debut, 0f), new GradientColorKey(fin, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, entree), new GradientAlphaKey(1f, sortie), new GradientAlphaKey(0f, 1f) });
            return g;
        }

        // ------------------------------------------------------------------ les effets ponctuels

        /// <summary>Une gerbe d'étincelles (court-circuit, impact sur un champ), avec un éclair de lumière.</summary>
        public static void Gerbe(Vector3 position, Vector3 direction, Color couleur, int nombre = 24, float force = 1f)
        {
            var p = Pilote.I;
            var chaud = Color.Lerp(couleur, Color.white, 0.75f);
            for (int i = 0; i < nombre; i++)
            {
                p.Emettre(p.etincelles, position, DansLeCone(direction, 50f) * Random.Range(2f, 7f) * force,
                    Random.Range(0.35f, 1.1f), Random.Range(0.018f, 0.03f), Random.value < 0.5f ? chaud : couleur);
            }
            p.Emettre(p.lueurs, position, Vector3.zero, 0.12f, 0.6f, chaud);
            for (int i = 0; i < 3; i++)
                p.Emettre(p.fumees, position + Random.insideUnitSphere * 0.05f, DansLeCone(direction, 30f) * 0.4f + Vector3.up * 0.3f,
                    Random.Range(1.2f, 2f), 0.7f, new Color(0.29f, 0.29f, 0.31f, 0.35f));
            EffetsCombat.Flash(position, couleur, 6000f * force, 0.09f, 5f);
        }

        /// <summary>
        /// Un tir d'énergie : éclair de bouche, trait qui file à <paramref name="vitesse"/> m/s, puis impact à l'arrivée.
        /// <paramref name="touche"/> est appelé à l'impact (pour toucher un champ de force, faire des dégâts…).
        /// </summary>
        public static void Tir(Vector3 depart, Vector3 arrivee, Color couleur, float vitesse = 60f, Vector3 normale = default, Action<Vector3> touche = null)
        {
            var p = Pilote.I;
            Vector3 dir = (arrivee - depart).normalized;
            float duree = Vector3.Distance(depart, arrivee) / vitesse;
            p.Emettre(p.eclairs, depart + dir * 0.08f, Vector3.zero, 0.06f, 0.55f, Color.white);
            EffetsCombat.Eclair(depart, dir, couleur, 0.8f);
            p.Emettre(p.traits, depart, dir * vitesse, duree, 0.06f, Color.white);         // le cœur blanc
            p.Emettre(p.lueurs, depart, dir * vitesse, duree, 0.32f, couleur);              // le halo
            p.Prevoir(duree, () =>
            {
                Impact(arrivee, normale == default ? -dir : normale, couleur);
                touche?.Invoke(arrivee);
            });
        }

        /// <summary>L'impact d'un tir : étincelles, lueur, fumée, lumière.</summary>
        public static void Impact(Vector3 point, Vector3 normale, Color couleur)
        {
            var p = Pilote.I;
            var blanc = new Color(1f, 0.95f, 0.88f);
            for (int i = 0; i < 14; i++)
                p.Emettre(p.etincelles, point, DansLeCone(normale, 55f) * Random.Range(2f, 6f), Random.Range(0.2f, 0.55f), 0.02f,
                    Random.value < 0.5f ? blanc : couleur);
            p.Emettre(p.lueurs, point + normale * 0.03f, Vector3.zero, 0.18f, 0.8f, Color.Lerp(couleur, blanc, 0.5f));
            p.Emettre(p.fumees, point + normale * 0.05f, normale * 0.6f, 1.4f, 0.8f, new Color(0.33f, 0.33f, 0.35f, 0.3f));
            EffetsCombat.Flash(point, couleur, 5000f, 0.1f, 5f);
        }

        /// <summary>Une direction tirée au hasard dans un cône (demi-ouverture en degrés).</summary>
        public static Vector3 DansLeCone(Vector3 direction, float ouverture)
        {
            var d = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector3.up;
            var r = Quaternion.AngleAxis(Random.Range(0f, 360f), d) * Quaternion.AngleAxis(ouverture * Mathf.Sqrt(Random.value), Vector3.Cross(d, Mathf.Abs(d.y) < 0.9f ? Vector3.up : Vector3.right).normalized);
            return r * d;
        }

        // ------------------------------------------------------------------ le pilote : les systèmes partagés, les impacts à venir

        sealed class Pilote : MonoBehaviour
        {
            static Pilote instance;

            public static Pilote I
            {
                get
                {
                    if (instance == null) instance = new GameObject("Effets du Colosse").AddComponent<Pilote>();
                    return instance;
                }
            }

            public ParticleSystem etincelles, traits, lueurs, fumees, eclairs;
            readonly List<(float quand, Action action)> prevus = new List<(float, Action)>();

            void Awake()
            {
                instance = this;
                var montee = AnimationCurve.Linear(0f, 0.15f, 1f, 1f);
                var baisse = AnimationCurve.Linear(0f, 1f, 1f, 0.35f);
                etincelles = Systeme(transform, "Étincelles", Particules(TextureEtincelle, true, 0f), true, 1f,
                    Degrade(Color.white, new Color(1f, 0.45f, 0.25f), 0.01f, 0.6f), baisse, 1500);
                var rebond = etincelles.collision;
                rebond.enabled = true;
                rebond.type = ParticleSystemCollisionType.World;
                rebond.mode = ParticleSystemCollisionMode.Collision3D;
                rebond.bounce = 0.35f;
                rebond.dampen = 0.4f;
                rebond.quality = ParticleSystemCollisionQuality.Low;
                traits = Systeme(transform, "Traits", Particules(TextureEtincelle, true, 0f), true, 0f,
                    Degrade(Color.white, Color.white, 0.001f, 0.98f), AnimationCurve.Constant(0f, 1f, 1f), 200);
                var r = traits.GetComponent<ParticleSystemRenderer>();
                r.velocityScale = 0.012f;
                lueurs = Systeme(transform, "Lueurs", Particules(TextureLueur, true, 0.2f), false, 0f,
                    Degrade(Color.white, Color.white, 0.01f, 0.5f), baisse, 1500);
                fumees = Systeme(transform, "Fumées", Particules(TextureFumee, false, 0.4f), false, -0.03f,
                    Degrade(Color.white, new Color(0.7f, 0.7f, 0.72f), 0.08f, 0.5f), montee, 600);
                var frein = fumees.limitVelocityOverLifetime;
                frein.enabled = true;
                frein.drag = 1.5f;
                eclairs = Systeme(transform, "Éclairs", Particules(TextureEclair, true, 0f), false, 0f,
                    Degrade(Color.white, new Color(1f, 0.5f, 0.3f), 0.01f, 0.3f), baisse, 60);
            }

            void OnDestroy() { if (instance == this) instance = null; }

            public void Emettre(ParticleSystem ps, Vector3 position, Vector3 vitesse, float vie, float taille, Color couleur)
            {
                var e = new ParticleSystem.EmitParams
                {
                    position = position,
                    velocity = vitesse,
                    startLifetime = vie,
                    startSize = taille,
                    startColor = couleur,
                    rotation = Random.Range(0f, 360f),
                    applyShapeToPosition = false,
                };
                ps.Emit(e, 1);
            }

            public void Prevoir(float delai, Action action) => prevus.Add((Time.time + delai, action));

            void Update()
            {
                for (int i = prevus.Count - 1; i >= 0; i--)
                {
                    if (Time.time < prevus[i].quand) continue;
                    var a = prevus[i].action;
                    prevus.RemoveAt(i);
                    a();
                }
            }
        }
    }
}
