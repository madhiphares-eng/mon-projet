using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchosEther.Prologue
{
    /// <summary>Un mouvement d'un objet du décor, tel que le bot des objets l'a trouvé (docs/prologue/objets-animes.json).</summary>
    [Serializable]
    public class MouvementObjet
    {
        [Tooltip("coulisser, tourner, animation ou vol")] public string type = "coulisser";
        [Tooltip("Les noms des pièces (« * » : tout l'objet)")] public string[] pieces = new string[0];
        [Tooltip("Coulisser : moins ou plus (le long de la largeur), haut")] public string sens = "plus";
        [Tooltip("Coulisser : la course, en fraction de la taille du battant")] public float course = 0.95f;
        [Tooltip("Tourner : degrés par seconde")] public float vitesse = 20f;
        [Tooltip("Animation : le nom de l'animation embarquée")] public string clip = "";
        [Tooltip("Animation : ouvrir (une porte) ou boucle")] public string mode = "boucle";
        [Tooltip("Animation d'une porte : l'instant le plus fermé et le plus ouvert (fractions de la durée)")] public float debut, fin = 1f;
    }

    /// <summary>
    /// Un objet du décor qui bouge : porte, sas, armoire, console, machine, relique, drone. Il suit la recette du bot des
    /// objets (vitrine/bot-objets.js) et l'énergie du Colosse (<see cref="EnergieColosse"/>) :
    /// - une porte s'ouvre quand quelqu'un approche (<see cref="Passants"/>) ; en alarme elle reste verrouillée ; en panne
    ///   elle se bloque à demi, par à-coups ;
    /// - une pièce qui tourne s'emballe en alarme, ralentit jusqu'à presque s'arrêter en panne ;
    /// - les lumières battent en rouge en alarme, grésillent en panne ;
    /// - un drone flotte et balaie la salle du regard ; en panne, il tombe.
    /// La recette ne nomme que des pièces et des directions géométriques : les axes sont retrouvés ici, d'après la forme
    /// des pièces, comme dans le navigateur. Menu Echos d'Ether → Prologue → Animer les objets du décor : le composant
    /// est posé et réglé sur tous les objets de la scène.
    /// </summary>
    public class ObjetAnime : MonoBehaviour
    {
        /// <summary>Ce qui fait ouvrir les portes : le héros et les PNJ s'inscrivent ici.</summary>
        public static readonly List<Transform> Passants = new List<Transform>();
        public static void Inscrire(Transform t) { if (t != null && !Passants.Contains(t)) Passants.Add(t); }
        public static void Desinscrire(Transform t) => Passants.Remove(t);

        public string cle;
        public List<MouvementObjet> mouvements = new List<MouvementObjet>();
        [Tooltip("Les matériaux lumineux (leur nom)")] public string[] lumieres = new string[0];
        [Tooltip("Une porte s'ouvre quand quelqu'un est à moins de cette distance (m)")] public float rayon = 2.6f;
        [Tooltip("Les .glb importés par glTFast ont l'axe X retourné par rapport au fichier : le sens d'un battant aussi")]
        public bool mirroirX = true;
        [Tooltip("Verrouillée quoi qu'il arrive (une porte du scénario)")] public bool verrouillee;
        public Color couleurAlarme = new Color(1f, 0.16f, 0.1f);

        class Mobile
        {
            public MouvementObjet mv;
            public Transform[] noeuds;
            public Vector3[] pos;          // la pose de repos de chaque pièce, dans le repère de l'objet
            public Quaternion[] rot;
            public Vector3 centre, axe, pas, depart;
            public float angle, vitesse, chute, cap, hauteur, sol;
            public AnimationClip clip;
            public GameObject racineClip;
        }

        class Lumiere { public Material m; public string propriete; public Color couleur; }

        readonly List<Mobile> mobiles = new List<Mobile>();
        readonly List<Lumiere> lampes = new List<Lumiere>();
        Bounds boite;
        float ouverture, temps, panneOuverture;

        /// <summary>De 0 (fermée) à 1 (ouverte).</summary>
        public float Ouverture => ouverture;

        void Start()
        {
            panneOuverture = UnityEngine.Random.Range(0.35f, 0.55f);
            boite = BoiteLocale(GetComponentsInChildren<Renderer>());
            // les axes de l'objet : l'épaisseur (l'axe horizontal le plus court), la largeur (l'autre)
            bool largeurX = boite.size.x > boite.size.z;
            foreach (var mv in mouvements)
            {
                var m = new Mobile { mv = mv };
                if (mv.type == "animation")
                {
                    var anim = GetComponentInChildren<Animation>();
                    m.clip = anim != null ? anim.GetClip(mv.clip) : null;
                    m.racineClip = anim != null ? anim.gameObject : gameObject;
                    if (anim != null) anim.playAutomatically = false;
                    if (m.clip == null) { Debug.LogWarning($"{name} : l'animation « {mv.clip} » manque (importer le modèle avec ses animations, en Legacy)."); continue; }
                    mobiles.Add(m);
                    continue;
                }
                var noeuds = new List<Transform>();
                foreach (string p in mv.pieces)
                    if (p == "*") noeuds.Add(transform);
                    else foreach (var t in GetComponentsInChildren<Transform>(true)) if (t.name == p) noeuds.Add(t);
                if (noeuds.Count == 0) { Debug.LogWarning($"{name} : pièces introuvables ({string.Join(", ", mv.pieces)})."); continue; }
                m.noeuds = noeuds.ToArray();
                if (mv.type == "vol")
                {
                    m.depart = transform.localPosition; m.cap = transform.localEulerAngles.y; m.hauteur = boite.size.y;
                    m.sol = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit h, 50f) ? transform.position.y - h.point.y : transform.position.y;
                    mobiles.Add(m);
                    continue;
                }
                m.pos = new Vector3[noeuds.Count]; m.rot = new Quaternion[noeuds.Count];
                var rends = new List<Renderer>();
                for (int i = 0; i < noeuds.Count; i++)
                {
                    m.pos[i] = transform.InverseTransformPoint(noeuds[i].position);
                    m.rot[i] = Quaternion.Inverse(transform.rotation) * noeuds[i].rotation;
                    rends.AddRange(noeuds[i].GetComponentsInChildren<Renderer>());
                }
                var b = BoiteLocale(rends.ToArray());
                m.centre = b.center;
                if (mv.type == "tourner")
                {
                    // l'axe court : celui de la plus petite dimension (un anneau tourne dans son plan)
                    var s = b.size;
                    m.axe = s.x <= s.y && s.x <= s.z ? Vector3.right : s.y <= s.z ? Vector3.up : Vector3.forward;
                }
                if (mv.type == "coulisser")
                {
                    if (mv.sens == "haut") m.pas = Vector3.up * b.size.y * mv.course;
                    else
                    {
                        float signe = mv.sens == "moins" ? -1f : 1f;
                        if (largeurX && mirroirX) signe = -signe;
                        m.pas = (largeurX ? Vector3.right * b.size.x : Vector3.forward * b.size.z) * signe * mv.course;
                    }
                }
                mobiles.Add(m);
            }
            // les lumières : chaque matériau nommé dans la recette, avec sa couleur de départ
            foreach (var r in GetComponentsInChildren<Renderer>())
                foreach (var mat in r.materials)
                    foreach (string nom in lumieres)
                    {
                        if (!mat.name.StartsWith(nom)) continue;
                        string prop = mat.HasProperty("_EmissiveColor") ? "_EmissiveColor" : "_EmissionColor";
                        var c = mat.HasProperty(prop) ? mat.GetColor(prop) : Color.black;
                        if (c.maxColorComponent < 0.01f)
                        {
                            // un écran sans lumière propre : sa couleur devient sa lumière
                            c = new Color(0.7f, 0.85f, 0.9f) * 0.6f;
                            if (mat.HasProperty("_BaseColorMap")) { mat.SetTexture("_EmissiveColorMap", mat.GetTexture("_BaseColorMap")); mat.EnableKeyword("_EMISSIVE_COLOR_MAP"); }
                            else if (mat.HasProperty("_MainTex")) { mat.SetTexture("_EmissionMap", mat.GetTexture("_MainTex")); mat.EnableKeyword("_EMISSION"); }
                        }
                        lampes.Add(new Lumiere { m = mat, propriete = prop, couleur = c });
                        break;
                    }
        }

        // la boîte de ces rendus, dans le repère de l'objet
        Bounds BoiteLocale(Renderer[] rends)
        {
            var b = new Bounds();
            bool premier = true;
            foreach (var r in rends)
            {
                Mesh mesh = r is SkinnedMeshRenderer sk ? sk.sharedMesh : r.GetComponent<MeshFilter>()?.sharedMesh;
                if (mesh == null) continue;
                var mb = mesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var coin = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    var p = transform.InverseTransformPoint(r.transform.TransformPoint(coin));
                    if (premier) { b = new Bounds(p, Vector3.zero); premier = false; } else b.Encapsulate(p);
                }
            }
            return b;
        }

        bool QuelquUnApproche()
        {
            var centre = transform.TransformPoint(boite.center);
            foreach (var p in Passants)
                if (p != null && (p.position - centre).sqrMagnitude < rayon * rayon) return true;
            return false;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            temps += dt;
            var etat = EnergieColosse.Etat;
            bool proche = QuelquUnApproche();
            // la porte : ouverte si quelqu'un approche ; verrouillée en alarme ; bloquée à demi, par à-coups, en panne
            float voulue = proche && etat != Energie.Alarme && !verrouillee ? 1f : 0f;
            if (etat == Energie.Panne && !verrouillee) voulue = proche ? panneOuverture + 0.04f * Mathf.Sign(Mathf.Sin(temps * 9f)) : panneOuverture;
            float vitesse = etat == Energie.Panne ? 0.25f : 0.9f;
            ouverture = Mathf.MoveTowards(ouverture, voulue, vitesse * dt);
            float o = Mathf.SmoothStep(0f, 1f, ouverture);

            foreach (var m in mobiles)
            {
                switch (m.mv.type)
                {
                    case "animation":
                        float u = m.mv.mode == "ouvrir" ? Mathf.Lerp(m.mv.debut, m.mv.fin, o)
                            : temps * (etat == Energie.Panne ? 0.3f : etat == Energie.Alarme ? 1.6f : 1f) / Mathf.Max(0.01f, m.clip.length) % 1f;
                        m.clip.SampleAnimation(m.racineClip, u * m.clip.length * 0.999f);
                        break;
                    case "coulisser":
                        Placer(m, Quaternion.identity, m.pas * o);
                        break;
                    case "tourner":
                        // la vitesse de croisière ; en panne, la machine ralentit jusqu'à presque s'arrêter, avec de l'inertie
                        float cible = m.mv.vitesse * (etat == Energie.Alarme ? 1.8f : etat == Energie.Panne ? 0.08f : 1f);
                        m.vitesse = Mathf.Lerp(m.vitesse, cible, 1f - Mathf.Exp(-dt / (etat == Energie.Panne ? 2.5f : 0.8f)));
                        m.angle += m.vitesse * dt;
                        Placer(m, Quaternion.AngleAxis(m.angle, m.axe), Vector3.zero);
                        break;
                    case "vol":
                        // il flotte et balaie la salle ; en alarme il balaie vite ; en panne il tombe et s'incline
                        m.chute = etat == Energie.Panne ? Mathf.Min(1f, m.chute + dt * 0.8f) : Mathf.Max(0f, m.chute - dt);
                        var p = m.depart;
                        p.y += (1f - m.chute) * 0.05f * m.hauteur * Mathf.Sin(temps * 2.1f) - m.chute * Mathf.Max(0f, m.sol - 0.05f);
                        transform.localPosition = p;
                        float lacet = etat == Energie.Alarme ? 52f * Mathf.Sin(temps * 2.4f) : 29f * Mathf.Sin(temps * 0.5f);
                        transform.localRotation = Quaternion.Euler(0f, m.cap + lacet, m.chute * 29f + (1f - m.chute) * 2.3f * Mathf.Sin(temps * 1.3f));
                        break;
                }
            }

            // les lumières : stables (un léger souffle) ; en alarme, rouges et battantes ; en panne, elles grésillent
            foreach (var l in lampes)
            {
                float f = 1f + 0.03f * Mathf.Sin(temps * 13.1f);
                var c = l.couleur;
                if (etat == Energie.Alarme)
                {
                    float b = 0.5f + 0.5f * Mathf.Sin(temps * Mathf.PI * 2f * 0.9f);
                    c = Color.Lerp(c, couleurAlarme * Mathf.Max(1f, c.maxColorComponent), 0.85f);
                    f = 0.25f + 1.6f * b;
                }
                else if (etat == Energie.Panne)
                {
                    float hasard = Mathf.Abs(Mathf.Sin(Mathf.Floor(temps * 12f) * 12.9898f) * 43758.5453f) % 1f;
                    f = hasard < 0.55f ? 0.05f : hasard < 0.8f ? 0.5f : 1.1f;
                }
                l.m.SetColor(l.propriete, c * f);
            }
        }

        // place les pièces : tournées autour du centre de leur groupe, puis glissées, dans le repère de l'objet
        void Placer(Mobile m, Quaternion r, Vector3 decal)
        {
            for (int i = 0; i < m.noeuds.Length; i++)
            {
                var local = m.centre + r * (m.pos[i] - m.centre) + decal;
                m.noeuds[i].position = transform.TransformPoint(local);
                m.noeuds[i].rotation = transform.rotation * (r * m.rot[i]);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.53f, 0.94f, 0.9f, 0.5f);
            Gizmos.DrawWireSphere(transform.TransformPoint(boite.center), rayon);
        }
    }
}
