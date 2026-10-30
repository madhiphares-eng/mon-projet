using UnityEngine;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Le fusil tenu à deux mains, sur n'importe quel personnage Humanoid qui joue les animations Fusil_ du studio :
    /// il est collé au poignet droit, et son orientation se déduit des deux mains (la gauche est sous le garde-main,
    /// 30 cm plus loin). Pendant le rechargement, la main gauche quitte l'arme : l'orientation par rapport à la main
    /// droite, mesurée juste avant, est gardée. Sans modèle enfant, il se fabrique un fusil simple (à remplacer).
    /// Comme le banc des personnages et l'essai des vues du navigateur.
    /// </summary>
    public class FusilDeuxMains : MonoBehaviour
    {
        public Animator animateur;
        [Tooltip("Vrai pendant le rechargement (la main gauche lâche l'arme)")] public bool rechargement;
        [Tooltip("Taille du personnage par rapport au mannequin du studio (1,83 m)")] public float echelle = 1f;
        public Transform bouche;

        Transform mainD, mainG;
        Quaternion depuisLaMain = Quaternion.identity;
        bool mesure;

        void Start()
        {
            if (animateur == null) animateur = GetComponentInParent<Animator>();
            if (animateur != null && animateur.isHuman)
            {
                mainD = animateur.GetBoneTransform(HumanBodyBones.RightHand);
                mainG = animateur.GetBoneTransform(HumanBodyBones.LeftHand);
            }
            if (transform.childCount == 0) Fabriquer();
        }

        void Fabriquer()
        {
            var acier = Jeu.Formes.Opaque(new Color(0.17f, 0.19f, 0.21f), 0.7f, 0.55f);
            var noir = Jeu.Formes.Opaque(new Color(0.08f, 0.09f, 0.1f), 0.3f, 0.3f);
            // z le long du canon, y le haut de l'arme ; l'origine est au poignet droit
            Piece(new Vector3(0.035f, 0.11f, 0.045f), new Vector3(0f, -0.045f, 0.05f), -20f, noir, "Poignée");
            Piece(new Vector3(0.05f, 0.075f, 0.32f), new Vector3(0f, 0.045f, 0.10f), 0f, acier, "Carcasse");
            Piece(new Vector3(0.04f, 0.09f, 0.26f), new Vector3(0f, 0.02f, -0.18f), 0f, noir, "Crosse");
            Piece(new Vector3(0.05f, 0.055f, 0.24f), new Vector3(0f, 0.04f, 0.36f), 0f, noir, "Garde-main");
            Piece(new Vector3(0.03f, 0.15f, 0.06f), new Vector3(0f, -0.05f, 0.17f), 14f, acier, "Chargeur");
            Piece(new Vector3(0.022f, 0.022f, 0.22f), new Vector3(0f, 0.045f, 0.58f), 0f, acier, "Canon");
            if (bouche == null)
            {
                bouche = new GameObject("Bouche").transform;
                bouche.SetParent(transform, false);
                bouche.localPosition = new Vector3(0f, 0.045f, 0.7f);
            }
        }

        void Piece(Vector3 taille, Vector3 position, float inclinaison, Material m, string nom)
        {
            var go = Jeu.Formes.Primitive(PrimitiveType.Cube, transform, m, nom);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(inclinaison, 0f, 0f);
            go.transform.localScale = taille;
        }

        void LateUpdate()
        {
            if (mainD == null || mainG == null) return;
            transform.localScale = Vector3.one * echelle;
            if (!rechargement || !mesure)
            {
                // le canon passe au-dessus et à droite du poignet gauche (studio : garde = poignée + 0,30 avant - 0,06 haut
                // + 0,035 gauche). Unity est en main gauche : Cross(haut, avant) donne la droite de l'arme.
                Vector3 R = mainD.position, L = mainG.position;
                Vector3 u = Vector3.up, d = (L - R).normalized;
                for (int k = 0; k < 3; k++)
                {
                    Vector3 droite = Vector3.Cross(u, d).normalized;
                    u = Vector3.Cross(d, droite).normalized;
                    d = (L - R + u * 0.06f * echelle + droite * 0.035f * echelle).normalized;
                }
                var arme = Quaternion.LookRotation(d, u);
                depuisLaMain = Quaternion.Inverse(mainD.rotation) * arme;
                mesure = true;
            }
            transform.SetPositionAndRotation(mainD.position, mainD.rotation * depuisLaMain);
        }
    }
}
