using System;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Scanner de l'Arpenteur (touche F) : une onde révèle les cristaux et les bulles temporelles proches.
    /// Bruyant, avec un temps de recharge. La portée dépend de la spécialité.
    /// </summary>
    [RequireComponent(typeof(EtatArpenteur))]
    public class Scanner : MonoBehaviour
    {
        public float porteeBase = 220f;
        public float recharge = 6f;
        public float dureeOnde = 1.8f;
        public float revelationBase = 9f;
        public float bruitBase = 18f;
        public Color couleurOnde = new Color(0.35f, 1.3f, 1.25f);

        public static event Action Utilise;
        /// <summary>Secondes avant de pouvoir scanner à nouveau.</summary>
        public float Attente => Mathf.Max(0f, prochain - Time.time);

        EtatArpenteur etat;
        JaugeBruit jauge;
        Transform onde;
        Material materiauOnde;
        float prochain, tOnde = -1f, portee;
        Vector3 origine;

        void Awake()
        {
            etat = GetComponent<EtatArpenteur>();
            jauge = GetComponent<JaugeBruit>();
        }

        void Update()
        {
            if (etat.Actif && EntreesJeu.Scanner() && Time.time >= prochain) Lancer();
            AnimerOnde();
        }

        public void Lancer()
        {
            var s = ProfilArpenteur.Actuel.Donnees;
            portee = porteeBase * s.porteeScanner;
            prochain = Time.time + recharge;
            origine = transform.position;
            tOnde = 0f;
            if (jauge != null) jauge.Ajouter(bruitBase * s.bruitScanner);
            foreach (var c in CristalEther.Tous)
                if (!c.EstRecolte && Vector3.Distance(c.transform.position, origine) < portee) c.Reveler(revelationBase * s.dureeRevele);
            foreach (var b in BulleTemporelle.Toutes)
                if (Vector3.Distance(b.transform.position, origine) < portee) b.Decouverte = true;
            BanqueSons.Jouer(BanqueSons.Instance?.scanner, 0.7f);
            Utilise?.Invoke();
        }

        void AnimerOnde()
        {
            if (tOnde < 0f) return;
            if (onde == null)
            {
                materiauOnde = Formes.Lueur(couleurOnde, 4f, 0f);
                onde = Formes.Primitive(PrimitiveType.Sphere, null, materiauOnde, "OndeScanner").transform;
            }
            tOnde += Time.deltaTime;
            float k = tOnde / dureeOnde;
            onde.gameObject.SetActive(k < 1f);
            if (k >= 1f) { tOnde = -1f; return; }
            onde.position = origine;
            onde.localScale = Vector3.one * (2f + k * portee) * 2f;
            materiauOnde.SetFloat("_Intensite", (1f - k) * 1.6f);
        }

        void OnDestroy()
        {
            if (onde != null) Destroy(onde.gameObject);
        }
    }
}
