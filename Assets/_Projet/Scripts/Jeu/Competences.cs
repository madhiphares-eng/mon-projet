using System.Collections.Generic;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>Compétence active de l'Arpenteur (une seule équipée, touche Q, bouton RB à la manette).</summary>
    public enum CompetenceActive { Bond, Voile, Stase, Leurre, Onde, Bouclier }

    /// <summary>Talents passifs : un au choix à l'enrôlement, puis un par niveau gagné.</summary>
    public enum Talent
    {
        PasFeutres, PiedLeger, MainSure, ChargeursEtendus, RechargementEclair, CombinaisonRenforcee,
        Regeneration, RecolteurExpert, EchoProfond, Artificier, CoupDePic, Recuperation, Pillard, Chronophage,
    }

    public readonly struct InfoCompetence
    {
        public readonly string nom, description;
        public readonly float recharge, duree;
        [ColorUsage(false, true)] public readonly Color couleur;
        public InfoCompetence(string nom, string description, float recharge, float duree, Color couleur)
        {
            this.nom = nom; this.description = description; this.recharge = recharge; this.duree = duree; this.couleur = couleur;
        }
    }

    public readonly struct InfoTalent
    {
        public readonly string nom, description;
        public InfoTalent(string nom, string description) { this.nom = nom; this.description = description; }
    }

    /// <summary>Catalogue des compétences et des talents, et courbe d'expérience.</summary>
    public static class Competences
    {
        public const int NiveauMax = 15;

        public static readonly CompetenceActive[] Actives =
            { CompetenceActive.Bond, CompetenceActive.Voile, CompetenceActive.Stase, CompetenceActive.Leurre, CompetenceActive.Onde, CompetenceActive.Bouclier };

        public static readonly Talent[] Talents = (Talent[])System.Enum.GetValues(typeof(Talent));

        public static InfoCompetence Info(CompetenceActive c)
        {
            switch (c)
            {
                case CompetenceActive.Voile:
                    return new InfoCompetence("Voile de silence", "Huit secondes sans presque aucun bruit : pas, tirs et récolte. La Ruche n'entend plus rien.", 25f, 8f, new Color(0.9f, 0.7f, 2.2f));
                case CompetenceActive.Stase:
                    return new InfoCompetence("Bulle de stase", "Crée une bulle de temps lent là où vous visez : les créatures s'y engluent pendant sept secondes.", 28f, 7f, new Color(0.45f, 0.8f, 2.2f));
                case CompetenceActive.Leurre:
                    return new InfoCompetence("Leurre holographique", "Un double de vous attire les créatures, puis crie : la Ruche frappe à sa place.", 22f, 6f, new Color(0.5f, 2.2f, 1.8f));
                case CompetenceActive.Onde:
                    return new InfoCompetence("Onde de choc", "Frappe le sol : repousse et blesse tout ce qui est à moins de sept mètres. Très bruyant.", 13f, 0f, new Color(2.4f, 1.4f, 0.6f));
                case CompetenceActive.Bouclier:
                    return new InfoCompetence("Bouclier d'Éther", "Absorbe soixante points de dégâts pendant huit secondes.", 24f, 8f, new Color(0.4f, 1.8f, 2.4f));
                default:
                    return new InfoCompetence("Bond propulsé", "Les réacteurs dorsaux vous projettent vers l'avant et vers le haut : franchir, fuir, surprendre.", 6f, 0f, new Color(2.4f, 1.2f, 0.5f));
            }
        }

        public static InfoTalent Info(Talent t)
        {
            switch (t)
            {
                case Talent.PasFeutres: return new InfoTalent("Pas feutrés", "Tous vos bruits sont réduits de 25 %.");
                case Talent.PiedLeger: return new InfoTalent("Pied léger", "Marche et course 12 % plus rapides.");
                case Talent.MainSure: return new InfoTalent("Main sûre", "Dispersion et recul des armes réduits de 40 %.");
                case Talent.ChargeursEtendus: return new InfoTalent("Chargeurs étendus", "Chargeurs 50 % plus grands, réserves 25 % plus grandes.");
                case Talent.RechargementEclair: return new InfoTalent("Rechargement éclair", "Rechargement 35 % plus rapide.");
                case Talent.CombinaisonRenforcee: return new InfoTalent("Combinaison renforcée", "Intégrité maximale +40.");
                case Talent.Regeneration: return new InfoTalent("Nanoréparation", "La combinaison se répare plus tôt et trois fois plus vite.");
                case Talent.RecolteurExpert: return new InfoTalent("Récolteur expert", "Récolte 40 % plus rapide.");
                case Talent.EchoProfond: return new InfoTalent("Écho profond", "Portée du scanner +50 %, et il révèle les créatures.");
                case Talent.Artificier: return new InfoTalent("Artificier", "Deux grenades de plus, rayon d'explosion +30 %.");
                case Talent.CoupDePic: return new InfoTalent("Coup de pic", "Pic +60 % de dégâts ; chaque créature achevée au pic répare 10 d'intégrité.");
                case Talent.Recuperation: return new InfoTalent("Récupération", "Recharge de la compétence active 30 % plus courte.");
                case Talent.Pillard: return new InfoTalent("Pillard", "Les créatures lâchent deux fois plus de cellules d'énergie.");
                default: return new InfoTalent("Chronophage", "Dans une bulle de temps lent, vos armes tirent et rechargent 30 % plus vite.");
            }
        }

        /// <summary>Expérience totale nécessaire pour atteindre un niveau (niveau 1 = 0).</summary>
        public static int Seuil(int niveau)
        {
            int total = 0;
            for (int n = 1; n < niveau; n++) total += 100 + 75 * (n - 1);
            return total;
        }

        /// <summary>
        /// Trois talents proposés au choix pour un point gagné. Tirage stable : rouvrir l'écran
        /// ne relance pas le hasard.
        /// </summary>
        public static List<Talent> Propositions(ProfilArpenteur profil, int nombre = 3)
        {
            var restants = new List<Talent>();
            foreach (var t in Talents) if (!profil.talents.Contains(t)) restants.Add(t);
            var hasard = new System.Random((profil.matricule ?? "").GetHashCode() ^ (profil.niveau * 7919) ^ (profil.talents.Count * 104729));
            for (int i = restants.Count - 1; i > 0; i--)
            {
                int j = hasard.Next(i + 1);
                (restants[i], restants[j]) = (restants[j], restants[i]);
            }
            if (restants.Count > nombre) restants.RemoveRange(nombre, restants.Count - nombre);
            return restants;
        }
    }
}
