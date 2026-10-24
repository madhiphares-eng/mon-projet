using UnityEngine;

namespace EchosEther.Combat
{
    public enum ModeTir
    {
        /// <summary>Un tir par appui (pistolet).</summary>
        Coup,
        /// <summary>Tir continu tant que la détente est tenue (carabine).</summary>
        Rafale,
        /// <summary>Plusieurs projectiles en éventail (fusil à éclats).</summary>
        Eclats,
        /// <summary>Se charge détente tenue, tire au relâchement, traverse plusieurs cibles (fusil Sablier).</summary>
        Charge,
        /// <summary>Objet lancé en cloche (grenade).</summary>
        Lancer,
        /// <summary>Coup au corps à corps (pic).</summary>
        Melee,
    }

    /// <summary>Caractéristiques d'une arme de l'Arpenteur.</summary>
    public sealed class DefinitionArme
    {
        public string id, nom, surnom, description;
        public ModeTir mode;
        /// <summary>Nom du modèle Kenney Blaster Kit (sans extension), ou vide pour une forme générée.</summary>
        public string modeleKenney;
        /// <summary>Longueur de l'arme à l'écran, en mètres.</summary>
        public float longueur = 0.6f;

        public float degats = 10f;
        /// <summary>Tirs par seconde.</summary>
        public float cadence = 4f;
        public float portee = 150f;
        /// <summary>Dispersion en degrés (hanche) ; divisée par deux en visée.</summary>
        public float dispersion = 1f;
        /// <summary>Dispersion ajoutée par tir en rafale, qui retombe ensuite.</summary>
        public float floraison = 0f;
        public int projectiles = 1;
        public int chargeur = 10;
        public int reserveMax = 60;
        public float rechargement = 1.5f;
        /// <summary>Recul de la caméra (degrés) et de l'arme à chaque tir.</summary>
        public float recul = 1f;
        /// <summary>Bruit ajouté à la jauge à chaque tir (la Ruche écoute).</summary>
        public float bruit = 8f;
        /// <summary>Vitesse du projectile visible (m/s), 0 = instantané avec traçante.</summary>
        public float vitesseProjectile = 0f;
        public float rayonExplosion = 0f;
        public float tempsCharge = 0f;
        /// <summary>Secondes de ralentissement infligées.</summary>
        public float ralenti = 0f;
        /// <summary>Nombre de cibles traversées.</summary>
        public int traversee = 1;
        public float reculCible = 3f;
        [ColorUsage(false, true)] public Color couleur = Color.white;

        public bool Infini => mode == ModeTir.Melee;
    }

    /// <summary>L'arsenal réglementaire de l'Arpenteur (dans l'ordre des touches 1 à 6).</summary>
    public static class Catalogue
    {
        public static readonly DefinitionArme[] Armes =
        {
            new DefinitionArme
            {
                id = "pic", nom = "Pic d'arpentage", surnom = "PIC", mode = ModeTir.Melee, longueur = 0.75f,
                description = "Outil de prélèvement réglementaire. Silencieux, et étonnamment convaincant de près.",
                degats = 34f, cadence = 1.7f, portee = 2.7f, bruit = 1.5f, recul = 0f, reculCible = 6f,
                couleur = new Color(0.4f, 1.6f, 1.5f),
            },
            new DefinitionArme
            {
                id = "ardent", nom = "Pistolet à impulsion Ardent", surnom = "ARDENT", mode = ModeTir.Coup, modeleKenney = "blaster-a", longueur = 0.34f,
                description = "Tire des éclairs de plasma visibles. Précis, assez discret, toujours fiable.",
                degats = 24f, cadence = 4.5f, portee = 140f, dispersion = 0.5f, chargeur = 12, reserveMax = 84, rechargement = 1.1f,
                recul = 1.3f, bruit = 7f, vitesseProjectile = 150f, reculCible = 2.5f, couleur = new Color(2.6f, 1.2f, 0.45f),
            },
            new DefinitionArme
            {
                id = "faucheuse", nom = "Carabine Faucheuse", surnom = "FAUCHEUSE", mode = ModeTir.Rafale, modeleKenney = "blaster-d", longueur = 0.78f,
                description = "Cadence élevée, traçantes bleues. Chaque rafale s'entend : la Ruche aussi aime la musique.",
                degats = 11f, cadence = 11f, portee = 160f, dispersion = 1.2f, floraison = 0.35f, chargeur = 32, reserveMax = 192, rechargement = 1.8f,
                recul = 0.55f, bruit = 3.2f, reculCible = 1.2f, couleur = new Color(0.9f, 1.4f, 2.6f),
            },
            new DefinitionArme
            {
                id = "grele", nom = "Fusil à éclats Grêle", surnom = "GRÊLE", mode = ModeTir.Eclats, modeleKenney = "blaster-l", longueur = 0.7f,
                description = "Neuf éclats en éventail. Dévastateur à courte portée, assourdissant partout.",
                degats = 10f, cadence = 1.25f, portee = 45f, dispersion = 5.5f, projectiles = 9, chargeur = 6, reserveMax = 36, rechargement = 2.4f,
                recul = 4.5f, bruit = 24f, reculCible = 5f, couleur = new Color(2.8f, 0.9f, 0.4f),
            },
            new DefinitionArme
            {
                id = "sablier", nom = "Fusil chronique Sablier", surnom = "SABLIER", mode = ModeTir.Charge, modeleKenney = "blaster-e", longueur = 1.15f,
                description = "Se charge (détente tenue) puis tire un rayon qui traverse trois cibles et les englue dans le temps. Presque silencieux.",
                degats = 80f, cadence = 1.4f, portee = 260f, dispersion = 0f, chargeur = 4, reserveMax = 20, rechargement = 2.3f,
                recul = 3.2f, bruit = 2.5f, tempsCharge = 0.9f, ralenti = 4f, traversee = 3, reculCible = 2f, couleur = new Color(1.5f, 0.7f, 2.8f),
            },
            new DefinitionArme
            {
                id = "echo", nom = "Grenade leurre Écho", surnom = "ÉCHO", mode = ModeTir.Lancer, modeleKenney = "grenade-a", longueur = 0.16f,
                description = "Explose au bout de deux secondes en hurlant : la Ruche frappe là où elle a crié. Attirez le ver sur l'essaim.",
                degats = 60f, cadence = 1.2f, portee = 0f, chargeur = 1, reserveMax = 3, rechargement = 0.6f,
                recul = 0f, bruit = 4f, rayonExplosion = 6.5f, reculCible = 9f, couleur = new Color(0.4f, 2.4f, 1.6f),
            },
        };

        public static int Index(string id)
        {
            for (int i = 0; i < Armes.Length; i++) if (Armes[i].id == id) return i;
            return -1;
        }
    }
}
