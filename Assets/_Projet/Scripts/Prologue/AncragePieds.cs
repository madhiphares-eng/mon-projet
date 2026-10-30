using UnityEngine;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Les pieds sur le sol, sur n'importe quel corps Humanoid. C'est le bot des mouvements de la vitrine
    /// (vitrine/bot-mouvements.js), en jeu et sur un vrai sol :
    /// - chaque pied suit la hauteur du sol sous lui (marches, rampes, passerelles), à la hauteur que lui donne l'animation ;
    /// - un pied posé se bloque là où il est tant qu'il porte le corps : il ne glisse plus quand le personnage avance un peu
    ///   plus vite ou moins vite que son animation, ni quand il tourne sur place ;
    /// - les hanches descendent du côté du pied le plus bas, pour que la jambe l'atteigne ;
    /// - le pied posé épouse la pente.
    /// Le plan du sol de l'animation est celui de l'objet (ses pieds y sont posés au repos). S'exécute après l'animation et
    /// avant la prise des armes (LateUpdate, ordre 90).
    /// </summary>
    [DefaultExecutionOrder(90)]
    public class AncragePieds : MonoBehaviour
    {
        public Animator animateur;
        [Tooltip("Les couches du sol")] public LayerMask sol = ~0;
        [Tooltip("Un pied de l'animation est posé quand il est à moins de cette hauteur (m) de son plus bas")] public float seuilAppui = 0.04f;
        [Tooltip("Jusqu'où chercher le sol sous un pied, au-dessus et au-dessous du plan de l'objet (m)")] public float portee = 0.7f;
        [Range(0f, 1f)] public float poids = 1f;
        public bool bloquerLesPiedsPoses = true;
        public bool epouserLaPente = true;

        struct Jambe
        {
            public Transform cuisse, mollet, pied;
            public float semelle;          // la hauteur de la cheville au-dessus du plan quand le pied est à plat (apprise en jouant)
            public bool pose;
            public Vector3 verrou;          // où le pied posé est bloqué (dans le monde)
            public float poidsVerrou;
        }
        Jambe gauche, droite;
        Transform hanches;
        float descente;
        bool pret;
        readonly RaycastHit[] touches = new RaycastHit[12];

        void Start()
        {
            if (animateur == null) animateur = GetComponentInChildren<Animator>();
            if (animateur == null || !animateur.isHuman) { enabled = false; return; }
            hanches = animateur.GetBoneTransform(HumanBodyBones.Hips);
            gauche = Preparer(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot);
            droite = Preparer(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot);
            pret = hanches != null && gauche.pied != null && droite.pied != null;
        }

        Jambe Preparer(HumanBodyBones cuisse, HumanBodyBones mollet, HumanBodyBones pied) => new Jambe
        {
            cuisse = animateur.GetBoneTransform(cuisse), mollet = animateur.GetBoneTransform(mollet), pied = animateur.GetBoneTransform(pied),
            semelle = float.MaxValue,
        };

        // le sol sous un point : sa hauteur et sa normale (le plan de l'objet s'il n'y a rien)
        bool Sol(Vector3 point, out float hauteur, out Vector3 normale)
        {
            float plan = transform.position.y;
            var depart = new Vector3(point.x, plan + portee, point.z);
            int n = Physics.RaycastNonAlloc(depart, Vector3.down, touches, portee * 2f, sol, QueryTriggerInteraction.Ignore);
            float meilleur = float.MaxValue; hauteur = plan; normale = Vector3.up;
            for (int i = 0; i < n; i++)
            {
                var t = touches[i];
                if (t.transform != null && t.transform.IsChildOf(transform)) continue;   // pas son propre corps
                if (t.distance < meilleur) { meilleur = t.distance; hauteur = t.point.y; normale = t.normal; }
            }
            return meilleur < float.MaxValue;
        }

        void LateUpdate()
        {
            if (!pret || poids <= 0f) return;
            float dt = Time.deltaTime, plan = transform.position.y;
            // ce que veut l'animation, avant toute correction
            Vector3 piedG = gauche.pied.position, piedD = droite.pied.position;
            Quaternion rotG = gauche.pied.rotation, rotD = droite.pied.rotation;
            Sol(piedG, out float solG, out Vector3 normG);
            Sol(piedD, out float solD, out Vector3 normD);
            // les hanches descendent du côté du pied le plus bas (jamais plus haut que l'animation)
            float voulue = Mathf.Min(0f, Mathf.Min(solG, solD) - plan);
            descente = Mathf.Lerp(descente, voulue, 1f - Mathf.Exp(-12f * dt));
            hanches.position += Vector3.up * (descente * poids);
            Poser(ref gauche, piedG, rotG, solG, normG, plan, dt);
            Poser(ref droite, piedD, rotD, solD, normD, plan, dt);
        }

        void Poser(ref Jambe j, Vector3 anim, Quaternion rotAnim, float hSol, Vector3 normale, float plan, float dt)
        {
            // la hauteur du pied au-dessus du plan de l'animation ; son plus bas est appris en jouant (il remonte lentement)
            float h = anim.y - plan;
            j.semelle = Mathf.Min(j.semelle, h) + 0.02f * dt;
            bool pose = h < j.semelle + seuilAppui;
            // la cible : la même hauteur, au-dessus du vrai sol
            Vector3 cible = anim + Vector3.up * (hSol - plan);
            if (bloquerLesPiedsPoses)
            {
                if (pose && !j.pose) j.verrou = cible;
                if (pose && (j.verrou - cible).sqrMagnitude > 0.36f) j.verrou = cible;   // trop loin : on le repose (téléportation, demi-tour)
                j.poidsVerrou = Mathf.MoveTowards(j.poidsVerrou, pose ? 1f : 0f, dt / (pose ? 0.06f : 0.12f));
                if (j.poidsVerrou > 0f)
                {
                    Vector3 v = j.verrou; v.y = cible.y;
                    cible = Vector3.Lerp(cible, v, j.poidsVerrou);
                }
            }
            j.pose = pose;
            cible = Vector3.Lerp(anim, cible, poids);
            // la jambe se replie vers la cible, le genou dans le plan de l'animation
            Vector3 genou = j.mollet.position, milieu = (j.cuisse.position + anim) * 0.5f;
            Vector3 pole = genou + (genou - milieu).normalized * 0.5f;
            PriseArme.Ik(j.cuisse, j.mollet, j.pied, cible, pole);
            // le pied garde l'orientation de l'animation ; posé, il épouse la pente
            Quaternion rot = rotAnim;
            if (epouserLaPente && pose) rot = Quaternion.Slerp(rotAnim, Quaternion.FromToRotation(Vector3.up, normale) * rotAnim, poids);
            j.pied.rotation = rot;
        }
    }
}
