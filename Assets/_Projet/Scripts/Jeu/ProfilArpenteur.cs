using System;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// L'Arpenteur du joueur : nom, matricule, spécialité et couleurs.
    /// Enregistré dans les PlayerPrefs, donc conservé d'une partie à l'autre.
    /// </summary>
    [Serializable]
    public class ProfilArpenteur
    {
        const string Cle = "EchosEther.Profil";
        static readonly string[] NomsSuggeres = { "Vega", "Orsa", "Kael", "Ilyan", "Sorel", "Maëlle", "Téo", "Nadir", "Isha", "Bram" };

        public string nom;
        public string matricule;
        public Specialite specialite = Specialite.Eclaireur;
        public string combinaison = "reglementaire";
        public string visiere = "ambre";
        public bool enrole;

        static ProfilArpenteur actuel;

        public static ProfilArpenteur Actuel => actuel ??= Charger();

        public DonneesSpecialite Donnees => DonneesSpecialite.De(specialite);
        public string NomAffiche => string.IsNullOrWhiteSpace(nom) ? matricule : nom.Trim();
        public Color CouleurCombinaison => Nuanciers.Couleur(Nuanciers.Combinaisons, combinaison);
        public Color CouleurVisiere => Nuanciers.Couleur(Nuanciers.Visieres, visiere);

        /// <summary>Remplace {nom} et {matricule} dans un texte du jeu.</summary>
        public string Personnaliser(string texte) => texte.Replace("{nom}", NomAffiche).Replace("{matricule}", matricule);

        public void Sauver()
        {
            PlayerPrefs.SetString(Cle, JsonUtility.ToJson(this));
            PlayerPrefs.Save();
        }

        static ProfilArpenteur Charger()
        {
            ProfilArpenteur p = null;
            string json = PlayerPrefs.GetString(Cle, "");
            if (!string.IsNullOrEmpty(json))
            {
                try { p = JsonUtility.FromJson<ProfilArpenteur>(json); }
                catch (ArgumentException) { p = null; }
            }
            if (p == null) p = new ProfilArpenteur { nom = NomsSuggeres[UnityEngine.Random.Range(0, NomsSuggeres.Length)] };
            if (string.IsNullOrEmpty(p.matricule)) p.matricule = NouveauMatricule();
            return p;
        }

        static string NouveauMatricule()
        {
            const string lettres = "ABCDEFGHJKLMNPRSTUVWXZ";
            return $"AR-{UnityEngine.Random.Range(1000, 10000)}-{lettres[UnityEngine.Random.Range(0, lettres.Length)]}";
        }
    }
}
