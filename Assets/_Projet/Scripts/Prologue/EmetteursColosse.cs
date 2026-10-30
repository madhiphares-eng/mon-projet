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

    /// <summary>
    /// Une fuite de vapeur (conduite percée, purge d'une machine) : un jet serré vers l'avant (+Z), qui ralentit,
    /// s'élargit et monte en se diluant.
    /// </summary>
    public class FuiteVapeur : MonoBehaviour
    {
        [Tooltip("Bouffées par seconde")] public float debit = 34f;
        [Tooltip("Vitesse de sortie (m/s)")] public float vitesse = 2.2f;
        [Tooltip("Taille des bouffées en fin de vie (m)")] public float taille = 0.95f;
        [Range(0f, 1f)] public float opacite = 0.16f;
        public Color couleur = new Color(0.78f, 0.8f, 0.82f);
        public bool actif = true;
        ParticleSystem ps;

        void Awake()
        {
            ps = EffetsColosse.Systeme(transform, "Vapeur", EffetsColosse.Particules(EffetsColosse.TextureFumee, false, 0.4f), false, -0.045f,
                EffetsColosse.Degrade(Color.white, new Color(0.8f, 0.8f, 0.8f), 0.06f, 0.65f), AnimationCurve.Linear(0f, 0.06f, 1f, 1f), 400);
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.4f, 2.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(vitesse * 0.85f, vitesse * 1.15f);
            main.startSize = taille;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(couleur.r, couleur.g, couleur.b, opacite);
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

    /// <summary>
    /// Le champ de force d'une cellule ou d'une porte : hexagones, balayage, cadre, ondes aux impacts.
    /// Il bloque le passage tant qu'il est allumé ; il s'éteint en clignotant (Couper) et se rallume (Allumer).
    /// Le plan va de -largeur/2 à +largeur/2 et de 0 à hauteur, dans le plan XY de l'objet.
    /// </summary>
    public class ChampDeForce : MonoBehaviour
    {
        public float largeur = 2f, hauteur = 2.6f;
        public Color couleur = new Color(0.35f, 0.85f, 1.2f);
        public float intensite = 1f;
        [Tooltip("Bloque le passage tant qu'il est allumé")] public bool bloquer = true;
        public bool Actif { get; private set; } = true;
        readonly Vector4[] impacts = new Vector4[8];
        int prochain;
        float temps, niveau = 1f;
        Material materiau;
        MeshRenderer rendu;
        BoxCollider mur;

        void Awake()
        {
            for (int i = 0; i < impacts.Length; i++) impacts[i] = new Vector4(0f, 0f, 0f, -1f);
            var go = new GameObject("Champ");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = EffetsColosse.Plan(largeur, hauteur);
            rendu = go.AddComponent<MeshRenderer>();
            rendu.shadowCastingMode = ShadowCastingMode.Off;
            materiau = EffetsColosse.Champ(couleur, largeur, hauteur);
            rendu.sharedMaterial = materiau;
            if (bloquer)
            {
                mur = gameObject.AddComponent<BoxCollider>();
                mur.center = new Vector3(0f, hauteur / 2f, 0f);
                mur.size = new Vector3(largeur, hauteur, 0.1f);
            }
        }

        /// <summary>Le champ est touché en ce point (un tir, un détenu qui le heurte) : une onde et des étincelles.</summary>
        public void Toucher(Vector3 pointMonde)
        {
            if (!Actif) return;
            var l = transform.InverseTransformPoint(pointMonde);
            impacts[prochain] = new Vector4(l.x, l.y, 0f, temps);
            prochain = (prochain + 1) % impacts.Length;
            EffetsColosse.Gerbe(pointMonde, -transform.forward, couleur, 8, 0.5f);
        }

        public void Couper() => Actif = false;
        public void Allumer() => Actif = true;

        void Update()
        {
            float dt = Time.deltaTime;
            temps += dt;
            niveau = Mathf.MoveTowards(niveau, Actif ? 1f : 0f, dt * (Actif ? 2.5f : 4f));
            // il s'éteint en clignotant, comme un générateur qui décroche
            float coupure = !Actif && niveau > 0.05f && Random.value < 0.3f ? 0f : 1f;
            materiau.SetFloat("_Temps", temps);
            materiau.SetVectorArray("_Impacts", impacts);
            materiau.SetFloat("_Intensite", intensite * niveau * coupure);
            rendu.enabled = niveau > 0.01f;
            if (mur != null) mur.enabled = Actif;
        }
    }

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

    /// <summary>
    /// Le gyrophare d'alarme : deux faisceaux opposés qui tournent dans la poussière, et la lumière qui balaie les murs.
    /// </summary>
    public class Gyrophare : MonoBehaviour
    {
        public Color couleur = new Color(1f, 0.16f, 0.1f);
        [Tooltip("Degrés par seconde")] public float vitesse = 180f;
        public float portee = 9f;
        [Tooltip("Lumière du projecteur (lumens)")] public float lumens = 30000f;
        public bool actif = true;
        Transform tete;
        Light projecteur;
        Material corps;

        void Start()
        {
            corps = Formes.Emissif(couleur * 0.4f, couleur * 8f);
            var boitier = Formes.Primitive(PrimitiveType.Cylinder, transform, corps, "Boîtier");
            boitier.transform.localScale = new Vector3(0.18f, 0.08f, 0.18f);
            tete = new GameObject("Tête").transform;
            tete.SetParent(transform, false);
            var faisceau = EffetsColosse.Volume(couleur, couleur * 1.3f, 1.5f, 1f, 0.02f, 0.25f, 0.6f);
            foreach (float s in new[] { 1f, -1f })
            {
                var go = new GameObject("Faisceau");
                go.transform.SetParent(tete, false);
                go.transform.localPosition = new Vector3(s * portee * 0.7f, 0f, 0f);
                go.transform.localRotation = Quaternion.Euler(0f, 0f, s * 90f);     // le sommet du cône au gyrophare
                go.AddComponent<MeshFilter>().sharedMesh = EffetsColosse.Tronc(portee * 0.22f, 0.01f, portee * 0.7f, 24);
                var r = go.AddComponent<MeshRenderer>();
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.sharedMaterial = faisceau;
            }
            var lp = new GameObject("Projecteur");
            lp.transform.SetParent(tete, false);
            lp.transform.localRotation = Quaternion.LookRotation(new Vector3(1f, -0.15f, 0f));
            lp.AddComponent<HDAdditionalLightData>();
            projecteur = lp.GetComponent<Light>();
            projecteur.type = LightType.Spot;
            projecteur.color = couleur;
            projecteur.range = portee;
            projecteur.spotAngle = 50f;
            projecteur.intensity = lumens;
            projecteur.shadows = LightShadows.None;
        }

        void Update()
        {
            tete.gameObject.SetActive(actif);
            projecteur.enabled = actif;
            if (actif) tete.Rotate(0f, vitesse * Time.deltaTime, 0f, Space.Self);
        }
    }
}
