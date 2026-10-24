using System;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Jauge de bruit de l'Arpenteur (0 à 100), inspirée de Dune : courir, sauter, scanner et récolter
    /// font monter la jauge ; à 100, la Ruche des Sables attaque l'endroit où était le bruit.
    /// </summary>
    [RequireComponent(typeof(EtatArpenteur))]
    public class JaugeBruit : MonoBehaviour
    {
        [Header("Variation par seconde")]
        public float decroissance = 16f;
        public float marche = 4f;
        public float course = 26f;
        public float silence = -9f;
        public float recolte = 20f;

        [Header("Bruits ponctuels")]
        public float saut = 9f;
        public float atterrissage = 10f;
        [Tooltip("Vitesse de chute (m/s) à partir de laquelle un atterrissage fait du bruit.")]
        public float chuteBruyante = 8f;

        /// <summary>Déclenché quand la jauge atteint 100 (position du bruit).</summary>
        public static event Action<Vector3> BruitMaximal;

        /// <summary>Un bruit ponctuel s'est fait entendre (position, intensité) : les créatures tendent l'oreille.</summary>
        public static event Action<Vector3, float> BruitEmis;

        /// <summary>Multiplie tous les bruits de l'Arpenteur (compétence Voile de silence, talent Pas feutrés).</summary>
        public static float MultiplicateurBruit = 1f;

        /// <summary>Signale un bruit qui ne vient pas de l'Arpenteur (explosion, créature) aux créatures proches.</summary>
        public static void Signaler(Vector3 position, float intensite) => BruitEmis?.Invoke(position, intensite);

        /// <summary>
        /// Leurre : appelle la Ruche à un autre endroit que l'Arpenteur (grenade Écho, hologramme).
        /// Respecte le didacticiel (Ruche interdite hors de son étape). Renvoie true si le ver vient.
        /// </summary>
        public static bool Leurre(Vector3 position)
        {
            var jeu = GestionnaireJeu.Instance;
            var jauge = jeu != null ? jeu.jauge : null;
            if (jauge != null && !jauge.RucheAutorisee) return false;
            if (Ruche.Instance == null || Ruche.Instance.EnCours) return false;
            BruitMaximal?.Invoke(position);
            return true;
        }

        public float Bruit { get; private set; }
        /// <summary>Plus haut niveau atteint pendant la partie.</summary>
        public float BruitMax { get; private set; }
        /// <summary>Le didacticiel la désactive jusqu'à l'étape de la Ruche.</summary>
        public bool RucheAutorisee { get; set; } = true;
        /// <summary>Secondes pendant lesquelles la Ruche ne peut pas revenir.</summary>
        public float Repit { get; set; }

        EtatArpenteur etat;
        bool recolteEnCours;
        float pic;

        void Awake() => etat = GetComponent<EtatArpenteur>();

        void OnEnable()
        {
            etat.Saut += SurSaut;
            etat.Atterrissage += SurAtterrissage;
        }

        void OnDisable()
        {
            etat.Saut -= SurSaut;
            etat.Atterrissage -= SurAtterrissage;
        }

        public void Reinitialiser() { Bruit = 0f; BruitMax = 0f; pic = 0f; Repit = 0f; }

        /// <summary>Bruit ponctuel (scanner, impact...).</summary>
        public void Ajouter(float quantite)
        {
            if (quantite > 0f) quantite *= MultiplicateurBruit;
            Bruit = Mathf.Clamp(Bruit + quantite, 0f, 100f);
            pic = Mathf.Max(pic, Bruit);
            if (quantite > 0.5f) BruitEmis?.Invoke(transform.position, quantite);
        }

        /// <summary>Appelé à chaque image pendant une récolte.</summary>
        public void SignalerRecolte() => recolteEnCours = true;

        /// <summary>Après une attaque, la jauge redescend et la Ruche se tait un moment.</summary>
        public void ApresAttaque(float plafond, float repit)
        {
            Bruit = Mathf.Min(Bruit, plafond);
            Repit = repit;
        }

        void SurSaut() => Ajouter(saut * ProfilArpenteur.Actuel.Donnees.bruitPas);

        void SurAtterrissage(float vitesseChute)
        {
            if (vitesseChute >= chuteBruyante) Ajouter(atterrissage * ProfilArpenteur.Actuel.Donnees.bruitPas);
        }

        void Update()
        {
            if (!etat.Actif) { recolteEnCours = false; return; }
            var s = ProfilArpenteur.Actuel.Donnees;
            float taux = -decroissance;
            if (etat.Bouge) taux = etat.Silence ? silence : (etat.Course ? course : marche) * s.bruitPas;
            if (recolteEnCours) taux = Mathf.Max(taux, 0f) + recolte * s.bruitRecolte;
            recolteEnCours = false;
            if (taux > 0f) taux *= MultiplicateurBruit;

            float avant = Mathf.Max(pic, Bruit);   // un bruit ponctuel compte avant la décroissance
            pic = 0f;
            Bruit = Mathf.Clamp(Bruit + taux * Time.deltaTime, 0f, 100f);
            BruitMax = Mathf.Max(BruitMax, avant, Bruit);
            if (Repit > 0f) Repit -= Time.deltaTime;

            bool rucheLibre = Ruche.Instance == null || !Ruche.Instance.EnCours;
            if (Mathf.Max(avant, Bruit) >= 100f && Repit <= 0f && RucheAutorisee && rucheLibre)
                BruitMaximal?.Invoke(transform.position);
        }
    }
}
