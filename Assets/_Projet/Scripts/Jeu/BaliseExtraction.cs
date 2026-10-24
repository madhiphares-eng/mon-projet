using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Balise d'extraction : un faisceau visible de loin, allumé quand le quota est atteint.
    /// À pied (moins de 5 m) ou en navette (moins de 45 m, touche E), elle lance l'extraction.
    /// </summary>
    public class BaliseExtraction : MonoBehaviour
    {
        public float distanceAPied = 5f;
        public float distanceNavette = 45f;
        public float hauteurFaisceau = 260f;
        [ColorUsage(false, true)] public Color couleur = new Color(2.4f, 1.3f, 0.5f);

        public bool Active { get; private set; }

        GameObject faisceau;
        Light lumiere;

        void Start()
        {
            if (transform.Find("Socle") == null)
            {
                var socle = Formes.Primitive(PrimitiveType.Cylinder, transform, Formes.Emissif(new Color(0.23f, 0.21f, 0.2f), new Color(0.95f, 0.6f, 0.3f) * 0.4f, 0.4f), "Socle");
                socle.transform.localScale = new Vector3(3.6f, 0.4f, 3.6f);
                socle.transform.localPosition = Vector3.up * 0.3f;
            }
            faisceau = Formes.Primitive(PrimitiveType.Cylinder, transform, Formes.Lueur(couleur, 1.2f, 0.25f, 0.6f), "Faisceau");
            faisceau.transform.localScale = new Vector3(2.6f, hauteurFaisceau / 2f, 2.6f);
            faisceau.transform.localPosition = Vector3.up * (hauteurFaisceau / 2f);
            lumiere = Formes.Lumiere(transform, "Lumiere", new Color(1f, 0.7f, 0.35f), 3000f, 30f);
            lumiere.transform.localPosition = Vector3.up * 4f;
            Activer(Active);
        }

        public void Activer(bool active)
        {
            Active = active;
            if (faisceau != null) faisceau.SetActive(active);
            if (lumiere != null) lumiere.enabled = active;
        }

        void Update()
        {
            var jeu = GestionnaireJeu.Instance;
            if (!Active || jeu == null || jeu.Etat != EtatJeu.Jeu) return;
            if (Vector3.Distance(jeu.joueur.transform.position, transform.position) < distanceAPied && Mission.Instance != null && Mission.Instance.QuotaAtteint)
                jeu.Extraction();
        }

        /// <summary>La navette peut-elle s'arrimer ici ?</summary>
        public bool AccessibleDepuis(Vector3 position) =>
            Active && Mission.Instance != null && Mission.Instance.QuotaAtteint && Vector3.Distance(position, transform.position) < distanceNavette;
    }
}
