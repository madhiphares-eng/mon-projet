using System;
using System.Collections.Generic;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Formation guidée de l'Arpenteur par l'officière Vesna, du Ministère de l'Enthousiasme :
    /// treize étapes qui enseignent les commandes et les mécaniques (bruit, scanner, récolte, Ruche,
    /// navette, bulle temporelle). N passe une étape. Les étapes de navette sont sautées s'il n'y en a pas.
    /// </summary>
    public class Didacticiel : MonoBehaviour
    {
        const string TitreVesna = "Officière Vesna · Formation";

        sealed class Etape
        {
            public string id, titre, texte;
            public Func<float> progres;          // 0 à 1 (barre), facultatif
            public Func<bool> test;              // condition de réussite, facultative si progres
            public Func<Vector3?> cible;          // repère à l'écran
            public Func<string> bravo;
            public bool navette;                 // nécessite une navette
            public bool bulle;                   // nécessite une bulle lente
        }

        public bool Actif { get; private set; }
        public int Index { get; private set; }
        public int Total => etapes.Count;
        public bool EnAttente => attente > 0f;

        readonly List<Etape> etapes = new List<Etape>();
        float attente;
        int passees;

        // compteurs remis à zéro à chaque étape
        float regard, distance, course, silence, dansBulle;
        int sauts, scans, poses;
        bool verFini, verTouche;

        GestionnaireJeu jeu;

        static string T(string touche) => $"<b><color=#86EFE6>{touche}</color></b>";

        void OnEnable()
        {
            Scanner.Utilise += SurScan;
            Ruche.AttaqueTerminee += SurAttaque;
            Navette.Posee += SurPose;
        }

        void OnDisable()
        {
            Scanner.Utilise -= SurScan;
            Ruche.AttaqueTerminee -= SurAttaque;
            Navette.Posee -= SurPose;
            if (jeu != null && jeu.etatJoueur != null) jeu.etatJoueur.Saut -= SurSaut;
        }

        void SurScan() => scans++;
        void SurSaut() => sauts++;
        void SurPose() => poses++;
        void SurAttaque(bool touche) { verFini = true; verTouche = touche; }

        public void Demarrer()
        {
            jeu = GestionnaireJeu.Instance;
            if (jeu.etatJoueur != null) { jeu.etatJoueur.Saut -= SurSaut; jeu.etatJoueur.Saut += SurSaut; }
            Construire();
            Actif = true;
            Index = 0;
            passees = 0;
            attente = 0f;
            Remettre();
            SauterEtapesImpossibles();
            Afficher();
            string accueil = ProfilArpenteur.Actuel.Personnaliser(
                "Bienvenue, recrue {nom}. Ici l'officière Vesna, du Ministère de l'Enthousiasme. Formation accélérée : treize étapes, zéro question. Suivez le panneau en haut à droite.");
            Transmissions.Envoyer(TitreVesna, accueil, duree: 10f);
        }

        void Construire()
        {
            etapes.Clear();
            var cristal = new Func<Vector3?>(() =>
            {
                CristalEther proche = null;
                float d = float.MaxValue;
                foreach (var c in CristalEther.Tous)
                {
                    if (c.EstRecolte) continue;
                    float dc = Vector3.Distance(c.transform.position, jeu.PositionObservateur);
                    if (dc < d) { d = dc; proche = c; }
                }
                return proche != null ? proche.transform.position : (Vector3?)null;
            });
            var bulle = new Func<Vector3?>(() => { var b = BulleLente(); return b != null ? b.transform.position : (Vector3?)null; });
            var navette = new Func<Vector3?>(() => jeu.navette != null ? jeu.navette.transform.position : (Vector3?)null);

            Ajouter(new Etape { id = "regarder", titre = "Regarder", texte = $"Bougez la {T("souris")} pour regarder autour de vous. Si le curseur est visible, cliquez d'abord dans la scène.",
                progres = () => regard / 120f, bravo = () => "Voilà le ciel de votre lieu d'affectation. Ce qui tourne là-haut est un trou noir. Ne le fixez pas trop, il vous le rend." });
            Ajouter(new Etape { id = "bouger", titre = "Se déplacer", texte = $"Marchez une dizaine de mètres avec {T("Z")} {T("Q")} {T("S")} {T("D")} (ou {T("W")} {T("A")} {T("S")} {T("D")}, ou les flèches).",
                progres = () => distance / 12f, bravo = () => "Excellent. Chaque pas que vous faites ici est un pas pour l'Empire, et une facture pour vous." });
            Ajouter(new Etape { id = "courir", titre = "Courir", texte = $"Maintenez {T("Maj")} en avançant pour courir. Regardez la jauge de bruit, en bas : courir s'entend de loin.",
                progres = () => course / 2.5f, bravo = () => "Rapide ! Et bruyant. Sous le sable, quelque chose a tourné la tête." });
            Ajouter(new Etape { id = "silence", titre = "Marcher en silence", texte = $"Maintenez {T("C")} en avançant. Vous êtes lent, mais le bruit retombe pendant que vous marchez.",
                progres = () => silence / 3f, bravo = () => "Parfait. La discrétion est la politesse des Arpenteurs vivants." });
            Ajouter(new Etape { id = "sauter", titre = "Sauter", texte = $"Appuyez sur {T("Espace")} pour sauter. La gravité est forte, et chaque atterrissage fait un peu de bruit.",
                test = () => sauts >= 1 && jeu.etatJoueur != null && jeu.etatJoueur.AuSol, bravo = () => "La gravité locale vous salue. Elle salue tout le monde, très fort, vers le bas." });
            Ajouter(new Etape { id = "scanner", titre = "Scanner", texte = $"Appuyez sur {T("F")}. L'onde révèle les cristaux d'Éther proches (losanges cyan) et les phénomènes. Le scanner aussi fait du bruit.",
                test = () => scans >= 1, bravo = () => "Le scanner a trouvé un gisement à deux pas. Le Bureau adore les gisements à deux pas." });
            Ajouter(new Etape { id = "recolter", titre = "Récolter l'Éther", texte = $"Rejoignez le cristal signalé par le repère orange et maintenez {T("E")} jusqu'au bout de la barre. Récolter est bruyant.",
                cible = cristal, test = () => Mission.Instance != null && Mission.Instance.Ether >= 1,
                bravo = () => "Premier cristal ! Ne prêtez pas attention aux voix. Les cristaux ne parlent pas, c'est officiel." });
            Ajouter(new Etape { id = "ver", titre = "La Ruche écoute", texte = "Faites du bruit exprès (courez, sautez, scannez) jusqu'à remplir la jauge. Quand le sol tremble, éloignez-vous du point, vite : la Ruche frappe là où était le bruit.",
                test = () => verFini, bravo = () => verTouche
                    ? "Touché. Les cristaux perdus retournent à leur gisement. Retenez la leçon : le bruit vous trahit, la Ruche frappe où vous étiez."
                    : "Esquive réglementaire ! La Ruche frappe où vous étiez, jamais où vous êtes. Restez toujours en mouvement." });
            Ajouter(new Etape { id = "navette", titre = "Monter à bord", texte = $"Retournez à la navette (repère orange) et appuyez sur {T("E")} à côté d'elle.", navette = true,
                cible = navette, test = () => jeu.Etat == EtatJeu.Vol, bravo = () => "Navette déverrouillée. Elle est assurée. Vous, moins." });
            Ajouter(new Etape { id = "voler", titre = "Piloter", texte = $"{T("Souris")} pour orienter, {T("Z")}/{T("S")} pour la poussée, {T("Espace")}/{T("C")} pour monter et descendre, {T("Maj")} pour accélérer. Volez jusqu'à la bulle de temps (repère orange).",
                navette = true, bulle = true, cible = bulle,
                test = () => jeu.Etat == EtatJeu.Vol && BulleLente() != null && Vector3.Distance(jeu.PositionObservateur, BulleLente().transform.position) < 70f,
                bravo = () => "Belle trajectoire. Cette bulle brillante est une anomalie temporelle. Le Bureau la qualifie de « décorative »." });
            Ajouter(new Etape { id = "poser", titre = "Se poser", texte = $"Descendez près du sol, ralentissez, puis appuyez sur {T("E")} pour vous poser à côté de la bulle. Évitez les pentes.",
                navette = true, test = () => jeu.Etat == EtatJeu.Jeu && poses >= 1, bravo = () => "Atterrissage validé. Le sol est intact, ce qui est rare." });
            Ajouter(new Etape { id = "bulle", titre = "Le temps ralentit", texte = "Entrez dans la bulle (repère orange). Regardez les deux horloges en haut à gauche : ici, le temps s'étire, pendant que là-haut, sur le vaisseau-mère, les jours défilent. La bulle respire : si elle se dégonfle, attendez qu'elle revienne.",
                bulle = true, cible = bulle, progres = () => dansBulle / 4f,
                bravo = () => "Vous venez de vieillir moins vite que l'Empire. Le Bureau ne rembourse pas le temps perdu là-haut. Et le monolithe ? Ne le touchez pas. Officiellement." });
            Ajouter(new Etape { id = "decoller", titre = "Décoller", texte = $"Remontez dans la navette ({T("E")}) et prenez de l'altitude : au-dessus de 80 m, votre formation est terminée.",
                navette = true, cible = navette, test = () => jeu.Etat == EtatJeu.Vol && jeu.navette != null && jeu.navette.Altitude > 80f,
                bravo = () => "Formation terminée. Bienvenue parmi les Arpenteurs. Votre espérance de vie a été mise à jour." });
        }

        void Ajouter(Etape e) => etapes.Add(e);

        static BulleTemporelle BulleLente()
        {
            foreach (var b in BulleTemporelle.Toutes) if (b.EstLente) return b;
            return null;
        }

        bool Possible(Etape e) => (!e.navette || jeu.navette != null) && (!e.bulle || BulleLente() != null);

        void SauterEtapesImpossibles()
        {
            while (Index < etapes.Count && !Possible(etapes[Index])) Index++;
        }

        void Remettre()
        {
            regard = distance = course = silence = dansBulle = 0f;
            sauts = scans = poses = 0;
            verFini = verTouche = false;
        }

        void Afficher()
        {
            if (Index >= etapes.Count) return;
            var e = etapes[Index];
            if (jeu.jauge != null) jeu.jauge.RucheAutorisee = e.id == "ver";
            jeu.interfaceJeu?.MontrerEtape(Index + 1, etapes.Count, e.titre, e.texte);
        }

        /// <summary>Repère de l'étape en cours (null si aucun).</summary>
        public Vector3? Cible => Actif && attente <= 0f && Index < etapes.Count && etapes[Index].cible != null ? etapes[Index].cible() : null;

        void Valider(bool avecBravo)
        {
            if (attente > 0f || Index >= etapes.Count) return;
            var e = etapes[Index];
            if (avecBravo)
            {
                string texte = e.bravo != null ? e.bravo() : "Étape validée.";
                Transmissions.Envoyer(TitreVesna, texte, duree: 9f);
                BanqueSons.Jouer(BanqueSons.Instance?.etapeValidee, 0.8f);
                jeu.interfaceJeu?.EtapeValidee();
                attente = Index == etapes.Count - 1 ? 4.5f : 1.6f;   // le dernier mot de Vesna avant l'écran de fin
            }
            else
            {
                passees++;
                attente = 0.01f;
            }
            if (e.id == "recolter" && jeu.jauge != null) jeu.jauge.Repit = 0f;   // la Ruche peut venir à l'étape suivante
        }

        void Update()
        {
            if (!Actif || jeu == null) return;
            if (jeu.Etat != EtatJeu.Jeu && jeu.Etat != EtatJeu.Vol) return;
            float dt = Time.deltaTime;

            if (jeu.Etat == EtatJeu.Jeu && jeu.etatJoueur != null)
            {
                var j = jeu.etatJoueur;
                if (Cursor.lockState == CursorLockMode.Locked) regard += Entrees.Regard().magnitude;
                if (j.Bouge) distance += j.Vitesse * dt;
                if (j.Bouge && j.Course) course += dt;
                if (j.Bouge && j.Silence) silence += dt;
                if (BulleTemporelle.PresenceLente(jeu.PositionObservateur) > 0.3f) dansBulle += dt;
            }

            if (EntreesJeu.PasserEtape()) Valider(false);

            if (attente > 0f)
            {
                attente -= dt;
                if (attente > 0f) return;
                Index++;
                Remettre();
                SauterEtapesImpossibles();
                if (Index >= etapes.Count) { Terminer(); return; }
                Afficher();
                return;
            }

            var e = etapes[Index];
            float p = e.progres != null ? Mathf.Clamp01(e.progres()) : 0f;
            jeu.interfaceJeu?.ProgressionEtape(e.progres != null ? p : -1f);
            if ((e.progres != null && p >= 1f) || (e.test != null && e.test())) Valider(true);
        }

        void Terminer()
        {
            Actif = false;
            if (jeu.jauge != null) jeu.jauge.RucheAutorisee = true;
            jeu.TerminerFormation(etapes.Count - passees, etapes.Count);
        }
    }
}
