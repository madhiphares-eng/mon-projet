using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Objectifs de la mission : quota de cristaux d'Éther, balise d'extraction, monolithe (hors mission).
    /// </summary>
    public class Mission : MonoBehaviour
    {
        public static Mission Instance { get; private set; }

        public int quota = 5;
        public BaliseExtraction balise;

        public int Ether { get; private set; }
        public bool QuotaAtteint => Ether >= quota;
        public bool MonolitheActive { get; set; }

        void OnEnable() => Instance = this;
        void OnDisable() { if (Instance == this) Instance = null; }

        public void AjouterEther()
        {
            Ether++;
            if (QuotaAtteint && balise != null && !balise.Active)
            {
                balise.Activer(true);
                BanqueSons.Jouer(BanqueSons.Instance?.quotaAtteint);
                Transmissions.Envoyer("Bureau du Quota", "Quota atteint. Gloire à vous ! Rejoignez la balise d'extraction, l'Empire attend sa livraison.", duree: 9f);
            }
        }

        /// <summary>Après une attaque de la Ruche : la moitié des cristaux retourne à ses gisements.</summary>
        public int PerdreMoitie()
        {
            int perte = Mathf.CeilToInt(Ether / 2f);
            Ether -= perte;
            int aRendre = perte;
            foreach (var c in CristalEther.Tous)
            {
                if (aRendre <= 0) break;
                if (c.EstRecolte) { c.Rendre(); aRendre--; }
            }
            if (balise != null && balise.Active && !QuotaAtteint)
            {
                balise.Activer(false);
                Transmissions.Envoyer("Bureau du Quota", "Quota non atteint : la balise d'extraction est coupée. Les cristaux perdus sont retournés à leurs gisements.", duree: 8f);
            }
            return perte;
        }
    }
}
