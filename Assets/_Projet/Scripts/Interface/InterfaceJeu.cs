using System;
using System.Collections.Generic;
using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchosEther.Interface
{
    /// <summary>
    /// Interface du jeu en UI Toolkit, entièrement construite par le code (aucun fichier UXML à régler) :
    /// HUD (horloges, bruit, invites, transmissions, repères, didacticiel) et écrans (voir InterfaceJeu.Ecrans.cs).
    /// Styles : Resources/EchosEtherUI.uss. Thème : Resources/EchosEtherTheme.tss.
    /// </summary>
    public partial class InterfaceJeu : MonoBehaviour
    {
        [Tooltip("Facultatif : réglages du panneau UI Toolkit. Sinon, un panneau 1920x1080 est créé.")]
        public PanelSettings panneau;

        UIDocument document;
        VisualElement racine, hud;

        // HUD
        Label tVecu, tVaisseau, ratio, vAltitude, vVitesse, bruitEtat, inviteTexte, alerteTitre, alerteSous, echo;
        Label tutoEtape, tutoTitre, tutoTexte;
        VisualElement panneauTemps, instruments, objectifs, listeObjectifs, tuto, tutoBarre, bruitPanneau, bruitBarre;
        VisualElement invite, inviteJauge, inviteBarre, alerte, transmissions, couche, fondu, impact, aide;
        readonly List<(VisualElement element, float fin)> messages = new List<(VisualElement, float)>();
        readonly List<VisualElement> marqueurs = new List<VisualElement>();
        int marqueursUtilises;
        float finEcho, finImpact;
        string cleObjectifs;
        bool modeDidacticiel;

        static GestionnaireJeu Jeu => GestionnaireJeu.Instance;

        void Awake() => Construire();

        void OnEnable()
        {
            Transmissions.MessageRecu += AjouterMessage;
            Transmissions.EchoRecu += MontrerEcho;
            Transmissions.JournalModifie += MajJournal;
            AbonnerCombat();
        }

        void OnDisable()
        {
            Transmissions.MessageRecu -= AjouterMessage;
            Transmissions.EchoRecu -= MontrerEcho;
            Transmissions.JournalModifie -= MajJournal;
            DesabonnerCombat();
        }

        // ------------------------------------------------------------------ construction

        void Construire()
        {
            var reglages = panneau != null ? panneau : CreerPanneau();
            var go = new GameObject("Interface (UI Toolkit)");
            go.SetActive(false);
            go.transform.SetParent(transform, false);
            document = go.AddComponent<UIDocument>();
            document.panelSettings = reglages;
            go.SetActive(true);

            racine = document.rootVisualElement;
            var styles = Resources.Load<StyleSheet>("EchosEtherUI");
            if (styles != null) racine.styleSheets.Add(styles);
            else Debug.LogWarning("Resources/EchosEtherUI.uss introuvable : interface sans style.");
            racine.AddToClassList("racine");
            racine.pickingMode = PickingMode.Ignore;

            ConstruireHud();
            ConstruireEcrans();
            ConstruireCombat();
            CacherTout();
        }

        static PanelSettings CreerPanneau()
        {
            var p = ScriptableObject.CreateInstance<PanelSettings>();
            p.name = "Panneau Echos d'Ether";
            p.themeStyleSheet = Resources.Load<ThemeStyleSheet>("EchosEtherTheme");
            p.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            p.referenceResolution = new Vector2Int(1920, 1080);
            p.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            p.match = 0.5f;
            p.sortingOrder = 10;
            return p;
        }

        static T Ajout<T>(VisualElement parent, T element, params string[] classes) where T : VisualElement
        {
            foreach (var c in classes) if (!string.IsNullOrEmpty(c)) element.AddToClassList(c);
            parent?.Add(element);
            return element;
        }

        static VisualElement Boite(VisualElement parent, params string[] classes) => Ajout(parent, new VisualElement(), classes);
        static Label Texte(VisualElement parent, string texte, params string[] classes) => Ajout(parent, new Label(texte), classes);
        static void Afficher(VisualElement e, bool visible) { if (e != null) e.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None; }

        Button Bouton(VisualElement parent, string texte, Action action, params string[] classes)
        {
            var b = Ajout(parent, new Button(() => { BanqueSons.Jouer(BanqueSons.Instance != null ? BanqueSons.Instance.clic : null, 0.5f); action(); }) { text = texte }, classes);
            b.AddToClassList("bouton");
            return b;
        }

        void ConstruireHud()
        {
            hud = Boite(racine, "hud");

            panneauTemps = Boite(hud, "panneau", "temps");
            var l1 = Boite(panneauTemps, "ligne");
            Texte(l1, "TEMPS VÉCU", "etiquette");
            tVecu = Texte(l1, "00:00:00", "valeur");
            var l2 = Boite(panneauTemps, "ligne");
            Texte(l2, "VAISSEAU-MÈRE", "etiquette");
            tVaisseau = Texte(l2, "J+0 · 00 h 00", "valeur", "ambre");
            ratio = Texte(panneauTemps, "", "ratio");

            instruments = Boite(hud, "panneau", "instruments");
            var a1 = Boite(instruments, "ligne");
            Texte(a1, "ALTITUDE", "etiquette");
            vAltitude = Texte(a1, "0 m", "valeur", "ether");
            var a2 = Boite(instruments, "ligne");
            Texte(a2, "VITESSE", "etiquette");
            vVitesse = Texte(a2, "0 km/h", "valeur", "ether");
            Texte(instruments, "Souris : orienter · Z/S : poussée · Q/D : latéral · Espace/C : monter/descendre · Maj : boost · E : se poser", "aide-vol");

            objectifs = Boite(hud, "panneau", "objectifs");
            Texte(objectifs, "ORDRES DE MISSION", "etiquette");
            listeObjectifs = Boite(objectifs);

            tuto = Boite(hud, "panneau", "tuto");
            var tete = Boite(tuto, "ligne");
            tutoEtape = Texte(tete, "FORMATION", "etiquette");
            Texte(tete, "N : PASSER", "etiquette");
            tutoTitre = Texte(tuto, "", "tuto-titre");
            tutoTexte = Texte(tuto, "", "tuto-texte");
            tutoTexte.enableRichText = true;
            tutoBarre = Boite(Boite(tuto, "jauge"), "jauge-barre");

            var bas = Boite(hud, "bas-centre");
            bruitPanneau = Boite(bas, "panneau", "bruit");
            var bt = Boite(bruitPanneau, "ligne");
            Texte(bt, "BRUIT", "etiquette");
            bruitEtat = Texte(bt, "SILENCIEUX", "bruit-etat");
            var jb = Boite(bruitPanneau, "jauge");
            bruitBarre = Boite(jb, "jauge-barre");
            Boite(jb, "seuil").style.left = Length.Percent(60);
            Boite(jb, "seuil").style.left = Length.Percent(85);

            invite = Boite(hud, "invite");
            inviteTexte = Texte(invite, "", "invite-texte");
            inviteJauge = Boite(invite, "invite-jauge");
            inviteBarre = Boite(inviteJauge, "invite-barre");

            alerte = Boite(hud, "alerte");
            alerteTitre = Texte(alerte, "", "alerte-titre");
            alerteSous = Texte(alerte, "", "alerte-sous");
            Afficher(alerte, false);

            transmissions = Boite(hud, "transmissions");
            echo = Texte(hud, "", "echo");
            couche = Boite(hud, "marqueurs");

            aide = Boite(hud, "aide");
            foreach (var t in new[] { "Clic : tirer", "Clic droit : viser", "1-6 : armes", $"{EntreesJeu.NomTouche("competence")} : compétence",
                         "F : scanner", $"{EntreesJeu.NomTouche("talents")} : talents", "Échap : pause" }) Texte(aide, t, "aide-item");

            impact = Boite(racine, "impact");
            fondu = Boite(racine, "fondu");

            hud.pickingMode = PickingMode.Ignore;
            hud.Query<VisualElement>().ForEach(e => e.pickingMode = PickingMode.Ignore);
            impact.pickingMode = PickingMode.Ignore;
            fondu.pickingMode = PickingMode.Ignore;
        }

        // ------------------------------------------------------------------ API du HUD

        public void ModeVol(bool vol)
        {
            Afficher(instruments, vol);
            Afficher(bruitPanneau, !vol);
        }

        public void Alerte(string titre, string sousTitre)
        {
            Afficher(alerte, !string.IsNullOrEmpty(titre));
            if (string.IsNullOrEmpty(titre)) return;
            alerteTitre.text = titre.ToUpperInvariant();
            alerteSous.text = (sousTitre ?? "").ToUpperInvariant();
        }

        public void Impact()
        {
            impact.AddToClassList("actif");
            finImpact = Time.unscaledTime + 0.15f;
        }

        /// <summary>Voile plein écran (fondu au blanc de l'extraction, fondu d'arrivée).</summary>
        public void Fondu(Color couleur, float opacite)
        {
            fondu.style.backgroundColor = couleur;
            fondu.style.opacity = Mathf.Clamp01(opacite);
        }

        public void MontrerEtape(int numero, int total, string titre, string texte)
        {
            tutoEtape.text = $"FORMATION · ÉTAPE {numero} / {total}";
            tutoTitre.text = titre.ToUpperInvariant();
            tutoTexte.text = texte;
            tuto.RemoveFromClassList("valide");
            tutoBarre.style.width = Length.Percent(0);
            Afficher(tutoBarre.parent, true);
        }

        public void EtapeValidee()
        {
            tuto.AddToClassList("valide");
            tutoBarre.style.width = Length.Percent(100);
        }

        /// <summary>Barre de l'étape (négatif : étape sans barre).</summary>
        public void ProgressionEtape(float p)
        {
            Afficher(tutoBarre.parent, p >= 0f);
            if (p >= 0f) tutoBarre.style.width = Length.Percent(p * 100f);
        }

        void AjouterMessage(Transmissions.Message m)
        {
            var el = Boite(transmissions, "transmission");
            if (m.personnel) el.AddToClassList("perso");
            Texte(el, m.titre.ToUpperInvariant(), "etiquette");
            Texte(el, m.texte, "transmission-texte");
            el.pickingMode = PickingMode.Ignore;
            el.Query<VisualElement>().ForEach(e => e.pickingMode = PickingMode.Ignore);
            messages.Add((el, Time.unscaledTime + m.duree));
            while (messages.Count > 3) { messages[0].element.RemoveFromHierarchy(); messages.RemoveAt(0); }
        }

        void MontrerEcho(string texte, float duree)
        {
            echo.text = texte;
            echo.AddToClassList("visible");
            finEcho = Time.unscaledTime + duree;
        }

        // ------------------------------------------------------------------ mise à jour

        void LateUpdate()
        {
            float maintenant = Time.unscaledTime;
            for (int i = messages.Count - 1; i >= 0; i--)
                if (maintenant > messages[i].fin) { messages[i].element.RemoveFromHierarchy(); messages.RemoveAt(i); }
            if (finEcho > 0f && maintenant > finEcho) { echo.RemoveFromClassList("visible"); finEcho = 0f; }
            if (finImpact > 0f && maintenant > finImpact) { impact.RemoveFromClassList("actif"); finImpact = 0f; }

            MettreAJourEcrans();

            var jeu = Jeu;
            marqueursUtilises = 0;
            bool enPartie = jeu != null && (jeu.Etat == EtatJeu.Jeu || jeu.Etat == EtatJeu.Vol || jeu.Etat == EtatJeu.Extraction || jeu.Etat == EtatJeu.Pause);
            if (enPartie)
            {
                MajHorloges(jeu);
                MajBruit(jeu);
                MajInvite(jeu);
                if (!modeDidacticiel) MajObjectifs();
                if (jeu.Etat == EtatJeu.Jeu || jeu.Etat == EtatJeu.Vol) MajMarqueurs(jeu);
                MajCombat(jeu);
            }
            for (int i = marqueursUtilises; i < marqueurs.Count; i++) Afficher(marqueurs[i], false);
        }

        void MajHorloges(GestionnaireJeu jeu)
        {
            var h = Horloges.Instance;
            if (h != null)
            {
                tVecu.text = Horloges.FormatHorloge(h.TempsVecu);
                tVaisseau.text = Horloges.FormatVaisseau(h.TempsVaisseau);
                ratio.text = $"1 h ici = {Horloges.FormatDuree(3600.0 / h.Facteur, true)} là-haut";
                ratio.EnableInClassList("dilate", BulleTemporelle.PresenceLente(jeu.PositionObservateur) > 0.3f);
            }
            if (jeu.NavetteEnVol != null)
            {
                vAltitude.text = $"{Mathf.RoundToInt(jeu.NavetteEnVol.Altitude)} m";
                vVitesse.text = $"{Mathf.RoundToInt(jeu.NavetteEnVol.Vitesse * 3.6f)} km/h";
            }
        }

        void MajBruit(GestionnaireJeu jeu)
        {
            float b = jeu.jauge != null ? jeu.jauge.Bruit : 0f;
            bruitBarre.style.width = Length.Percent(b);
            Color c = b > 85f ? new Color(0.85f, 0.29f, 0.23f) : b > 60f ? new Color(0.95f, 0.7f, 0.42f) : new Color(0.53f, 0.94f, 0.9f);
            bruitBarre.style.backgroundColor = c;
            bruitEtat.text = b > 85f ? "DANGER" : b > 60f ? "BRUYANT" : b > 25f ? "AUDIBLE" : "SILENCIEUX";
            bruitEtat.style.color = b > 60f ? c : new Color(0.93f, 0.9f, 0.85f);
        }

        void MajInvite(GestionnaireJeu jeu)
        {
            bool visible = (jeu.Etat == EtatJeu.Jeu || jeu.Etat == EtatJeu.Vol) && Invite.Lire(out string texte, out float progression);
            Afficher(invite, visible);
            if (!visible) return;
            Invite.Lire(out texte, out progression);
            inviteTexte.text = texte;
            Afficher(inviteJauge, progression >= 0f);
            if (progression >= 0f) inviteBarre.style.width = Length.Percent(Mathf.Clamp01(progression) * 100f);
        }

        void MajObjectifs()
        {
            var mission = Mission.Instance;
            if (mission == null) return;
            var items = new List<(string texte, string classe)>
            {
                ($"Quota impérial : cristaux d'Éther {Mathf.Min(mission.Ether, mission.quota)}/{mission.quota}", mission.QuotaAtteint ? "fait" : ""),
            };
            bool bulleVue = false;
            foreach (var b in BulleTemporelle.Toutes) if (b.EstLente && b.Decouverte) bulleVue = true;
            if (bulleVue || mission.MonolitheActive)
                items.Add((mission.MonolitheActive ? "Le monolithe a parlé" : "Hors mission : comprendre le monolithe dans la bulle", mission.MonolitheActive ? "fait" : "secret"));
            if (mission.balise != null && mission.balise.Active) items.Add(("Rejoindre la balise d'extraction", ""));

            string cle = string.Join("|", items);
            if (cle == cleObjectifs) return;
            cleObjectifs = cle;
            listeObjectifs.Clear();
            foreach (var (texte, classe) in items)
            {
                var l = Texte(listeObjectifs, texte, "objectif", classe);
                l.pickingMode = PickingMode.Ignore;
            }
        }

        // ------------------------------------------------------------------ repères à l'écran

        void MajMarqueurs(GestionnaireJeu jeu)
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 observateur = jeu.PositionObservateur;
            foreach (var c in CristalEther.Tous)
                if (!c.EstRecolte && c.Revele > 0f) Marqueur(c.transform.position + Planete.Haut(c.transform.position) * 2.5f, "cristal", cam, observateur);
            foreach (var b in BulleTemporelle.Toutes)
                if (b.Decouverte && !b.ephemere && Vector3.Distance(observateur, b.transform.position) > b.rayon + 5f) Marqueur(b.transform.position, "bulle", cam, observateur);
            var balise = Mission.Instance != null ? Mission.Instance.balise : null;
            if (balise != null && balise.Active) Marqueur(balise.transform.position + Planete.Haut(balise.transform.position) * 3f, "balise", cam, observateur);
            var cible = jeu.didacticiel != null ? jeu.didacticiel.Cible : null;
            if (cible.HasValue) Marqueur(cible.Value + Planete.Haut(cible.Value) * 3.5f, "tuto", cam, observateur);
            MarqueursCreatures(cam, observateur);
            // la navette garée, pour la retrouver de loin
            var navette = jeu.navette;
            if (navette != null && jeu.NavetteEnVol == null && navette.Etat == Navette.EtatNavette.Garee && !navette.EnManoeuvre
                && Vector3.Distance(observateur, navette.transform.position) > 40f)
                Marqueur(navette.transform.position + Planete.Haut(navette.transform.position) * 6f, "navette", cam, observateur);
        }

        static readonly string[] ClassesMarqueur = { "cristal", "bulle", "balise", "tuto", "navette", "menace" };

        void Marqueur(Vector3 monde, string classe, Camera cam, Vector3 observateur)
        {
            VisualElement m;
            if (marqueursUtilises < marqueurs.Count) m = marqueurs[marqueursUtilises];
            else
            {
                m = Boite(couche, "marqueur");
                Boite(m, "marqueur-icone");
                Texte(m, "", "marqueur-distance");
                m.pickingMode = PickingMode.Ignore;
                m.Query<VisualElement>().ForEach(e => e.pickingMode = PickingMode.Ignore);
                marqueurs.Add(m);
            }
            marqueursUtilises++;
            if (!m.ClassListContains(classe))
            {
                foreach (var c in ClassesMarqueur) m.RemoveFromClassList(c);
                m.AddToClassList(classe);
            }

            float l = couche.layout.width, h = couche.layout.height;
            if (float.IsNaN(l) || l <= 1f) { Afficher(m, false); return; }
            Vector3 vp = cam.WorldToViewportPoint(monde);
            float x = vp.x * l, y = (1f - vp.y) * h;
            if (vp.z < 0f) { x = l - x; y = h - 60f; }
            x = Mathf.Clamp(x, 48f, l - 48f);
            y = Mathf.Clamp(y, 100f, h - 140f);
            m.style.left = x;
            m.style.top = y;
            m.Q<Label>().text = $"{Mathf.RoundToInt(Vector3.Distance(monde, observateur))} m";
            Afficher(m, true);
        }
    }
}
