using System.Collections.Generic;
using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchosEther.Interface
{
    /// <summary>Écrans de l'interface : menu principal, enrôlement, arrivée, options, crédits, pause, fin, journal.</summary>
    public partial class InterfaceJeu
    {
        VisualElement ecranMenu, ecranEnrolement, ecranOptions, ecranCredits, ecranPause, ecranFin, journal, listeJournal;
        Label profilResume, matricule, finSurtitre, finTitre, finTemps, finVerite, finChiffre4Titre;
        Label finVecu, finVaisseau, finEther, finChiffre4;
        Button boutonDidacticiel, boutonOptions, boutonEmbarquer, boutonRecommencer, boutonReprendre, boutonRejouer, boutonRetourOptions;
        TextField champNom;
        readonly List<(Button bouton, Specialite spec)> boutonsSpecialite = new List<(Button, Specialite)>();
        readonly List<(Button bouton, string cle)> pastillesCombi = new List<(Button, string)>();
        readonly List<(Button bouton, string cle)> pastillesVisiere = new List<(Button, string)>();
        Slider curseurSensibilite, curseurVolume;
        Toggle caseInverser, caseSon;
        DropdownField listeQualite;
        VisualElement retourOptions, ecranLargage, largageTitre;
        Label largageSous, largageReplique;
        TypePartie typeEnrolement;
        float glissement, dernierGlissement = -10f;
        bool glisse;

        /// <summary>Options ou journal ouverts par-dessus la pause.</summary>
        public bool SousEcranOuvert => Visible(ecranOptions) || Visible(journal);

        static bool Visible(VisualElement e) => e != null && e.style.display.value != DisplayStyle.None;

        void ConstruireEcrans()
        {
            ConstruireMenu();
            ConstruireEnrolement();
            ConstruireOptions();
            ConstruireCredits();
            ConstruirePause();
            ConstruireFin();
            ConstruireJournal();
            ConstruireLargage();
        }

        void CacherTout()
        {
            foreach (var e in new[] { ecranMenu, ecranEnrolement, ecranOptions, ecranCredits, ecranPause, ecranFin, journal, ecranLargage }) Afficher(e, false);
            Afficher(hud, false);
        }

        // ------------------------------------------------------------------ menu principal

        void ConstruireMenu()
        {
            ecranMenu = Boite(racine, "ecran", "gauche");
            var carte = Boite(ecranMenu, "carte");
            Texte(carte, "PROTOTYPE JOUABLE · PHASE 1 · LISIÈRE DE L'HORIZON", "surtitre");
            var titre = Boite(carte);
            Texte(titre, "ÉCHOS", "titre");
            var ligne = Boite(titre, "titre-ligne");
            Texte(ligne, "D'", "titre");
            Texte(ligne, "ÉTHER", "titre", "ether");
            var ordre = Boite(carte, "ordre");
            Texte(ordre, "ORDRE DE L'EMPIRE N° 7-204", "ordre-titre");
            Texte(ordre, "Arpenteur ! L'Empire a besoin d'Éther, et vous êtes volontaire. Vous serez largué sur un monde au bord d'un trou noir. Le Bureau du Quota vous remercie de ne pas poser de questions.", "texte");
            Texte(carte, "Ici, une heure vaut trois jours là-haut. Faites peu de bruit : quelque chose écoute sous le sable.", "echo-texte");
            boutonDidacticiel = BoutonMenu(carte, "DIDACTICIEL", "Formation de l'Arpenteur, une dizaine de minutes", () => Jeu.OuvrirEnrolement(TypePartie.Didacticiel), true);
            BoutonMenu(carte, "MISSION", "Cinq cristaux d'Éther, une balise, un monolithe", () => Jeu.OuvrirEnrolement(TypePartie.Mission), false);
            var rangee = Boite(carte, "rangee");
            boutonOptions = Bouton(rangee, "OPTIONS", () => OuvrirOptions(ecranMenu), "tertiaire");
            Bouton(rangee, "CRÉDITS", () => { Afficher(ecranMenu, false); Afficher(ecranCredits, true); }, "tertiaire");
            Bouton(rangee, "QUITTER", () => Jeu.Quitter(), "tertiaire");
            profilResume = Texte(carte, "", "note");
        }

        Button BoutonMenu(VisualElement parent, string titre, string sousTitre, System.Action action, bool principal)
        {
            var b = Bouton(parent, "", action, "bouton-menu", principal ? "principal" : "");
            Texte(b, titre).pickingMode = PickingMode.Ignore;
            Texte(b, sousTitre, "sous").pickingMode = PickingMode.Ignore;
            return b;
        }

        public void MontrerMenu()
        {
            CacherTout();
            var p = ProfilArpenteur.Actuel;
            profilResume.text = p.enrole ? $"Arpenteur enrôlé : {p.NomAffiche} ({p.matricule}) · {p.Donnees.nom}" : "";
            Afficher(profilResume, p.enrole);
            Fondu(Color.black, 0f);
            Afficher(ecranMenu, true);
            boutonDidacticiel.Focus();
        }

        // ------------------------------------------------------------------ enrôlement

        void ConstruireEnrolement()
        {
            ecranEnrolement = Boite(racine, "ecran", "gauche", "sans-voile");
            ecranEnrolement.style.justifyContent = Justify.FlexStart;
            var defilement = Ajout(ecranEnrolement, new ScrollView(ScrollViewMode.Vertical), "defilement");
            var carte = Boite(defilement, "carte", "etroite");
            Texte(carte, "FORMULAIRE 27-B · ENRÔLEMENT VOLONTAIRE OBLIGATOIRE", "surtitre");
            Texte(carte, "VOTRE ARPENTEUR", "titre-2");
            champNom = Ajout(carte, new TextField("Nom") { maxLength = 18 }, "champ-nom");
            champNom.RegisterValueChangedCallback(e => ProfilArpenteur.Actuel.nom = NettoyerNom(e.newValue));
            matricule = Texte(carte, "", "note");

            Texte(carte, "SPÉCIALITÉ", "etiquette");
            foreach (Specialite s in System.Enum.GetValues(typeof(Specialite)))
            {
                var d = DonneesSpecialite.De(s);
                var spec = s;
                var b = Bouton(carte, "", () => ChoisirSpecialite(spec), "spec");
                Texte(b, d.nom.ToUpperInvariant(), "spec-nom").pickingMode = PickingMode.Ignore;
                Texte(b, d.atout, "spec-atout").pickingMode = PickingMode.Ignore;
                Texte(b, d.bureau, "spec-bureau").pickingMode = PickingMode.Ignore;
                boutonsSpecialite.Add((b, s));
            }

            Texte(carte, "COMBINAISON", "etiquette");
            var combis = Boite(carte, "rangee");
            foreach (var t in Nuanciers.Combinaisons) pastillesCombi.Add((Pastille(combis, t, () => ChoisirCombinaison(t.cle), false), t.cle));
            Texte(carte, "VISIÈRE", "etiquette");
            var visieres = Boite(carte, "rangee");
            foreach (var t in Nuanciers.Visieres) pastillesVisiere.Add((Pastille(visieres, t, () => ChoisirVisiere(t.cle), true), t.cle));

            var actions = Boite(carte, "rangee");
            boutonEmbarquer = Bouton(actions, "EMBARQUER", Embarquer, "principal");
            Bouton(actions, "RETOUR", () => Jeu.RetourMenu());
            Texte(carte, "Le Bureau rappelle que l'enrôlement est volontaire et obligatoire.", "note");
            Texte(ecranEnrolement, "Glisser à droite pour faire tourner l'Arpenteur", "indice-apercu").pickingMode = PickingMode.Ignore;

            // glisser sur le fond (hors carte) fait tourner l'aperçu
            ecranEnrolement.pickingMode = PickingMode.Position;
            ecranEnrolement.RegisterCallback<PointerDownEvent>(e =>
            {
                if (e.target != ecranEnrolement) return;
                glisse = true;
                ecranEnrolement.CapturePointer(e.pointerId);
            });
            ecranEnrolement.RegisterCallback<PointerMoveEvent>(e =>
            {
                if (!glisse) return;
                glissement -= e.deltaPosition.x * 0.4f;
                dernierGlissement = Time.unscaledTime;
            });
            ecranEnrolement.RegisterCallback<PointerUpEvent>(e =>
            {
                glisse = false;
                ecranEnrolement.ReleasePointer(e.pointerId);
            });
        }

        Button Pastille(VisualElement parent, Nuanciers.Teinte teinte, System.Action action, bool lueur)
        {
            var b = Bouton(parent, "", action, "pastille");
            var rond = Boite(b, "pastille-rond");
            rond.style.backgroundColor = teinte.couleur;
            if (lueur) rond.style.borderTopColor = rond.style.borderBottomColor = rond.style.borderLeftColor = rond.style.borderRightColor = new Color(0.1f, 0.07f, 0.04f);
            rond.pickingMode = PickingMode.Ignore;
            Texte(b, teinte.nom, "pastille-nom").pickingMode = PickingMode.Ignore;
            return b;
        }

        static string NettoyerNom(string nom) => string.Join(" ", (nom ?? "").Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries));

        /// <summary>Rotation demandée par glissement depuis le dernier appel (degrés).</summary>
        public bool GlisserApercu(out float degres)
        {
            degres = glissement;
            glissement = 0f;
            return Time.unscaledTime - dernierGlissement < 2.5f;
        }

        public void MontrerEnrolement(TypePartie type)
        {
            typeEnrolement = type;
            CacherTout();
            var p = ProfilArpenteur.Actuel;
            champNom.SetValueWithoutNotify(p.nom ?? "");
            matricule.text = $"Matricule attribué : {p.matricule}";
            boutonEmbarquer.text = type == TypePartie.Didacticiel ? "COMMENCER LA FORMATION" : "EMBARQUER";
            MajChoix();
            Afficher(ecranEnrolement, true);
            champNom.Focus();
        }

        void ChoisirSpecialite(Specialite s) { ProfilArpenteur.Actuel.specialite = s; MajChoix(); }
        void ChoisirCombinaison(string cle) { ProfilArpenteur.Actuel.combinaison = cle; MajChoix(); }
        void ChoisirVisiere(string cle) { ProfilArpenteur.Actuel.visiere = cle; MajChoix(); }

        void MajChoix()
        {
            var p = ProfilArpenteur.Actuel;
            foreach (var (b, s) in boutonsSpecialite) b.EnableInClassList("choisi", s == p.specialite);
            foreach (var (b, cle) in pastillesCombi) b.EnableInClassList("choisi", cle == p.combinaison);
            foreach (var (b, cle) in pastillesVisiere) b.EnableInClassList("choisi", cle == p.visiere);
            if (Jeu != null && Jeu.apparence != null) Jeu.apparence.Appliquer();
        }

        void Embarquer()
        {
            var p = ProfilArpenteur.Actuel;
            p.nom = NettoyerNom(champNom.value);
            p.Sauver();
            Jeu.Lancer(typeEnrolement);
        }

        // ------------------------------------------------------------------ partie

        public void MontrerJeu(TypePartie type)
        {
            CacherTout();
            modeDidacticiel = type == TypePartie.Didacticiel;
            Afficher(hud, true);
            Afficher(tuto, modeDidacticiel);
            Afficher(objectifs, !modeDidacticiel);
            ModeVol(Jeu != null && Jeu.Etat == EtatJeu.Vol);
            Fondu(Color.black, 0f);
        }

        // ------------------------------------------------------------------ arrivée en navette

        void ConstruireLargage()
        {
            ecranLargage = Boite(racine, "largage");
            Boite(ecranLargage, "bande", "haut");
            Boite(ecranLargage, "bande", "bas");
            largageTitre = Boite(ecranLargage, "largage-titre");
            Texte(largageTitre, "LISIÈRE DE L'HORIZON", "largage-lieu");
            largageSous = Texte(largageTitre, "", "largage-sous");
            largageReplique = Texte(ecranLargage, "", "largage-replique");
            Texte(ecranLargage, "Espace : passer l'arrivée", "largage-passer");
            ecranLargage.pickingMode = PickingMode.Ignore;
            ecranLargage.Query<VisualElement>().ForEach(e => e.pickingMode = PickingMode.Ignore);
        }

        /// <summary>Bandes de cinéma, nom du lieu et répliques du pilote pendant l'arrivée.</summary>
        public void MontrerLargage(TypePartie type)
        {
            CacherTout();
            largageSous.text = (type == TypePartie.Didacticiel ? "FORMATION · ARPENTEUR " : "MISSION · ARPENTEUR ") + ProfilArpenteur.Actuel.NomAffiche.ToUpperInvariant();
            largageReplique.text = "";
            largageTitre.RemoveFromClassList("visible");
            Afficher(ecranLargage, true);
            largageTitre.schedule.Execute(() => largageTitre.AddToClassList("visible")).StartingIn(900);
            largageTitre.schedule.Execute(() => largageTitre.RemoveFromClassList("visible")).StartingIn(4600);
        }

        public void RepliqueLargage(string texte)
        {
            if (largageReplique != null) largageReplique.text = texte;
        }

        public void MontrerPause(bool didacticiel)
        {
            Afficher(journal, false);
            boutonRecommencer.text = didacticiel ? "RECOMMENCER LA FORMATION" : "NOUVEAU LARGAGE";
            Afficher(ecranPause, true);
            boutonReprendre.Focus();
        }

        void ConstruirePause()
        {
            ecranPause = Boite(racine, "ecran", "centre");
            var carte = Boite(ecranPause, "carte", "etroite");
            Texte(carte, "PAUSE", "surtitre");
            Texte(carte, "LE TEMPS EST SUSPENDU", "titre-2");
            Texte(carte, "Rien ne s'écoule tant que le jeu est en pause, ni ici ni sur le vaisseau.", "note");
            var r1 = Boite(carte, "rangee");
            boutonReprendre = Bouton(r1, "REPRENDRE", () => Jeu.Reprendre(), "principal");
            boutonRecommencer = Bouton(r1, "NOUVEAU LARGAGE", () => Jeu.Recommencer());
            var r2 = Boite(carte, "rangee");
            Bouton(r2, "OPTIONS", () => OuvrirOptions(ecranPause), "tertiaire");
            Bouton(r2, "MENU PRINCIPAL", () => Jeu.MenuPrincipal(), "tertiaire");
        }

        // ------------------------------------------------------------------ options

        void ConstruireOptions()
        {
            ecranOptions = Boite(racine, "ecran", "centre");
            var carte = Boite(ecranOptions, "carte", "etroite", "options");
            Texte(carte, "RÉGLAGES", "surtitre");
            Texte(carte, "OPTIONS", "titre-2");

            curseurSensibilite = Ajout(carte, new Slider("Sensibilité de la souris", 0.4f, 2f));
            curseurSensibilite.RegisterValueChangedCallback(e =>
            {
                Reglages.Sensibilite = e.newValue;
                CameraOrbitale.MultiplicateurSensibilite = e.newValue;
            });
            caseInverser = Ajout(carte, new Toggle("Inverser l'axe vertical"));
            caseInverser.RegisterValueChangedCallback(e => { Reglages.InverserY = e.newValue; CameraOrbitale.InverserY = e.newValue; });
            var niveaux = new List<string>(QualitySettings.names);
            if (niveaux.Count == 0) niveaux.Add("Par défaut");
            listeQualite = Ajout(carte, new DropdownField("Qualité d'image", niveaux, Mathf.Clamp(Reglages.Qualite, 0, niveaux.Count - 1)));
            listeQualite.RegisterValueChangedCallback(e => { Reglages.Qualite = listeQualite.index; Reglages.Appliquer(); });
            caseSon = Ajout(carte, new Toggle("Son activé"));
            caseSon.RegisterValueChangedCallback(e => { Reglages.Son = e.newValue; Reglages.Appliquer(); });
            curseurVolume = Ajout(carte, new Slider("Volume", 0f, 1f));
            curseurVolume.RegisterValueChangedCallback(e => { Reglages.Volume = e.newValue; Reglages.Appliquer(); });

            var commandes = Ajout(carte, new Foldout { text = "Commandes", value = false });
            var liste = Boite(commandes, "commandes-liste");
            foreach (var (touche, action) in new[]
            {
                ("Z Q S D", "se déplacer (ou W A S D, flèches)"), ("Souris", "regarder"), ("Maj", "courir, bruyant"),
                ("C", "marcher en silence"), ("Espace", "sauter"), ("E", "maintenir pour récolter, toucher"),
                ("F", "scanner, bruyant"), ("E", "près de la navette : monter à bord et piloter"), ("J", "journal de bord"),
                ("M", "couper ou remettre le son"), ("N", "didacticiel : passer l'étape"), ("Échap", "pause"),
            })
            {
                var ligne = Boite(liste, "commande");
                Texte(ligne, touche, "touche");
                Texte(ligne, action, "commande-texte");
            }
            boutonRetourOptions = Bouton(Boite(carte, "rangee"), "RETOUR", FermerOptions, "principal");
        }

        void OuvrirOptions(VisualElement depuis)
        {
            retourOptions = depuis;
            curseurSensibilite.SetValueWithoutNotify(Reglages.Sensibilite);
            caseInverser.SetValueWithoutNotify(Reglages.InverserY);
            if (QualitySettings.names.Length > 0) listeQualite.SetValueWithoutNotify(QualitySettings.names[Reglages.Qualite]);
            caseSon.SetValueWithoutNotify(Reglages.Son);
            curseurVolume.SetValueWithoutNotify(Reglages.Volume);
            Afficher(depuis, false);
            Afficher(ecranOptions, true);
            boutonRetourOptions.Focus();
        }

        void FermerOptions()
        {
            Reglages.Sauver();
            Afficher(ecranOptions, false);
            Afficher(retourOptions, true);
            (retourOptions == ecranPause ? boutonReprendre : boutonOptions).Focus();
        }

        // ------------------------------------------------------------------ crédits

        void ConstruireCredits()
        {
            ecranCredits = Boite(racine, "ecran", "centre");
            var carte = Boite(ecranCredits, "carte", "etroite");
            Texte(carte, "CRÉDITS", "surtitre");
            Texte(carte, "ÉCHOS D'ÉTHER", "titre-2");
            foreach (var (titre, texte) in new[]
            {
                ("MOTEUR", "Unity 6, High Definition Render Pipeline"),
                ("DÉCORS ET TEXTURES", "Poly Haven et ambientCG, licence CC0"),
                ("MODÈLES 3D", "Sketchfab (CC BY, CC0) et NASA 3D Resources. Auteurs à citer : fichier CREDITS-ASSETS.txt à la racine du projet."),
                ("CIEL", "Cartes d'étoiles et de la Lune : NASA's Scientific Visualization Studio"),
                ("SONS", "Kenney (kenney.nl), licence CC0"),
            })
            {
                var c = Boite(carte, "credit");
                Texte(c, titre, "credit-titre");
                Texte(c, texte, "texte");
            }
            Bouton(Boite(carte, "rangee"), "RETOUR", () => { Afficher(ecranCredits, false); Afficher(ecranMenu, true); }, "principal");
        }

        // ------------------------------------------------------------------ fin

        void ConstruireFin()
        {
            ecranFin = Boite(racine, "ecran", "centre");
            var carte = Boite(ecranFin, "carte");
            finSurtitre = Texte(carte, "", "surtitre");
            finTitre = Texte(carte, "", "titre-2");
            finTemps = Texte(carte, "", "echo-texte");
            var chiffres = Boite(carte, "chiffres");
            finVecu = Chiffre(chiffres, "TEMPS VÉCU", out _);
            finVaisseau = Chiffre(chiffres, "ÉCOULÉ LÀ-HAUT", out _);
            finEther = Chiffre(chiffres, "ÉTHER LIVRÉ", out _);
            finChiffre4 = Chiffre(chiffres, "TRANSMISSIONS", out finChiffre4Titre);
            finVerite = Texte(carte, "", "verite");
            var r = Boite(carte, "rangee");
            boutonRejouer = Bouton(r, "NOUVEAU LARGAGE", () => Jeu.LancerMission(), "principal");
            Bouton(r, "MENU PRINCIPAL", () => Jeu.MenuPrincipal());
        }

        static Label Chiffre(VisualElement parent, string titre, out Label etiquette)
        {
            var c = Boite(parent, "chiffre");
            etiquette = Texte(c, titre, "chiffre-titre");
            return Texte(c, "", "chiffre-valeur");
        }

        void RemplirFin(string surtitre, string titre, string temps, string ligne4, string valeur4, string verite, string bouton)
        {
            var h = Horloges.Instance;
            var m = Mission.Instance;
            CacherTout();
            finSurtitre.text = surtitre;
            finTitre.text = titre;
            finTemps.text = temps;
            finVecu.text = h != null ? Horloges.FormatDuree(h.TempsVecu) : "-";
            finVaisseau.text = h != null ? Horloges.FormatDuree(h.TempsVaisseau) : "-";
            finEther.text = m != null ? $"{m.Ether} / {m.quota}" : "-";
            finChiffre4Titre.text = ligne4;
            finChiffre4.text = valeur4;
            finVerite.text = verite;
            boutonRejouer.text = bouton;
            Fondu(Color.white, 0f);
            Afficher(ecranFin, true);
            boutonRejouer.Focus();
        }

        public void MontrerFinMission()
        {
            var h = Horloges.Instance;
            var p = ProfilArpenteur.Actuel;
            bool monolithe = Mission.Instance != null && Mission.Instance.MonolitheActive;
            int recues = Jeu != null && Jeu.transmissions != null ? Jeu.transmissions.Recues : 0;
            RemplirFin("EXTRACTION RÉUSSIE", "RETOUR AU VAISSEAU",
                h != null ? $"Vous avez vécu {Horloges.FormatDuree(h.TempsVecu)} sur la planète. Sur le vaisseau-mère, il s'est écoulé {Horloges.FormatDuree(h.TempsVaisseau)}." : "",
                "TRANSMISSIONS", $"{recues} / {Transmissions.NombreEmpire}",
                p.Personnaliser(monolithe
                    ? "Le Bureau du Quota félicite l'Arpenteur {nom}. Vous, vous savez maintenant ce que vous avez livré."
                    : "Le Bureau du Quota félicite l'Arpenteur {nom}. Quelque part, sous une bulle de temps lent, un monolithe attend toujours."),
                "NOUVEAU LARGAGE");
        }

        public void MontrerFinFormation(int reussies, int total)
        {
            var h = Horloges.Instance;
            RemplirFin("FORMATION TERMINÉE", $"ARPENTEUR {ProfilArpenteur.Actuel.NomAffiche.ToUpperInvariant()}, CERTIFIÉ",
                h != null ? $"Pendant votre formation, vous avez vécu {Horloges.FormatDuree(h.TempsVecu)}. Là-haut, sur le vaisseau-mère, il s'est écoulé {Horloges.FormatDuree(h.TempsVaisseau)}." : "",
                "ÉTAPES RÉUSSIES", $"{reussies} / {total}",
                "Le Ministère de l'Enthousiasme vous déclare apte. Votre première mission vous attend : cinq cristaux, une balise, et surtout aucune question.",
                "LANCER LA MISSION");
        }

        // ------------------------------------------------------------------ journal

        void ConstruireJournal()
        {
            journal = Boite(racine, "journal");
            Texte(journal, "JOURNAL DE BORD", "titre-2");
            var defilement = Ajout(journal, new ScrollView(ScrollViewMode.Vertical));
            defilement.style.flexGrow = 1;
            listeJournal = defilement.contentContainer;
            Texte(journal, "J pour fermer", "note");
            journal.pickingMode = PickingMode.Position;
        }

        public void BasculerJournal()
        {
            bool ouvrir = !Visible(journal);
            if (ouvrir) MajJournal();
            Afficher(journal, ouvrir);
        }

        void MajJournal()
        {
            if (listeJournal == null) return;
            listeJournal.Clear();
            if (Transmissions.Journal.Count == 0)
            {
                Texte(listeJournal, "Rien encore. Les transmissions et les échos s'inscrivent ici.", "note");
                return;
            }
            foreach (var e in Transmissions.Journal)
            {
                var el = Boite(listeJournal, "journal-entree", e.echo ? "echo" : "");
                Texte(el, e.titre.ToUpperInvariant(), "etiquette");
                Texte(el, e.texte, "journal-texte");
            }
        }

        // ------------------------------------------------------------------ clavier dans les menus

        void MettreAJourEcrans()
        {
            var jeu = Jeu;
            if (jeu == null || !EntreesJeu.Pause()) return;
            if (Visible(ecranOptions)) FermerOptions();
            else if (Visible(ecranCredits)) { Afficher(ecranCredits, false); Afficher(ecranMenu, true); }
            else if (jeu.Etat == EtatJeu.Enrolement) jeu.RetourMenu();
            else if (Visible(journal)) Afficher(journal, false);
        }
    }
}
