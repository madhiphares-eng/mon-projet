using UnityEngine;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Une arme et ses points de prise, pour <see cref="PriseArme"/>. Repère de l'arme : +Z vers la bouche, +Y vers le haut.
    /// Chaque prise est un repère enfant : sa position est le centre de la paume, son avant (Z) la direction des doigts
    /// (du poignet aux jointures), son haut (Y) la normale de la paume. La main gauche glisse sur le rail (Z local entre
    /// railMin et railMax) si le bras ne l'atteint pas. Sans enfants, clic droit → « Créer le fusil de l'Empire » ou
    /// « Créer le pistolet de l'Empire » : la forme simple et les prises, les mêmes que dans le navigateur (prise-armes.js).
    /// </summary>
    public class ArmeTenue : MonoBehaviour
    {
        public Transform priseDroite, priseGauche, crosse, bouche;
        public float railMin = 0.21f, railMax = 0.45f;
        [Tooltip("La main gauche ne vient que si l'animation l'approche à moins de cette distance (pistolet à une main), 0 = toujours")]
        public float gaucheFacultative;

        void Awake()
        {
            if (priseDroite == null) Fusil();
        }

        // les nombres de prise-armes.js ; là-bas x est la gauche de l'arme, ici x est sa droite
        static Vector3 V(float gauche, float haut, float avant) => new Vector3(-gauche, haut, avant);

        Transform Repere(string nom, Vector3 position, Vector3 doigts, Vector3 paume)
        {
            var t = transform.Find(nom);
            if (t == null) { t = new GameObject(nom).transform; t.SetParent(transform, false); }
            t.localPosition = position;
            t.localRotation = doigts == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(doigts, paume);
            return t;
        }

        void Piece(Vector3 taille, Vector3 position, float inclinaison, Material m, string nom)
        {
            var go = Jeu.Formes.Primitive(PrimitiveType.Cube, transform, m, nom);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(inclinaison * Mathf.Rad2Deg, 0f, 0f);     // une rotation autour de x ne change pas par le miroir
            go.transform.localScale = taille;
        }

        [ContextMenu("Créer le fusil de l'Empire")]
        public void Fusil()
        {
            var acier = Jeu.Formes.Opaque(new Color(0.17f, 0.19f, 0.21f), 0.75f, 0.58f);
            var noir = Jeu.Formes.Opaque(new Color(0.08f, 0.09f, 0.1f), 0.3f, 0.35f);
            var rouge = Jeu.Formes.Emissif(new Color(0.54f, 0.11f, 0.11f), new Color(2f, 0.3f, 0.3f));
            Piece(new Vector3(0.034f, 0.105f, 0.042f), V(0f, -0.058f, -0.005f), -0.32f, noir, "Poignée");
            Piece(new Vector3(0.05f, 0.075f, 0.34f), V(0f, 0.03f, 0.06f), 0f, acier, "Carcasse");
            Piece(new Vector3(0.04f, 0.085f, 0.27f), V(0f, 0.012f, -0.245f), 0f, noir, "Crosse");
            Piece(new Vector3(0.052f, 0.056f, 0.26f), V(0f, 0.025f, 0.36f), 0f, noir, "Garde-main");
            Piece(new Vector3(0.03f, 0.15f, 0.06f), V(0f, -0.07f, 0.12f), 0.25f, acier, "Chargeur");
            Piece(new Vector3(0.02f, 0.026f, 0.06f), V(0f, 0.081f, 0.03f), 0f, noir, "Viseur");
            Piece(new Vector3(0.012f, 0.012f, 0.012f), V(0f, 0.098f, 0.05f), 0f, rouge, "Diode");
            Piece(new Vector3(0.022f, 0.022f, 0.24f), V(0f, 0.03f, 0.61f), 0f, acier, "Canon");
            priseDroite = Repere("Prise droite", V(-0.03f, -0.045f, -0.012f), V(0f, -0.55f, 0.83f), V(1f, 0f, 0f));
            priseGauche = Repere("Prise gauche", V(0.014f, -0.024f, 0.31f), V(-0.72f, 0.12f, 0.68f), V(-0.3f, 0.95f, 0f));
            crosse = Repere("Crosse", V(0f, 0.02f, -0.38f), Vector3.zero, Vector3.zero);
            bouche = Repere("Bouche", V(0f, 0.03f, 0.73f), Vector3.zero, Vector3.zero);
            railMin = 0.21f; railMax = 0.45f; gaucheFacultative = 0f;
        }

        [ContextMenu("Créer le pistolet de l'Empire")]
        public void Pistolet()
        {
            var acier = Jeu.Formes.Opaque(new Color(0.17f, 0.19f, 0.21f), 0.75f, 0.58f);
            var noir = Jeu.Formes.Opaque(new Color(0.08f, 0.09f, 0.1f), 0.3f, 0.35f);
            Piece(new Vector3(0.03f, 0.1f, 0.04f), V(0f, -0.05f, 0f), -0.28f, noir, "Poignée");
            Piece(new Vector3(0.032f, 0.04f, 0.19f), V(0f, 0.018f, 0.05f), 0f, acier, "Culasse");
            priseDroite = Repere("Prise droite", V(-0.028f, -0.04f, -0.012f), V(0f, -0.55f, 0.83f), V(1f, 0f, 0f));
            priseGauche = Repere("Prise gauche", V(0.03f, -0.06f, -0.005f), V(-0.4f, -0.55f, 0.73f), V(-0.95f, 0.15f, 0f));
            crosse = Repere("Crosse", V(0f, 0f, -0.03f), Vector3.zero, Vector3.zero);
            bouche = Repere("Bouche", V(0f, 0.018f, 0.18f), Vector3.zero, Vector3.zero);
            railMin = -0.07f; railMax = 0.01f; gaucheFacultative = 0.14f;
        }
    }
}
