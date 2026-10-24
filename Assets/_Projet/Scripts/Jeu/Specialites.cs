using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>Spécialités d'Arpenteur choisies à l'enrôlement. Chacune change une règle du jeu.</summary>
    public enum Specialite { Eclaireur, Foreur, Chronologue }

    /// <summary>Règles et textes d'une spécialité (multiplicateurs appliqués par les mécaniques).</summary>
    public readonly struct DonneesSpecialite
    {
        public readonly string nom, atout, bureau;
        public readonly float bruitPas;        // marche, course, saut, atterrissage
        public readonly float vitesseRecolte;  // 2 = deux fois plus rapide
        public readonly float bruitRecolte;
        public readonly float porteeScanner;
        public readonly float dureeRevele;     // durée d'affichage des repères du scanner
        public readonly float bruitScanner;

        DonneesSpecialite(string nom, string atout, string bureau, float bruitPas = 1f, float vitesseRecolte = 1f,
            float bruitRecolte = 1f, float porteeScanner = 1f, float dureeRevele = 1f, float bruitScanner = 1f)
        {
            this.nom = nom; this.atout = atout; this.bureau = bureau;
            this.bruitPas = bruitPas; this.vitesseRecolte = vitesseRecolte; this.bruitRecolte = bruitRecolte;
            this.porteeScanner = porteeScanner; this.dureeRevele = dureeRevele; this.bruitScanner = bruitScanner;
        }

        public static DonneesSpecialite De(Specialite s)
        {
            switch (s)
            {
                case Specialite.Foreur:
                    return new DonneesSpecialite("Foreur",
                        "Foreuse lourde : récolte deux fois plus rapide, mais nettement plus bruyante.",
                        "« Le quota n'attend pas. La faune locale non plus. »",
                        vitesseRecolte: 2f, bruitRecolte: 1.6f);
                case Specialite.Chronologue:
                    return new DonneesSpecialite("Chronologue",
                        "Sens du temps : scanner à longue portée (×1,6), plus discret, et repères visibles plus longtemps.",
                        "« Le Bureau décline toute responsabilité pour les souvenirs que vous n'avez pas encore vécus. »",
                        porteeScanner: 1.6f, dureeRevele: 1.7f, bruitScanner: 0.6f);
                default:
                    return new DonneesSpecialite("Éclaireur",
                        "Pas feutrés : vos déplacements, sauts et atterrissages font 45 % de bruit en moins.",
                        "« Le silence est une vertu. Le Bureau l'apprécie aussi dans vos réclamations. »",
                        bruitPas: 0.55f);
            }
        }
    }

    /// <summary>Couleurs proposées pour la combinaison et la visière.</summary>
    public static class Nuanciers
    {
        public readonly struct Teinte
        {
            public readonly string cle, nom;
            public readonly Color couleur;
            public Teinte(string cle, string nom, string hex)
            {
                this.cle = cle; this.nom = nom;
                ColorUtility.TryParseHtmlString(hex, out couleur);
            }
        }

        public static readonly Teinte[] Combinaisons =
        {
            new Teinte("reglementaire", "Réglementaire", "#D9DEE6"),
            new Teinte("ocre", "Ocre", "#C8A06A"),
            new Teinte("acier", "Acier", "#8A95A3"),
            new Teinte("nuit", "Nuit", "#3D4C6B"),
            new Teinte("rouille", "Rouille", "#A9573F"),
        };

        public static readonly Teinte[] Visieres =
        {
            new Teinte("ambre", "Ambre", "#F2A24B"),
            new Teinte("ether", "Éther", "#5FF0DE"),
            new Teinte("aurore", "Aurore", "#FF6FAE"),
            new Teinte("lune", "Lune", "#CFD8FF"),
        };

        public static Color Couleur(Teinte[] liste, string cle)
        {
            foreach (var t in liste) if (t.cle == cle) return t.couleur;
            return liste[0].couleur;
        }
    }
}
