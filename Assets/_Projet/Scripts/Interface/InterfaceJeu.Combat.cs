using System.Collections.Generic;
using EchosEther.Combat;
using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchosEther.Interface
{
    /// <summary>
    /// Interface du combat et des compétences : réticule (s'ouvre avec la dispersion), marque de touche,
    /// arme et munitions, emplacements 1 à 6, intégrité et bouclier, compétence et sa recharge, niveau et
    /// expérience, flèche vers la source des dégâts, repères des créatures, écran des talents (K),
    /// choix de la compétence et du talent de départ à l'enrôlement.
    /// </summary>
    public partial class InterfaceJeu
    {
        VisualElement reticule, reticuleH, reticuleB, reticuleG, reticuleD, marqueTouche, chargeFond, chargeBarre;
        VisualElement panneauArme, rechargeBarre, emplacements, panneauVie, vieBarre, bouclierBarre, panneauCompetence, competenceBarre;
        VisualElement niveauBloc, niveauBarre, flecheDegats, gainsXp;
        Label badgeTalent, armeNom, armeMunitions, armeReserve, vieValeur, competenceNom, competenceTouche, niveauTexte;
        readonly List<Label> emplacementsLabels = new List<Label>();
        float finTouche, finFleche;
        bool toucheMortelle;
        Vector3 sourceDegats;
        readonly List<(Label label, float fin)> gains = new List<(Label, float)>();

        // talents
        VisualElement ecranTalents, listePropositions, listeAcquis, listeCompetences;
        Label talentsResume, talentsCompetenceInfo;
        bool talentsDepuisJeu;
        readonly List<(Button bouton, CompetenceActive c)> boutonsCompetence = new List<(Button, CompetenceActive)>();

        // enrôlement
        readonly List<(Button bouton, CompetenceActive c)> boutonsCompetenceEnrolement = new List<(Button, CompetenceActive)>();
        readonly List<(Button bouton, Talent t)> boutonsTalentDepart = new List<(Button, Talent)>();
        Label enrolementCompetence, enrolementTalent;

        static int tuees;
        /// <summary>Créatures neutralisées par l'Arpenteur depuis le début de la partie.</summary>
        public static int Tuees => tuees;

        public bool TalentsOuverts => Visible(ecranTalents);

        void AbonnerCombat()
        {
            Combat.Combat.Touche += SurTouche;
            VieArpenteur.Blesse += SurBlesse;
            Echarde.Morte += SurMort;
            CompetencesArpenteur.ExperienceGagnee += SurExperience;
        }

        void DesabonnerCombat()
        {
            Combat.Combat.Touche -= SurTouche;
            VieArpenteur.Blesse -= SurBlesse;
            Echarde.Morte -= SurMort;
            CompetencesArpenteur.ExperienceGagnee -= SurExperience;
        }

        void SurTouche(bool mort) { finTouche = Time.unscaledTime + (mort ? 0.35f : 0.12f); toucheMortelle = mort; }
        void SurBlesse(float quantite, Vector3 source) { sourceDegats = source; finFleche = Time.unscaledTime + 1.2f; }
        void SurMort(Echarde e, bool parJoueur) { if (parJoueur) tuees++; }

        void SurExperience(int quantite, string raison)
        {
            if (gainsXp == null) return;
            var l = Texte(gainsXp, $"+{quantite} · {raison}", "gain-xp");
            l.pickingMode = PickingMode.Ignore;
            gains.Add((l, Time.unscaledTime + 2.2f));
            while (gains.Count > 4) { gains[0].label.RemoveFromHierarchy(); gains.RemoveAt(0); }
        }

        // ------------------------------------------------------------------ construction du HUD de combat

        void ConstruireCombat()
        {
            tuees = 0;
            reticule = Boite(hud, "reticule");
            Boite(reticule, "reticule-point");
            reticuleH = Boite(reticule, "reticule-trait", "vertical");
            reticuleB = Boite(reticule, "reticule-trait", "vertical");
            reticuleG = Boite(reticule, "reticule-trait", "horizontal");
            reticuleD = Boite(reticule, "reticule-trait", "horizontal");
            marqueTouche = Boite(reticule, "marque-touche");
            foreach (float a in new[] { 45f, 135f, 225f, 315f })
            {
                var trait = Boite(marqueTouche, "marque-trait");
                trait.style.rotate = new Rotate(new Angle(a + 90f, AngleUnit.Degree));
                trait.style.left = Mathf.Cos(a * Mathf.Deg2Rad) * 14f - 1.5f;
                trait.style.top = Mathf.Sin(a * Mathf.Deg2Rad) * 14f - 6f;
            }
            chargeFond = Boite(reticule, "charge-fond");
            chargeBarre = Boite(chargeFond, "charge-barre");

            flecheDegats = Boite(hud, "fleche-degats");
            Boite(flecheDegats, "fleche-pointe");
            gainsXp = Boite(hud, "gains-xp");

            panneauArme = Boite(hud, "panneau", "arme");
            var ligne = Boite(panneauArme, "ligne");
            armeNom = Texte(ligne, "", "arme-nom");
            var munitions = Boite(ligne, "munitions");
            armeMunitions = Texte(munitions, "", "arme-munitions");
            armeReserve = Texte(munitions, "", "arme-reserve");
            rechargeBarre = Boite(Boite(panneauArme, "jauge"), "jauge-barre");
            emplacements = Boite(panneauArme, "emplacements");
            for (int i = 0; i < Catalogue.Armes.Length; i++)
                emplacementsLabels.Add(Texte(emplacements, $"{i + 1} {Catalogue.Armes[i].surnom}", "emplacement"));

            panneauCompetence = Boite(hud, "panneau", "competence");
            var lc = Boite(panneauCompetence, "ligne");
            competenceNom = Texte(lc, "", "competence-nom");
            competenceTouche = Texte(lc, "", "touche");
            competenceBarre = Boite(Boite(panneauCompetence, "jauge"), "jauge-barre");

            panneauVie = Boite(hud, "panneau", "vie");
            var lv = Boite(panneauVie, "ligne");
            Texte(lv, "COMBINAISON", "etiquette");
            vieValeur = Texte(lv, "100", "vie-valeur");
            var jv = Boite(panneauVie, "jauge", "jauge-vie");
            vieBarre = Boite(jv, "jauge-barre");
            bouclierBarre = Boite(jv, "bouclier-barre");

            niveauBloc = Boite(hud, "niveau");
            niveauTexte = Texte(niveauBloc, "", "niveau-texte");
            niveauBarre = Boite(Boite(niveauBloc, "jauge", "jauge-xp"), "jauge-barre");
            badgeTalent = Texte(niveauBloc, "", "badge-talent");

            foreach (var e in new[] { reticule, flecheDegats, gainsXp, panneauArme, panneauCompetence, panneauVie, niveauBloc })
            {
                e.pickingMode = PickingMode.Ignore;
                e.Query<VisualElement>().ForEach(x => x.pickingMode = PickingMode.Ignore);
            }
            ConstruireTalents();
        }

        // ------------------------------------------------------------------ mise à jour

        void MajCombat(GestionnaireJeu jeu)
        {
            float maintenant = Time.unscaledTime;
            for (int i = gains.Count - 1; i >= 0; i--)
                if (maintenant > gains[i].fin) { gains[i].label.RemoveFromHierarchy(); gains.RemoveAt(i); }

            bool aPied = jeu.Etat == EtatJeu.Jeu || (jeu.Etat == EtatJeu.Pause && jeu.NavetteEnVol == null);
            var arsenal = Arsenal.Instance;
            var vie = jeu.joueur != null ? jeu.joueur.GetComponent<VieArpenteur>() : null;
            var comp = CompetencesArpenteur.Instance;
            Afficher(reticule, aPied && arsenal != null && jeu.Etat == EtatJeu.Jeu);
            Afficher(panneauArme, aPied && arsenal != null);
            Afficher(panneauVie, aPied && vie != null);
            Afficher(panneauCompetence, aPied && comp != null);
            Afficher(niveauBloc, jeu.Etat != EtatJeu.Extraction);

            if (arsenal != null && aPied)
            {
                var d = arsenal.Arme;
                float ouverture = 6f + Mathf.Tan(arsenal.Dispersion * Mathf.Deg2Rad) * 900f;
                ouverture = Mathf.Min(ouverture, 70f);
                reticuleH.style.top = -ouverture - 10f; reticuleH.style.left = -1f;
                reticuleB.style.top = ouverture; reticuleB.style.left = -1f;
                reticuleG.style.left = -ouverture - 10f; reticuleG.style.top = -1f;
                reticuleD.style.left = ouverture; reticuleD.style.top = -1f;
                reticule.EnableInClassList("melee", d.mode == ModeTir.Melee);
                reticule.EnableInClassList("vise", arsenal.Vise);
                bool touche = maintenant < finTouche;
                Afficher(marqueTouche, touche);
                marqueTouche.EnableInClassList("mortelle", touche && toucheMortelle);
                Afficher(chargeFond, d.mode == ModeTir.Charge && arsenal.Charge > 0f);
                chargeBarre.style.width = Length.Percent(arsenal.Charge * 100f);

                armeNom.text = d.surnom;
                if (d.mode == ModeTir.Melee) { armeMunitions.text = ""; armeReserve.text = "corps à corps"; }
                else if (d.mode == ModeTir.Lancer) { armeMunitions.text = arsenal.Grenades.ToString(); armeReserve.text = " grenades"; }
                else { armeMunitions.text = arsenal.Chargeur.ToString(); armeReserve.text = " / " + arsenal.Reserve; }
                armeMunitions.EnableInClassList("vide", d.mode != ModeTir.Melee && arsenal.Chargeur == 0 && d.mode != ModeTir.Lancer);
                Afficher(rechargeBarre.parent, arsenal.EnRechargement);
                rechargeBarre.style.width = Length.Percent(arsenal.ProgressionRechargement * 100f);
                for (int i = 0; i < emplacementsLabels.Count; i++)
                {
                    emplacementsLabels[i].EnableInClassList("actif", i == arsenal.Index);
                    if (Catalogue.Armes[i].mode == ModeTir.Lancer) emplacementsLabels[i].text = $"{i + 1} {Catalogue.Armes[i].surnom} ×{arsenal.Grenades}";
                }
            }

            if (vie != null && aPied)
            {
                float max = Mathf.Max(1f, vie.IntegriteMax);
                vieBarre.style.width = Length.Percent(vie.Integrite / max * 100f);
                bouclierBarre.style.width = Length.Percent(vie.BouclierMax > 0f ? vie.Bouclier / vie.BouclierMax * 100f : 0f);
                vieValeur.text = Mathf.CeilToInt(vie.Integrite).ToString();
                panneauVie.EnableInClassList("critique", vie.Integrite < max * 0.3f);
            }

            if (comp != null && aPied)
            {
                var info = comp.Info;
                competenceNom.text = info.nom.ToUpperInvariant();
                competenceTouche.text = EntreesJeu.NomTouche("competence");
                float attente = comp.Attente;
                bool effet = comp.Effet > 0f;
                competenceBarre.style.width = Length.Percent(effet ? comp.Effet * 100f : (1f - attente / Mathf.Max(0.01f, comp.AttenteMax)) * 100f);
                panneauCompetence.EnableInClassList("prete", attente <= 0f);
                panneauCompetence.EnableInClassList("effet", effet);
            }

            var p = ProfilArpenteur.Actuel;
            niveauTexte.text = $"NIVEAU {p.niveau}";
            niveauBarre.style.width = Length.Percent(p.ProgressionNiveau * 100f);
            Afficher(badgeTalent, p.pointsTalent > 0);
            if (p.pointsTalent > 0)
            {
                badgeTalent.text = $"{p.pointsTalent} TALENT{(p.pointsTalent > 1 ? "S" : "")} À CHOISIR · {EntreesJeu.NomTouche("talents")}";
                badgeTalent.EnableInClassList("clignote", Mathf.Repeat(maintenant, 1.2f) < 0.6f);
            }

            // flèche vers la source des dégâts, autour du réticule
            bool fleche = maintenant < finFleche && Camera.main != null && jeu.joueur != null;
            Afficher(flecheDegats, fleche);
            if (fleche)
            {
                var cam = Camera.main.transform;
                Vector3 haut = Planete.Haut(jeu.joueur.transform.position);
                Vector3 vers = Vector3.ProjectOnPlane(sourceDegats - jeu.joueur.transform.position, haut);
                float angle = Vector3.SignedAngle(Vector3.ProjectOnPlane(cam.forward, haut), vers, haut);
                flecheDegats.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
                flecheDegats.style.opacity = Mathf.Clamp01((finFleche - maintenant) / 0.6f);
            }
        }

        /// <summary>Repères des créatures révélées (scanner avec Écho profond, embuscade) ou qui chassent hors champ.</summary>
        void MarqueursCreatures(Camera cam, Vector3 observateur)
        {
            foreach (var e in Echarde.Toutes)
            {
                if (!e.Vivante) continue;
                float d = Vector3.Distance(e.Centre, observateur);
                Vector3 vp = cam.WorldToViewportPoint(e.Centre);
                bool horsChamp = vp.z < 0f || vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f;
                if (e.Revele > 0f || (e.EnChasse && d < 35f && horsChamp))
                    Marqueur(e.Centre + Planete.Haut(e.Centre) * 1.5f, "menace", cam, observateur);
            }
        }

        // ------------------------------------------------------------------ talents (K, ou menu pause)

        void ConstruireTalents()
        {
            ecranTalents = Boite(racine, "ecran", "centre");
            var defilement = Ajout(ecranTalents, new ScrollView(ScrollViewMode.Vertical), "defilement");
            var carte = Boite(defilement, "carte", "talents");
            Texte(carte, "DOSSIER DE L'ARPENTEUR", "surtitre");
            Texte(carte, "COMPÉTENCES ET TALENTS", "titre-2");
            talentsResume = Texte(carte, "", "note");

            Texte(carte, "COMPÉTENCE ÉQUIPÉE", "etiquette");
            listeCompetences = Boite(carte, "rangee", "choix-competences");
            foreach (var c in Competences.Actives)
            {
                var cc = c;
                var b = Bouton(listeCompetences, Competences.Info(c).nom.ToUpperInvariant(), () => { ProfilArpenteur.Actuel.competence = cc; ProfilArpenteur.Actuel.Sauver(); MajTalents(); }, "choix");
                boutonsCompetence.Add((b, c));
            }
            talentsCompetenceInfo = Texte(carte, "", "note");

            Texte(carte, "NOUVEAU TALENT", "etiquette");
            listePropositions = Boite(carte, "propositions");
            Texte(carte, "TALENTS ACQUIS", "etiquette");
            listeAcquis = Boite(carte, "acquis");
            Bouton(Boite(carte, "rangee"), "FERMER", FermerTalents, "principal");
        }

        /// <summary>Ouvre l'écran des talents (depuis le jeu : la partie est en pause pendant le choix).</summary>
        public void MontrerTalents(bool depuisJeu)
        {
            talentsDepuisJeu = depuisJeu;
            Afficher(ecranPause, false);
            Afficher(journal, false);
            MajTalents();
            Afficher(ecranTalents, true);
        }

        public void FermerTalents()
        {
            Afficher(ecranTalents, false);
            if (talentsDepuisJeu) Jeu?.Reprendre();
            else Afficher(ecranPause, true);
        }

        void MajTalents()
        {
            var p = ProfilArpenteur.Actuel;
            talentsResume.text = $"Arpenteur {p.NomAffiche} · niveau {p.niveau} · {p.experience} points d'expérience · " +
                (p.pointsTalent > 0 ? $"{p.pointsTalent} talent{(p.pointsTalent > 1 ? "s" : "")} à choisir" : "prochain talent au niveau suivant");
            foreach (var (b, c) in boutonsCompetence) b.EnableInClassList("choisi", c == p.competence);
            var info = Competences.Info(p.competence);
            talentsCompetenceInfo.text = $"{info.description} Recharge : {Mathf.RoundToInt(info.recharge)} s.";

            listePropositions.Clear();
            if (p.pointsTalent > 0)
            {
                foreach (var t in Competences.Propositions(p))
                {
                    var tt = t;
                    var it = Competences.Info(t);
                    var b = Bouton(listePropositions, "", () => { if (ProfilArpenteur.Actuel.Choisir(tt)) { BanqueSons.Jouer(BanqueSons.Instance?.niveau, 0.6f); MajTalents(); } }, "proposition");
                    Texte(b, it.nom.ToUpperInvariant(), "spec-nom").pickingMode = PickingMode.Ignore;
                    Texte(b, it.description, "spec-atout").pickingMode = PickingMode.Ignore;
                }
            }
            else Texte(listePropositions, "Gagnez de l'expérience (cristaux, créatures, esquives, monolithe) pour obtenir un nouveau talent à chaque niveau.", "note");

            listeAcquis.Clear();
            foreach (var t in p.talents)
            {
                var it = Competences.Info(t);
                var l = Texte(listeAcquis, $"{it.nom} : {it.description}", "acquis-ligne");
                l.pickingMode = PickingMode.Ignore;
            }
        }

        // ------------------------------------------------------------------ enrôlement : compétence et talent de départ

        void ConstruireChoixEnrolement(VisualElement carte)
        {
            Texte(carte, "COMPÉTENCE ACTIVE", "etiquette");
            var rangee = Boite(carte, "rangee", "choix-competences");
            foreach (var c in Competences.Actives)
            {
                var cc = c;
                var b = Bouton(rangee, Competences.Info(c).nom.ToUpperInvariant(), () => { ProfilArpenteur.Actuel.competence = cc; MajChoix(); }, "choix");
                boutonsCompetenceEnrolement.Add((b, c));
            }
            enrolementCompetence = Texte(carte, "", "note");

            Texte(carte, "TALENT DE DÉPART", "etiquette");
            var talents = Boite(carte, "rangee", "choix-talents");
            foreach (var t in Competences.Talents)
            {
                var tt = t;
                var b = Bouton(talents, Competences.Info(t).nom, () => { ProfilArpenteur.Actuel.TalentDeDepart(tt); MajChoix(); }, "choix", "petit");
                boutonsTalentDepart.Add((b, t));
            }
            enrolementTalent = Texte(carte, "", "note");
        }

        void MajChoixEnrolement()
        {
            var p = ProfilArpenteur.Actuel;
            foreach (var (b, c) in boutonsCompetenceEnrolement) b.EnableInClassList("choisi", c == p.competence);
            var info = Competences.Info(p.competence);
            if (enrolementCompetence != null) enrolementCompetence.text = $"{info.description} Recharge : {Mathf.RoundToInt(info.recharge)} s. Touche {EntreesJeu.NomTouche("competence")}.";
            Talent depart = p.talents.Count > 0 ? p.talents[0] : ProfilArpenteur.DepartConseille(p.specialite);
            foreach (var (b, t) in boutonsTalentDepart) b.EnableInClassList("choisi", t == depart);
            if (enrolementTalent != null)
                enrolementTalent.text = $"{Competences.Info(depart).description}" + (p.talents.Count > 1 ? $" Talents gagnés depuis : {p.talents.Count - 1}, niveau {p.niveau}." : "");
        }
    }
}
