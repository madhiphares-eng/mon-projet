using EchosEther.Jeu;
using UnityEngine;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Le banc des effets, dans Unity : à poser sur un objet vide d'une scène vide, puis Play.
    /// Il construit un bout de couloir du Colosse avec chaque effet dans son alcôve (comme vitrine/banc-effets.html) :
    /// court-circuit et gyrophare, fuite de vapeur, cellule à champ de force sous le feu d'une tourelle,
    /// propulseur à l'essai, relique et son rayon d'éther, feu, et une explosion toutes les quatre secondes environ.
    /// Touches (nouveau Input System) : T tire une rafale, C coupe ou rallume le champ, G arrête ou relance l'alarme,
    /// + et - règlent le propulseur. Sans clavier, la tourelle tire seule de temps en temps.
    /// </summary>
    public class BancEffetsColosse : MonoBehaviour
    {
        public Color couleurTir = new Color(1f, 0.23f, 0.13f);
        ChampDeForce champ;
        Propulseur propulseur;
        Gyrophare gyrophare;
        Vector3 bouche;
        float rafale, explosion = 1.2f;

        void Start()
        {
            var acier = Formes.Opaque(new Color(0.16f, 0.17f, 0.2f), 0.75f, 0.52f);
            var acierClair = Formes.Opaque(new Color(0.23f, 0.25f, 0.28f), 0.7f, 0.6f);
            Bloc(PrimitiveType.Cube, acier, new Vector3(0f, -0.05f, 0f), new Vector3(26f, 0.1f, 10f), "Sol");
            Bloc(PrimitiveType.Cube, acier, new Vector3(0f, 2.5f, -2.15f), new Vector3(26f, 5f, 0.3f), "Fond");
            for (int x = -12; x <= 12; x += 2) Bloc(PrimitiveType.Cube, acierClair, new Vector3(x, 2.3f, -1.9f), new Vector3(0.3f, 4.6f, 0.35f), "Poutre");
            for (int x = -11; x <= 11; x += 4)
            {
                var l = Formes.Lumiere(transform, "Lampe au sodium", new Color(1f, 0.66f, 0.38f), 6000f, 8f);
                l.transform.localPosition = new Vector3(x, 3.6f, -1.2f);
            }

            // 1. le court-circuit, sous le gyrophare
            Bloc(PrimitiveType.Cube, acierClair, new Vector3(-8f, 1.9f, -1.9f), new Vector3(1.1f, 1.4f, 0.18f), "Panneau");
            var panne = new GameObject("Court-circuit").AddComponent<EtincellesPanne>();
            panne.transform.SetParent(transform, false);
            panne.transform.localPosition = new Vector3(-8.1f, 2.1f, -1.72f);
            panne.transform.localRotation = Quaternion.LookRotation(new Vector3(0.2f, 0.3f, 1f));
            gyrophare = new GameObject("Gyrophare").AddComponent<Gyrophare>();
            gyrophare.transform.SetParent(transform, false);
            gyrophare.transform.localPosition = new Vector3(-8f, 4.38f, -1.2f);

            // 2. la conduite qui fuit
            var conduite = Bloc(PrimitiveType.Cylinder, acierClair, new Vector3(-4f, 3.4f, -1.6f), new Vector3(0.32f, 3f, 0.32f), "Conduite");
            conduite.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            var fuite = new GameObject("Fuite de vapeur").AddComponent<FuiteVapeur>();
            fuite.transform.SetParent(transform, false);
            fuite.transform.localPosition = new Vector3(-4f, 3.3f, -1.42f);
            fuite.transform.localRotation = Quaternion.LookRotation(new Vector3(1f, -0.25f, 0.35f));

            // 3. la cellule et son champ de force
            Bloc(PrimitiveType.Cube, acierClair, new Vector3(-1.15f, 1.5f, -0.9f), new Vector3(0.3f, 3f, 0.4f), "Montant");
            Bloc(PrimitiveType.Cube, acierClair, new Vector3(1.15f, 1.5f, -0.9f), new Vector3(0.3f, 3f, 0.4f), "Montant");
            Bloc(PrimitiveType.Cube, acierClair, new Vector3(0f, 3.05f, -0.9f), new Vector3(2.6f, 0.3f, 0.4f), "Linteau");
            champ = new GameObject("Champ de force").AddComponent<ChampDeForce>();
            champ.transform.SetParent(transform, false);
            champ.transform.localPosition = new Vector3(0f, 0f, -0.9f);
            champ.largeur = 2f;
            champ.hauteur = 2.9f;
            bouche = transform.TransformPoint(new Vector3(1.8f, 1.5f, 4.5f));

            // 4. le propulseur à l'essai, le jet vers +X
            Bloc(PrimitiveType.Cube, acierClair, new Vector3(2.5f, 1.6f, -0.6f), new Vector3(1.2f, 1f, 1f), "Carter");
            propulseur = new GameObject("Propulseur").AddComponent<Propulseur>();
            propulseur.transform.SetParent(transform, false);
            propulseur.transform.localPosition = new Vector3(3.2f, 1.6f, -0.6f);
            propulseur.transform.localRotation = Quaternion.LookRotation(Vector3.left);
            propulseur.longueur = 2.2f;
            propulseur.rayon = 0.32f;
            propulseur.puissance = 0.8f;

            // 6. des débris qui brûlent, et l'aire des explosions
            var feu = new GameObject("Feu").AddComponent<Feu>();
            feu.transform.SetParent(transform, false);
            feu.transform.localPosition = new Vector3(-11.5f, 0.1f, -0.8f);
            Bloc(PrimitiveType.Cube, acierClair, new Vector3(-11.6f, 0.17f, -1.2f), new Vector3(0.9f, 0.35f, 0.6f), "Débris");

            // 5. la relique, sur son socle de pierre noire
            Bloc(PrimitiveType.Cylinder, Formes.Opaque(new Color(0.06f, 0.065f, 0.08f), 0.2f, 0.75f), new Vector3(8f, 0.2f, -0.6f), new Vector3(1.6f, 0.2f, 1.6f), "Socle");
            var rayon = new GameObject("Rayon d'éther").AddComponent<RayonEther>();
            rayon.transform.SetParent(transform, false);
            rayon.transform.localPosition = new Vector3(8f, 0.4f, -0.6f);
            rayon.hauteur = 4.4f;
        }

        GameObject Bloc(PrimitiveType type, Material m, Vector3 position, Vector3 echelle, string nom)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = nom;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = position;
            go.transform.localScale = echelle;
            go.GetComponent<MeshRenderer>().sharedMaterial = m;
            return go;
        }

        void Tirer()
        {
            var cible = champ.transform.TransformPoint(new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(0.6f, 2.4f), 0f));
            EffetsColosse.Tir(bouche, cible, couleurTir, 45f, champ.transform.forward * -1f, p => champ.Toucher(p));
        }

        void Update()
        {
            explosion -= Time.deltaTime;
            if (explosion < 0f)
            {
                EffetsColosse.Explosion(transform.TransformPoint(new Vector3(13f + Random.Range(-0.4f, 0.4f), 0.6f, -0.2f)));
                explosion = Random.Range(3.5f, 5f);
            }
            rafale -= Time.deltaTime;
            if (rafale < 0f)
            {
                Tirer();
                rafale = Random.value < 0.3f ? 0.15f : Random.Range(0.8f, 2.2f);
            }
#if ENABLE_INPUT_SYSTEM
            var clavier = UnityEngine.InputSystem.Keyboard.current;
            if (clavier == null) return;
            if (clavier.tKey.wasPressedThisFrame) for (int i = 0; i < 4; i++) Invoke(nameof(Tirer), i * 0.14f);
            if (clavier.cKey.wasPressedThisFrame) { if (champ.Actif) champ.Couper(); else champ.Allumer(); }
            if (clavier.gKey.wasPressedThisFrame) gyrophare.actif = !gyrophare.actif;
            if (clavier.equalsKey.isPressed || clavier.numpadPlusKey.isPressed) propulseur.puissance = Mathf.Min(1.5f, propulseur.puissance + Time.deltaTime * 0.5f);
            if (clavier.minusKey.isPressed || clavier.numpadMinusKey.isPressed) propulseur.puissance = Mathf.Max(0f, propulseur.puissance - Time.deltaTime * 0.5f);
#endif
        }
    }
}
