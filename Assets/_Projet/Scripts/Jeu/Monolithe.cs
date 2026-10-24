using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Monolithe dans la bulle lente : il ne répond que lorsque le temps est assez ralenti autour de lui.
    /// Maintenir E trois secondes révèle la vérité sur l'Éther (hors mission).
    /// </summary>
    public class Monolithe : MonoBehaviour
    {
        public float distanceInteraction = 6f;
        public float dureeContact = 3f;
        [Tooltip("Présence minimale dans la bulle lente pour que le monolithe réponde.")]
        public float presenceRequise = 0.6f;
        [TextArea(3, 6)]
        public string revelation = "Les glyphes ne sont pas gravés : ils s'écrivent, lentement, à rebours. « L'Éther n'est pas un minerai. C'est du temps qui s'est figé. Chaque cristal récolté est un instant que quelqu'un ne vivra pas. Les Arpenteurs d'avant vous avaient le même quota. »";

        public bool Actif { get; private set; }
        float progression;
        Material materiau;
        LineRenderer[] anneaux;

        void Start()
        {
            if (GetComponentInChildren<Renderer>() == null) Construire();
            else materiau = GetComponentInChildren<Renderer>().material;
        }

        void Construire()
        {
            materiau = Formes.Opaque(new Color(0.04f, 0.04f, 0.06f), 0.7f, 0.88f);
            var glyphes = Glyphes();
            materiau.SetTexture("_EmissiveColorMap", glyphes);
            materiau.EnableKeyword("_EMISSIVE_COLOR_MAP");
            Formes.DefinirEmission(materiau, Color.white * 0.05f);
            var dalle = Formes.Primitive(PrimitiveType.Cube, transform, materiau, "Dalle");
            dalle.transform.localScale = new Vector3(2.4f, 13f, 0.75f);
            dalle.transform.localPosition = Vector3.up * 6.2f;
            dalle.transform.localRotation = Quaternion.Euler(0f, 23f, 0f);
            dalle.AddComponent<BoxCollider>();

            anneaux = new LineRenderer[3];
            var lueur = Formes.Lueur(new Color(0.5f, 1.6f, 1.5f), 0f, 1f);
            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject("Anneau");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = Vector3.up * 6.5f;
                var l = go.AddComponent<LineRenderer>();
                l.useWorldSpace = false;
                l.loop = false;
                l.widthMultiplier = 0.12f;
                l.sharedMaterial = lueur;
                float rayon = 3.2f + i * 1.1f, arc = Mathf.PI * (0.7f + i * 0.25f);
                l.positionCount = 48;
                for (int p = 0; p < 48; p++)
                {
                    float a = arc * p / 47f;
                    l.SetPosition(p, new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * rayon);
                }
                go.SetActive(false);
                anneaux[i] = l;
            }
        }

        static Texture2D Glyphes()
        {
            var t = new Texture2D(128, 512, TextureFormat.RGBA32, true) { name = "Glyphes" };
            var pixels = new Color32[128 * 512];
            var alea = new System.Random(77);
            void Point(int x, int y) { if (x >= 0 && x < 128 && y >= 0 && y < 512) pixels[y * 128 + x] = new Color32(150, 255, 255, 255); }
            for (int col = 0; col < 3; col++)
                for (int row = 0; row < 26; row++)
                {
                    int cx = 22 + col * 38, cy = 22 + row * 18;
                    int x0 = cx, y0 = cy;
                    for (int s = 0; s < 3; s++)
                    {
                        int x1 = cx + alea.Next(-9, 10), y1 = cy + alea.Next(-6, 7);
                        int n = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0)) + 1;
                        for (int i = 0; i <= n; i++)
                        {
                            int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, i / (float)n)), y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, i / (float)n));
                            Point(x, y); Point(x + 1, y);
                        }
                        x0 = x1; y0 = y1;
                    }
                }
            t.SetPixels32(pixels);
            t.Apply(true);
            return t;
        }

        void Update()
        {
            var jeu = GestionnaireJeu.Instance;
            float presence = BulleTemporelle.PresenceLente(transform.position);
            if (anneaux != null && Actif)
                for (int i = 0; i < anneaux.Length; i++)
                    anneaux[i].transform.localRotation = Quaternion.Euler(Time.time * (12f + i * 7f), Time.time * (18f - i * 4f), i * 60f);
            if (materiau != null)
            {
                float lueur = Actif ? 3.5f + Mathf.Sin(Time.time * 1.3f) * 1.2f : 0.05f + presence * 0.6f + progression * 3f;
                Formes.DefinirEmission(materiau, Color.white * lueur);
            }
            if (Actif || jeu == null || jeu.Etat != EtatJeu.Jeu) return;

            float d = Vector3.Distance(jeu.joueur.transform.position, transform.position);
            if (d > distanceInteraction) { progression = 0f; return; }
            if (BulleTemporelle.PresenceLente(jeu.joueur.transform.position) < presenceRequise)
            {
                progression = 0f;
                Invite.Proposer("Le monolithe reste inerte. Le temps n'est pas assez lent ici.", -1f, 1f);
                return;
            }
            if (EntreesJeu.InteragirMaintenu()) progression += Time.deltaTime / dureeContact;
            else progression = Mathf.Max(0f, progression - Time.deltaTime);
            Invite.Proposer("Maintenir E · poser la main sur le monolithe", progression, 2f);
            if (progression >= 1f) Activer();
        }

        void Activer()
        {
            Actif = true;
            if (anneaux != null) foreach (var a in anneaux) a.gameObject.SetActive(true);
            if (Mission.Instance != null) Mission.Instance.MonolitheActive = true;
            BanqueSons.Jouer(BanqueSons.Instance?.monolithe);
            Transmissions.Echo("Le monolithe", revelation, 16f);
        }
    }
}
