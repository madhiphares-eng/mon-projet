using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// L'Arpenteur du joueur : nom, matricule, spécialité, couleurs, compétence active, talents et expérience.
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

        public CompetenceActive competence = CompetenceActive.Bond;
        public List<Talent> talents = new List<Talent>();
        public int niveau = 1;
        public int experience;
        /// <summary>Talents gagnés pas encore choisis.</summary>
        public int pointsTalent;

        static ProfilArpenteur actuel;

        /// <summary>Oublie le profil en mémoire : il sera relu des PlayerPrefs (après un test automatique).</summary>
        public static void Oublier() => actuel = null;

        public static ProfilArpenteur Actuel => actuel ??= Charger();

        public DonneesSpecialite Donnees => DonneesSpecialite.De(specialite);
        public string NomAffiche => string.IsNullOrWhiteSpace(nom) ? matricule : nom.Trim();
        public Color CouleurCombinaison => Nuanciers.Couleur(Nuanciers.Combinaisons, combinaison);
        public Color CouleurVisiere => Nuanciers.Couleur(Nuanciers.Visieres, visiere);

        public bool A(Talent t) => talents != null && talents.Contains(t);

        /// <summary>Ajoute de l'expérience ; renvoie le nombre de niveaux gagnés (un point de talent chacun).</summary>
        public int AjouterExperience(int quantite)
        {
            if (quantite <= 0 || niveau >= Competences.NiveauMax) return 0;
            experience += quantite;
            int gagnes = 0;
            while (niveau < Competences.NiveauMax && experience >= Competences.Seuil(niveau + 1))
            {
                niveau++;
                pointsTalent++;
                gagnes++;
            }
            return gagnes;
        }

        /// <summary>Progression dans le niveau actuel (0 à 1).</summary>
        public float ProgressionNiveau
        {
            get
            {
                if (niveau >= Competences.NiveauMax) return 1f;
                int a = Competences.Seuil(niveau), b = Competences.Seuil(niveau + 1);
                return Mathf.Clamp01((experience - a) / (float)Mathf.Max(1, b - a));
            }
        }

        /// <summary>Choisit un talent avec un point disponible.</summary>
        public bool Choisir(Talent t)
        {
            if (pointsTalent <= 0 || A(t)) return false;
            talents.Add(t);
            pointsTalent--;
            Sauver();
            return true;
        }

        /// <summary>Remplace le talent de départ (enrôlement), sans dépenser de point.</summary>
        public void TalentDeDepart(Talent t)
        {
            if (talents.Count == 0) talents.Add(t);
            else talents[0] = t;
            for (int i = talents.Count - 1; i > 0; i--) if (talents[i] == t) talents.RemoveAt(i);
        }

        /// <summary>Rend tous les talents sauf celui de départ (points récupérés).</summary>
        public void ReinitialiserTalents()
        {
            if (talents.Count <= 1) return;
            pointsTalent += talents.Count - 1;
            talents.RemoveRange(1, talents.Count - 1);
        }

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
            if (p.talents == null) p.talents = new List<Talent>();
            if (p.talents.Count == 0) p.talents.Add(DepartConseille(p.specialite));
            if (p.niveau < 1) p.niveau = 1;
            return p;
        }

        /// <summary>Talent de départ proposé par défaut selon la spécialité.</summary>
        public static Talent DepartConseille(Specialite s) =>
            s == Specialite.Foreur ? Talent.CombinaisonRenforcee : s == Specialite.Chronologue ? Talent.Chronophage : Talent.PiedLeger;

        static string NouveauMatricule()
        {
            const string lettres = "ABCDEFGHJKLMNPRSTUVWXZ";
            return $"AR-{UnityEngine.Random.Range(1000, 10000)}-{lettres[UnityEngine.Random.Range(0, lettres.Length)]}";
        }
    }
}
