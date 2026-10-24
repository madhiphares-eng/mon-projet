using System.Collections.Generic;
using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Combat
{
    /// <summary>
    /// Effets visuels du combat, générés par le code et recyclés : éclairs de bouche, traçantes, rayons,
    /// étincelles d'impact, explosions, ondes de choc et éclats de cristal. Lueurs additives HDRP (shader Lueur).
    /// </summary>
    public class EffetsCombat : MonoBehaviour
    {
        static EffetsCombat instance;

        static EffetsCombat I
        {
            get
            {
                if (instance == null) instance = new GameObject("Effets de combat").AddComponent<EffetsCombat>();
                return instance;
            }
        }

        static readonly int IdCouleur = Shader.PropertyToID("_Couleur");
        static readonly int IdIntensite = Shader.PropertyToID("_Intensite");

        enum Forme { Sphere, Eclat }

        sealed class Particule
        {
            public Transform t;
            public MeshRenderer rendu;
            public Forme forme;
            public Vector3 vitesse, echelle0, echelle1, axe;
            public float age, vie, intensite, rotation;
            public Color couleur;
            public bool gravite, etirer;
        }

        sealed class Trait
        {
            public LineRenderer ligne;
            public float age, vie, largeur;
            public Color couleur;
        }

        sealed class Eclairage
        {
            public Light lumiere;
            public float age, vie, lumens;
        }

        readonly List<Particule> actives = new List<Particule>();
        readonly Stack<Particule> libresSphere = new Stack<Particule>();
        readonly Stack<Particule> libresEclat = new Stack<Particule>();
        readonly List<Trait> traits = new List<Trait>();
        readonly Stack<Trait> traitsLibres = new Stack<Trait>();
        readonly List<Eclairage> flashs = new List<Eclairage>();
        readonly Stack<Eclairage> flashsLibres = new Stack<Eclairage>();
        MaterialPropertyBlock bloc;
        Material plein, bulle;
        Mesh sphere;

        void Awake()
        {
            instance = this;
            bloc = new MaterialPropertyBlock();
            plein = Formes.Lueur(Color.white, 0f, 1f);
            bulle = Formes.Lueur(Color.white, 2.2f, 0.12f);
            var temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere = temp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(temp);
        }

        void OnDestroy() { if (instance == this) instance = null; }

        // ------------------------------------------------------------------ API

        /// <summary>Éclair à la bouche du canon.</summary>
        public static void Eclair(Vector3 position, Vector3 direction, Color couleur, float taille = 1f)
        {
            var p = I.Prendre(Forme.Sphere, I.plein, position, couleur, 0.07f, 2.2f);
            p.t.rotation = Quaternion.LookRotation(direction);
            p.echelle0 = new Vector3(0.16f, 0.16f, 0.42f) * taille;
            p.echelle1 = p.echelle0 * 0.3f;
            I.Lumiere(position, couleur, 9000f * taille, 0.06f, 10f);
        }

        /// <summary>Traçante entre deux points (balle instantanée).</summary>
        public static void Tracante(Vector3 a, Vector3 b, Color couleur, float largeur = 0.035f, float duree = 0.08f) =>
            I.Ligne(a, b, couleur, largeur, duree);

        /// <summary>Rayon épais du fusil Sablier.</summary>
        public static void Rayon(Vector3 a, Vector3 b, Color couleur)
        {
            I.Ligne(a, b, couleur, 0.16f, 0.45f);
            I.Ligne(a, b, Color.white * 1.5f, 0.05f, 0.25f);
            Vector3 dir = (b - a);
            float l = dir.magnitude;
            for (int i = 0; i < Mathf.Min(14, (int)(l / 3f)); i++)
            {
                Vector3 pos = a + dir * Random.value;
                var p = I.Prendre(Forme.Sphere, I.plein, pos, couleur, Random.Range(0.4f, 0.8f), 1.2f);
                p.vitesse = Random.insideUnitSphere * 0.8f;
                p.echelle0 = Vector3.one * 0.12f;
                p.echelle1 = Vector3.zero;
            }
        }

        /// <summary>Étincelles d'impact sur une surface ou une créature.</summary>
        public static void Impact(Vector3 point, Vector3 normale, Color couleur, int nombre = 8, float force = 1f)
        {
            if (normale.sqrMagnitude < 0.01f) normale = Planete.Haut(point);
            for (int i = 0; i < nombre; i++)
            {
                var p = I.Prendre(Forme.Sphere, I.plein, point, couleur, Random.Range(0.18f, 0.4f), 1.8f);
                p.vitesse = (normale + Random.insideUnitSphere * 0.9f).normalized * Random.Range(3f, 9f) * force;
                p.echelle0 = new Vector3(0.035f, 0.035f, 0.2f);
                p.echelle1 = new Vector3(0.01f, 0.01f, 0.05f);
                p.gravite = true;
                p.etirer = true;
            }
            var eclair = I.Prendre(Forme.Sphere, I.plein, point, couleur, 0.08f, 1.6f);
            eclair.echelle0 = Vector3.one * 0.35f * force;
            eclair.echelle1 = Vector3.one * 0.05f;
        }

        /// <summary>Explosion : boule qui gonfle, éclair, étincelles et secousse selon la distance.</summary>
        public static void Explosion(Vector3 centre, float rayon, Color couleur)
        {
            var boule = I.Prendre(Forme.Sphere, I.bulle, centre, couleur, 0.45f, 3f);
            boule.echelle0 = Vector3.one * 0.5f;
            boule.echelle1 = Vector3.one * rayon * 2f;
            var coeur = I.Prendre(Forme.Sphere, I.plein, centre, Color.Lerp(couleur, Color.white, 0.5f), 0.18f, 3f);
            coeur.echelle0 = Vector3.one * rayon * 0.6f;
            coeur.echelle1 = Vector3.one * rayon * 0.1f;
            Impact(centre, Planete.Haut(centre), couleur, 22, 1.6f);
            Anneau(centre, Planete.Haut(centre), rayon * 1.3f, couleur, 0.5f);
            I.Lumiere(centre, couleur, 60000f, 0.35f, rayon * 5f);
            var jeu = GestionnaireJeu.Instance;
            if (jeu != null)
            {
                float d = Vector3.Distance(jeu.PositionObservateur, centre);
                CameraSecousse.Choc(Mathf.Clamp01(1.2f - d / (rayon * 5f)));
            }
        }

        /// <summary>Onde plate qui s'étend au sol (onde de choc, atterrissage d'un colosse).</summary>
        public static void Anneau(Vector3 centre, Vector3 haut, float rayon, Color couleur, float duree = 0.6f)
        {
            var a = I.Prendre(Forme.Sphere, I.bulle, centre, couleur, duree, 2.4f);
            a.t.rotation = Quaternion.FromToRotation(Vector3.up, haut);
            a.echelle0 = new Vector3(1f, 0.15f, 1f);
            a.echelle1 = new Vector3(rayon * 2f, 0.6f, rayon * 2f);
        }

        /// <summary>Éclats de cristal projetés (mort d'une créature, cristal brisé).</summary>
        public static void Eclats(Vector3 centre, Color couleur, int nombre, float vitesse, float taille = 0.25f)
        {
            for (int i = 0; i < nombre; i++)
            {
                var p = I.Prendre(Forme.Eclat, I.plein, centre + Random.insideUnitSphere * 0.4f, couleur, Random.Range(0.9f, 1.6f), 1.4f);
                p.vitesse = (Planete.Haut(centre) * 0.8f + Random.insideUnitSphere).normalized * vitesse * Random.Range(0.5f, 1.2f);
                p.echelle0 = new Vector3(0.45f, 1.4f, 0.45f) * taille * Random.Range(0.6f, 1.4f);
                p.echelle1 = Vector3.zero;
                p.gravite = true;
                p.axe = Random.onUnitSphere;
                p.rotation = Random.Range(180f, 720f);
                p.t.rotation = Random.rotation;
            }
        }

        /// <summary>Petite lueur qui monte (ramassage, soin, niveau).</summary>
        public static void Etincelle(Vector3 position, Color couleur, float taille = 0.3f)
        {
            var p = I.Prendre(Forme.Sphere, I.plein, position, couleur, 0.6f, 1.5f);
            p.vitesse = Planete.Haut(position) * 2.5f;
            p.echelle0 = Vector3.one * taille;
            p.echelle1 = Vector3.zero;
        }

        /// <summary>Éclair de lumière seul.</summary>
        public static void Flash(Vector3 position, Color couleur, float lumens, float duree, float portee) =>
            I.Lumiere(position, couleur, lumens, duree, portee);

        // ------------------------------------------------------------------ réserve

        Particule Prendre(Forme forme, Material materiau, Vector3 position, Color couleur, float vie, float intensite)
        {
            var pile = forme == Forme.Sphere ? libresSphere : libresEclat;
            Particule p;
            if (pile.Count > 0) p = pile.Pop();
            else
            {
                var go = new GameObject(forme == Forme.Sphere ? "Lueur" : "Eclat");
                go.transform.SetParent(transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = forme == Forme.Sphere ? sphere : Formes.Eclat;
                var r = go.AddComponent<MeshRenderer>();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                p = new Particule { t = go.transform, rendu = r, forme = forme };
            }
            p.rendu.sharedMaterial = materiau;
            p.t.gameObject.SetActive(true);
            p.t.position = position;
            p.t.rotation = Quaternion.identity;
            p.vitesse = Vector3.zero;
            p.age = 0f;
            p.vie = Mathf.Max(0.01f, vie);
            p.couleur = couleur;
            p.intensite = intensite;
            p.gravite = p.etirer = false;
            p.rotation = 0f;
            p.echelle0 = p.echelle1 = Vector3.one * 0.2f;
            p.t.localScale = p.echelle0;
            actives.Add(p);
            return p;
        }

        void Ligne(Vector3 a, Vector3 b, Color couleur, float largeur, float duree)
        {
            Trait tr;
            if (traitsLibres.Count > 0) tr = traitsLibres.Pop();
            else
            {
                var go = new GameObject("Trait");
                go.transform.SetParent(transform, false);
                var l = go.AddComponent<LineRenderer>();
                l.sharedMaterial = plein;
                l.positionCount = 2;
                l.useWorldSpace = true;
                l.numCapVertices = 2;
                l.generateLightingData = true;   // normales pour le shader Lueur
                l.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                l.receiveShadows = false;
                tr = new Trait { ligne = l };
            }
            tr.ligne.gameObject.SetActive(true);
            tr.ligne.SetPosition(0, a);
            tr.ligne.SetPosition(1, b);
            tr.age = 0f;
            tr.vie = duree;
            tr.largeur = largeur;
            tr.couleur = couleur;
            traits.Add(tr);
        }

        void Lumiere(Vector3 position, Color couleur, float lumens, float duree, float portee)
        {
            if (flashs.Count >= 6) return;   // au-delà, l'éclair ne se voit plus et coûte cher
            Eclairage f;
            if (flashsLibres.Count > 0) f = flashsLibres.Pop();
            else f = new Eclairage { lumiere = Formes.Lumiere(transform, "Flash", Color.white, 0f, 10f) };
            f.lumiere.transform.position = position;
            f.lumiere.color = new Color(Mathf.Clamp01(couleur.r / Mathf.Max(couleur.maxColorComponent, 0.01f)),
                Mathf.Clamp01(couleur.g / Mathf.Max(couleur.maxColorComponent, 0.01f)), Mathf.Clamp01(couleur.b / Mathf.Max(couleur.maxColorComponent, 0.01f)));
            f.lumiere.range = portee;
            f.lumiere.enabled = true;
            f.age = 0f;
            f.vie = duree;
            f.lumens = lumens;
            flashs.Add(f);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            for (int i = actives.Count - 1; i >= 0; i--)
            {
                var p = actives[i];
                p.age += dt;
                float k = p.age / p.vie;
                if (k >= 1f)
                {
                    p.t.gameObject.SetActive(false);
                    (p.forme == Forme.Sphere ? libresSphere : libresEclat).Push(p);
                    actives.RemoveAt(i);
                    continue;
                }
                if (p.gravite) p.vitesse -= Planete.Haut(p.t.position) * (9f * dt);
                p.t.position += p.vitesse * dt;
                if (p.etirer && p.vitesse.sqrMagnitude > 0.01f) p.t.rotation = Quaternion.LookRotation(p.vitesse);
                if (p.rotation != 0f) p.t.rotation = Quaternion.AngleAxis(p.rotation * dt, p.axe) * p.t.rotation;
                p.t.localScale = Vector3.Lerp(p.echelle0, p.echelle1, 1f - (1f - k) * (1f - k));
                bloc.SetColor(IdCouleur, p.couleur);
                bloc.SetFloat(IdIntensite, p.intensite * (1f - k));
                p.rendu.SetPropertyBlock(bloc);
            }
            for (int i = traits.Count - 1; i >= 0; i--)
            {
                var tr = traits[i];
                tr.age += dt;
                float k = tr.age / tr.vie;
                if (k >= 1f)
                {
                    tr.ligne.gameObject.SetActive(false);
                    traitsLibres.Push(tr);
                    traits.RemoveAt(i);
                    continue;
                }
                tr.ligne.startWidth = tr.largeur * (1f - k * 0.5f);
                tr.ligne.endWidth = tr.largeur * (1f - k);
                bloc.SetColor(IdCouleur, tr.couleur);
                bloc.SetFloat(IdIntensite, 2f * (1f - k));
                tr.ligne.SetPropertyBlock(bloc);
            }
            for (int i = flashs.Count - 1; i >= 0; i--)
            {
                var f = flashs[i];
                f.age += dt;
                float k = f.age / f.vie;
                if (k >= 1f)
                {
                    f.lumiere.enabled = false;
                    flashsLibres.Push(f);
                    flashs.RemoveAt(i);
                    continue;
                }
                f.lumiere.intensity = f.lumens * (1f - k);
            }
        }
    }
}
