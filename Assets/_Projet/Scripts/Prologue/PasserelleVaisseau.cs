using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Le passage du héros au vaisseau et retour : près de la porte, E (manette : X) pour monter à bord ;
    /// posé dans le hangar et presque arrêté, E pour descendre. Il bascule les caméras et les commandes.
    /// </summary>
    public class PasserelleVaisseau : MonoBehaviour
    {
        public ControleVaisseau vaisseau;
        public HerosPrologue heros;
        public CameraTroisiemePersonne vueAPied;
        public CameraVaisseau vueEnVol;
        public float distance = 2.6f;

        void Start() => Basculer(false);

        void Update()
        {
            if (vaisseau == null || heros == null) return;
            if (!vaisseau.pilote)
            {
                var porte = vaisseau.porte != null ? vaisseau.porte : vaisseau.transform;
                if (Vector3.Distance(heros.transform.position, porte.position) > distance) return;
                Invite.Proposer("Monter à bord de la navette");
                if (EntreesJeu.Interagir()) Basculer(true);
            }
            else if (vaisseau.Pose())
            {
                Invite.Proposer("Descendre de la navette");
                if (EntreesJeu.Interagir()) Basculer(false);
            }
        }

        void Basculer(bool aBord)
        {
            if (aBord) vaisseau.pilote = true;
            else
            {
                if (vaisseau.pilote && vaisseau.porte != null)
                {
                    var cc = heros.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;            // sinon il refuse d'être déplacé
                    heros.transform.position = vaisseau.porte.position;
                    if (cc != null) cc.enabled = true;
                }
                vaisseau.Couper();
            }
            heros.gameObject.SetActive(!aBord);
            if (vueAPied != null) vueAPied.gameObject.SetActive(!aBord);
            if (vueEnVol != null) vueEnVol.gameObject.SetActive(aBord);
            CameraTroisiemePersonne.Capturer(true);
        }
    }
}
