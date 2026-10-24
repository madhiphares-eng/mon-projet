#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EchosEther.Combat;
using EchosEther.Interface;
using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Outils
{
    /// <summary>
    /// Test automatique (éditeur seulement), lancé par le menu "Echos d'Ether > Test automatique" :
    /// joue tout seul le menu, l'enrôlement, l'arrivée en navette, les 6 armes, les 3 créatures,
    /// les 6 compétences, les dégâts et le rapatriement, les talents, la pause, la navette et l'extraction.
    /// Note les erreurs de la Console, les images par seconde, prend des captures (docs/captures)
    /// et écrit docs/RAPPORT-TEST.md (aussi copié dans le presse-papiers). Le profil du joueur est restauré à la fin.
    /// </summary>
    public class TestAutomatique : MonoBehaviour
    {
        public const string Cle = "EchosEther.TestAuto";
        const string CleProfil = "EchosEther.Profil";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Amorcer()
        {
            if (!UnityEditor.SessionState.GetBool(Cle, false)) return;
            if (FindFirstObjectByType<TestAutomatique>() != null) return;
            new GameObject("Test automatique").AddComponent<TestAutomatique>();
        }

        enum Resultat { Ok, Attention, Echec }

        readonly List<(Resultat r, string etape, string detail)> resultats = new List<(Resultat, string, string)>();
        readonly List<string> journal = new List<string>();
        readonly Dictionary<string, int> messages = new Dictionary<string, int>();
        readonly List<string> captures = new List<string>();
        int erreurs, avertissements;
        float tempsTotal, pireSeconde = 999f, fenetre;
        int images, imagesFenetre;
        string profilSauve;
        GestionnaireJeu jeu;
        string dossierDocs;

        void Awake()
        {
            Application.logMessageReceived += Console;
            dossierDocs = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs"));
        }

        void OnDestroy() => Application.logMessageReceived -= Console;

        void Console(string message, string pile, LogType type)
        {
            if (message.StartsWith("[Test]")) return;
            if (type == LogType.Log) return;
            if (type == LogType.Warning) avertissements++; else erreurs++;
            string premiereLigne = string.IsNullOrEmpty(pile) ? "" : pile.Split('\n')[0].Trim();
            string cle = $"[{type}] {message.Split('\n')[0]}" + (premiereLigne.Length > 0 ? $"  ({premiereLigne})" : "");
            messages[cle] = messages.TryGetValue(cle, out int n) ? n + 1 : 1;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            tempsTotal += dt;
            images++;
            fenetre += dt;
            imagesFenetre++;
            if (fenetre >= 0.5f)
            {
                if (tempsTotal > 3f) pireSeconde = Mathf.Min(pireSeconde, imagesFenetre / fenetre);
                fenetre = 0f;
                imagesFenetre = 0;
            }
        }

        IEnumerator Start()
        {
            profilSauve = PlayerPrefs.GetString(CleProfil, "");
            Debug.Log("[Test] Test automatique d'Échos d'Éther : début.");
            yield return Etape("Démarrage", Demarrage());
            if (jeu != null)
            {
                yield return Etape("Menu principal", MenuPrincipal());
                yield return Etape("Enrôlement", Enrolement());
                yield return Etape("Arrivée en navette", Arrivee());
                yield return Etape("Arpenteur au sol et animation", AuSol());
                yield return Etape("Armes", Armes());
                yield return Etape("Créatures", Creatures());
                yield return Etape("Compétences", CompetencesActives());
                yield return Etape("Intégrité et rapatriement", Integrite());
                yield return Etape("Talents et niveaux", Talents());
                yield return Etape("Pause et journal", PauseJournal());
                yield return Etape("Navette et extraction", NavetteExtraction());
            }
            Terminer();
        }

        /// <summary>Exécute une étape ; une exception l'arrête et devient un échec, sans arrêter le test.</summary>
        IEnumerator Etape(string nom, IEnumerator corps)
        {
            Debug.Log("[Test] " + nom);
            while (true)
            {
                object courant;
                try
                {
                    if (!corps.MoveNext()) break;
                    courant = corps.Current;
                }
                catch (Exception e)
                {
                    Noter(Resultat.Echec, nom, $"exception : {e.GetType().Name} : {e.Message}");
                    yield break;
                }
                yield return courant;
            }
        }

        void Noter(Resultat r, string etape, string detail) => resultats.Add((r, etape, detail));
        void Ok(string etape, string detail) => Noter(Resultat.Ok, etape, detail);
        void Attention(string etape, string detail) => Noter(Resultat.Attention, etape, detail);
        void Echec(string etape, string detail) => Noter(Resultat.Echec, etape, detail);
        void Verifier(bool condition, string etape, string ok, string echec) { if (condition) Ok(etape, ok); else Echec(etape, echec); }

        static WaitForSecondsRealtime Attendre(float s) => new WaitForSecondsRealtime(s);

        IEnumerator AttendreQue(Func<bool> condition, float delai)
        {
            float fin = Time.realtimeSinceStartup + delai;
            while (!condition() && Time.realtimeSinceStartup < fin) yield return null;
        }

        void Capturer(string nom)
        {
            try
            {
                string dossier = Path.Combine(dossierDocs, "captures");
                Directory.CreateDirectory(dossier);
                string fichier = $"test-{captures.Count + 1:00}-{nom}.png";
                ScreenCapture.CaptureScreenshot(Path.Combine(dossier, fichier));
                captures.Add(fichier);
            }
            catch (Exception e) { Debug.LogWarning("[Test] capture impossible : " + e.Message); }
        }

        Vector3 Devant(float distance)
        {
            var j = jeu.joueur.transform;
            Vector3 haut = Planete.Haut(j.position);
            Vector3 avant = Camera.main != null ? Vector3.ProjectOnPlane(Camera.main.transform.forward, haut).normalized : j.forward;
            if (avant.sqrMagnitude < 0.01f) avant = Vector3.ProjectOnPlane(j.forward, haut).normalized;
            return j.position + avant * distance;
        }

        // ------------------------------------------------------------------ étapes

        IEnumerator Demarrage()
        {
            yield return AttendreQue(() => GestionnaireJeu.Instance != null, 5f);
            jeu = GestionnaireJeu.Instance;
            if (jeu == null) { Echec("Démarrage", "pas de GestionnaireJeu : menu Echos d'Ether > Installer le jeu dans la scène"); yield break; }
            var manquants = new List<string>();
            if (jeu.joueur == null) manquants.Add("Arpenteur");
            if (jeu.interfaceJeu == null) manquants.Add("interface");
            if (jeu.navette == null) manquants.Add("navette");
            if (jeu.sequenceLargage == null) manquants.Add("arrivée");
            if (jeu.joueur != null)
            {
                if (jeu.joueur.GetComponent<Arsenal>() == null) manquants.Add("armes");
                if (jeu.joueur.GetComponent<VieArpenteur>() == null) manquants.Add("intégrité");
                if (jeu.joueur.GetComponent<CompetencesArpenteur>() == null) manquants.Add("compétences");
                if (jeu.joueur.GetComponent<AnimateurArpenteur>() == null) manquants.Add("animation");
            }
            if (manquants.Count == 0) Ok("Démarrage", "tous les systèmes sont présents");
            else Attention("Démarrage", "absents : " + string.Join(", ", manquants) + " (relancer Installer le jeu dans la scène)");
            if (Shader.Find("EchosEther/Lueur") == null) Echec("Démarrage", "shader EchosEther/Lueur introuvable");
        }

        IEnumerator MenuPrincipal()
        {
            yield return AttendreQue(() => jeu.Etat == EtatJeu.Menu, 5f);
            yield return Attendre(1.5f);
            Capturer("menu");
            Verifier(jeu.Etat == EtatJeu.Menu, "Menu principal", "affiché", $"état inattendu : {jeu.Etat}");
        }

        IEnumerator Enrolement()
        {
            jeu.OuvrirEnrolement(TypePartie.Didacticiel);
            yield return Attendre(1.5f);
            Capturer("enrolement");
            Verifier(jeu.Etat == EtatJeu.Enrolement, "Enrôlement", "écran et aperçu de l'Arpenteur", $"état inattendu : {jeu.Etat}");
        }

        IEnumerator Arrivee()
        {
            jeu.Lancer(TypePartie.Didacticiel);
            yield return Attendre(3f);
            if (jeu.Etat == EtatJeu.Largage)
            {
                Capturer("arrivee-navette");
                Ok("Arrivée en navette", "la cinématique a démarré");
                jeu.sequenceLargage.Passer();
            }
            else Attention("Arrivée en navette", $"pas de cinématique (état {jeu.Etat}) : navette ou Largage absent ?");
            yield return AttendreQue(() => jeu.Etat == EtatJeu.Jeu, 10f);
            Verifier(jeu.Etat == EtatJeu.Jeu, "Arrivée en navette", "l'Arpenteur a la main", $"la partie n'a pas commencé (état {jeu.Etat})");
        }

        IEnumerator AuSol()
        {
            yield return AttendreQue(() => jeu.etatJoueur != null && jeu.etatJoueur.AuSol, 6f);
            var p = jeu.joueur.transform.position;
            if (Planete.Sol(p, out RaycastHit sol)) Verifier(Vector3.Distance(p, sol.point) < 3f, "Arpenteur au sol et animation",
                $"au sol ({Vector3.Distance(p, sol.point):0.00} m entre le centre et le sol)", $"flotte ou s'enfonce : {Vector3.Distance(p, sol.point):0.0} m du sol");
            else Echec("Arpenteur au sol et animation", "aucun sol sous l'Arpenteur");
            var anim = jeu.joueur.GetComponent<AnimateurArpenteur>();
            if (anim != null)
            {
                string mode = anim.Mode;
                if (mode == "clips" || mode == "os") Ok("Arpenteur au sol et animation", $"animation : {mode}");
                else Attention("Arpenteur au sol et animation", $"animation : {mode} (pas de modèle à squelette sous l'Arpenteur)");
            }
            // petite course pour voir les foulées
            jeu.joueur.Propulser(Vector3.ProjectOnPlane(Devant(1f) - p, Planete.Haut(p)).normalized * 7f);
            yield return Attendre(0.4f);
            Capturer("course");
            yield return Attendre(1f);
        }

        IEnumerator Armes()
        {
            var arsenal = jeu.joueur.GetComponent<Arsenal>();
            if (arsenal == null) { Echec("Armes", "pas d'arsenal sur l'Arpenteur"); yield break; }
            var vue = jeu.joueur.GetComponent<VueArme>();
            int modeles = 0;
            if (arsenal.modeles != null) foreach (var m in arsenal.modeles) if (m != null) modeles++;
            if (modeles > 0) Ok("Armes", $"{modeles} modèles Kenney branchés");
            else Attention("Armes", "formes générées (Blaster Kit absent)");

            for (int i = 0; i < Catalogue.Armes.Length; i++)
            {
                var d = Catalogue.Armes[i];
                arsenal.Changer(i);
                yield return Attendre(0.7f);
                if (arsenal.Index != i) { Echec("Armes", $"{d.surnom} : changement d'arme sans effet"); continue; }
                if (vue == null || !vue.Visible) Attention("Armes", $"{d.surnom} : arme invisible");
                int avant = arsenal.Chargeur + arsenal.Reserve;
                int coups = d.mode == ModeTir.Lancer || d.mode == ModeTir.Charge ? 1 : 3;
                for (int c = 0; c < coups; c++)
                {
                    arsenal.TirerPourTest();
                    yield return Attendre(d.mode == ModeTir.Melee ? 0.7f : 0.35f);
                }
                if (i == 1 || i == 3 || i == 5) Capturer("arme-" + d.id);
                if (d.mode == ModeTir.Lancer) { yield return Attendre(2.6f); Capturer("explosion-grenade"); }
                int apres = arsenal.Chargeur + arsenal.Reserve;
                if (d.mode == ModeTir.Melee) Ok("Armes", $"{d.surnom} : coups portés");
                else Verifier(apres < avant || d.mode == ModeTir.Lancer && arsenal.Grenades < avant, "Armes", $"{d.surnom} : {avant - apres} munition(s) utilisée(s)", $"{d.surnom} : aucun tir n'est parti");
                Vector3 bouche = vue != null ? vue.Bouche : Vector3.zero;
                if (float.IsNaN(bouche.x)) Echec("Armes", $"{d.surnom} : position de la bouche invalide");
            }
            // rechargement
            arsenal.Changer(1);
            yield return Attendre(0.7f);
            arsenal.TirerPourTest();
            yield return Attendre(0.4f);
            arsenal.Recharger();
            yield return Attendre(Catalogue.Armes[1].rechargement + 0.5f);
            Verifier(arsenal.Chargeur == arsenal.ChargeurMax(arsenal.Arme), "Armes", "rechargement complet", $"rechargement incomplet ({arsenal.Chargeur}/{arsenal.ChargeurMax(arsenal.Arme)})");
        }

        IEnumerator Creatures()
        {
            var types = new[] { TypeEcharde.Rodeuse, TypeEcharde.Cracheuse, TypeEcharde.Colosse };
            var creees = new List<Echarde>();
            for (int i = 0; i < types.Length; i++)
            {
                Vector3 p = Devant(11f + i * 5f);
                Vector3 haut = Planete.Haut(p);
                Vector3 cote = Vector3.Cross(haut, (p - jeu.joueur.transform.position).normalized);
                creees.Add(Essaim.Faire(types[i], p + cote * (i - 1) * 5f, p));
            }
            yield return Attendre(3f);
            Capturer("creatures");
            foreach (var e in creees)
            {
                if (e == null) { Echec("Créatures", "créature détruite trop tôt"); continue; }
                Vector3 c = e.Centre;
                if (float.IsNaN(c.x)) { Echec("Créatures", $"{e.NomType} : position invalide"); continue; }
                if (Planete.Sol(c + Planete.Haut(c) * 3f, out RaycastHit sol))
                {
                    float h = Vector3.Distance(c, sol.point);
                    if (h > 0.15f && h < 5f) Ok("Créatures", $"{e.NomType} : debout à {h:0.0} m du sol");
                    else Attention("Créatures", $"{e.NomType} : corps à {h:0.0} m du sol (enfoncé ou flottant)");
                }
            }
            yield return Attendre(3f);
            Capturer("creatures-chasse");
            int vivantes = 0;
            foreach (var e in creees) if (e != null && e.Vivante) vivantes++;
            int mortesAvant = 0;
            Action<Echarde, bool> compter = (x, y) => mortesAvant++;
            Echarde.Morte += compter;
            foreach (var e in creees)
                if (e != null && e.Vivante) Combat.Combat.Frapper(e, new Degats(9999f, e.Centre, Vector3.up, 0f, true));
            yield return Attendre(1.2f);
            Echarde.Morte -= compter;
            Verifier(mortesAvant == vivantes, "Créatures", $"{mortesAvant} créature(s) abattue(s), éclats et cellules", $"{vivantes - mortesAvant} créature(s) n'ont pas pu être abattues");
            int cellules = FindObjectsByType<CelluleEnergie>(FindObjectsSortMode.None).Length;
            Ok("Créatures", $"{cellules} cellule(s) d'énergie au sol");
        }

        IEnumerator CompetencesActives()
        {
            var comp = jeu.joueur.GetComponent<CompetencesArpenteur>();
            if (comp == null) { Echec("Compétences", "pas de composant CompetencesArpenteur"); yield break; }
            var profil = ProfilArpenteur.Actuel;
            var avant = profil.competence;
            foreach (var c in Competences.Actives)
            {
                profil.competence = c;
                comp.Activer();
                yield return Attendre(c == CompetenceActive.Leurre ? 1.5f : 0.9f);
                if (c == CompetenceActive.Stase || c == CompetenceActive.Bouclier || c == CompetenceActive.Leurre) Capturer("competence-" + c.ToString().ToLowerInvariant());
                Ok("Compétences", Competences.Info(c).nom);
            }
            profil.competence = avant;
            yield return Attendre(4f);   // le leurre crie, la bulle de stase s'éteint
        }

        IEnumerator Integrite()
        {
            var vie = jeu.joueur.GetComponent<VieArpenteur>();
            if (vie == null) { Echec("Intégrité et rapatriement", "pas de VieArpenteur"); yield break; }
            vie.Reinitialiser();
            vie.ActiverBouclier(0f, 0f);
            float max = vie.IntegriteMax;
            vie.Subir(new Degats(30f, jeu.joueur.transform.position, Vector3.forward, 2f, false));
            yield return Attendre(0.3f);
            Verifier(Mathf.Abs(vie.Integrite - (max - 30f)) < 2f, "Intégrité et rapatriement", $"dégâts encaissés ({vie.Integrite:0}/{max:0})", $"intégrité inattendue : {vie.Integrite:0}/{max:0}");
            Capturer("degats");
            vie.Subir(new Degats(9999f, jeu.joueur.transform.position, Vector3.forward, 0f, false));
            Verifier(vie.EnRapatriement, "Intégrité et rapatriement", "effondrement à zéro", "pas de rapatriement à zéro");
            yield return AttendreQue(() => !vie.EnRapatriement, 6f);
            yield return Attendre(0.5f);
            float dNavette = jeu.navette != null ? Vector3.Distance(jeu.joueur.transform.position, jeu.navette.transform.position) : 0f;
            Verifier(vie.Vivante && vie.Integrite > max * 0.5f, "Intégrité et rapatriement", $"rapatrié à {dNavette:0} m de la navette, intégrité {vie.Integrite:0}", "l'Arpenteur n'est pas revenu");
            vie.Reinitialiser();
        }

        IEnumerator Talents()
        {
            var profil = ProfilArpenteur.Actuel;
            int niveauAvant = profil.niveau;
            int gagnes = profil.AjouterExperience(Competences.Seuil(profil.niveau + 2) - profil.experience);
            Verifier(gagnes >= 1 && profil.pointsTalent >= 1, "Talents et niveaux", $"niveau {niveauAvant} → {profil.niveau}, {profil.pointsTalent} point(s)", "pas de point de talent gagné");
            jeu.Pause();
            jeu.interfaceJeu?.MontrerTalents(false);
            yield return Attendre(1f);
            Capturer("talents");
            var propositions = Competences.Propositions(profil);
            int avant = profil.talents.Count;
            if (propositions.Count > 0) profil.Choisir(propositions[0]);
            Verifier(profil.talents.Count == avant + 1, "Talents et niveaux", propositions.Count > 0 ? $"talent choisi : {Competences.Info(propositions[0]).nom}" : "aucune proposition", "le choix du talent n'a rien fait");
            jeu.interfaceJeu?.FermerTalents();
            yield return Attendre(0.5f);
            jeu.Reprendre();
            yield return Attendre(0.5f);
            Verifier(jeu.Etat == EtatJeu.Jeu, "Talents et niveaux", "retour au jeu", $"état inattendu : {jeu.Etat}");
        }

        IEnumerator PauseJournal()
        {
            jeu.Pause();
            yield return Attendre(0.8f);
            Capturer("pause");
            Verifier(jeu.Etat == EtatJeu.Pause && Time.timeScale == 0f, "Pause et journal", "le temps est suspendu", "la pause n'arrête pas le temps");
            jeu.Reprendre();
            yield return Attendre(0.3f);
            jeu.interfaceJeu?.BasculerJournal();
            yield return Attendre(0.8f);
            Capturer("journal");
            jeu.interfaceJeu?.BasculerJournal();
            Verifier(jeu.Etat == EtatJeu.Jeu, "Pause et journal", "reprise de la partie", $"état inattendu : {jeu.Etat}");
        }

        IEnumerator NavetteExtraction()
        {
            if (jeu.navette == null) { Attention("Navette et extraction", "pas de navette"); yield break; }
            jeu.navette.Embarquer();
            yield return Attendre(1.5f);
            Capturer("vol");
            Verifier(jeu.Etat == EtatJeu.Vol, "Navette et extraction", "à bord, en vol", $"embarquement raté (état {jeu.Etat})");
            jeu.Extraction();
            yield return AttendreQue(() => jeu.Etat == EtatJeu.Fin, jeu.dureeExtraction + 4f);
            yield return Attendre(1f);
            Capturer("fin");
            Verifier(jeu.Etat == EtatJeu.Fin, "Navette et extraction", "écran de fin", $"pas d'écran de fin (état {jeu.Etat})");
        }

        // ------------------------------------------------------------------ rapport

        void Terminer()
        {
            // le profil du joueur est remis comme avant le test
            if (string.IsNullOrEmpty(profilSauve)) PlayerPrefs.DeleteKey(CleProfil);
            else PlayerPrefs.SetString(CleProfil, profilSauve);
            PlayerPrefs.Save();
            ProfilArpenteur.Oublier();

            int echecs = 0, alertes = 0;
            foreach (var r in resultats) { if (r.r == Resultat.Echec) echecs++; else if (r.r == Resultat.Attention) alertes++; }
            var sb = new StringBuilder();
            sb.AppendLine("# Rapport du test automatique");
            sb.AppendLine();
            sb.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm} · Unity {Application.unityVersion} · scène {UnityEngine.SceneManagement.SceneManager.GetActiveScene().path}");
            sb.AppendLine();
            sb.AppendLine($"**Bilan : {echecs} échec(s), {alertes} point(s) à surveiller, {erreurs} erreur(s) et {avertissements} avertissement(s) dans la Console.**");
            sb.AppendLine($"Images par seconde : moyenne {(tempsTotal > 0f ? images / tempsTotal : 0f):0}, pire demi-seconde {(pireSeconde < 999f ? pireSeconde : 0f):0}.");
            sb.AppendLine();
            sb.AppendLine("## Étapes");
            sb.AppendLine();
            foreach (var (r, etape, detail) in resultats)
                sb.AppendLine($"- {(r == Resultat.Ok ? "[OK]" : r == Resultat.Attention ? "[!]" : "[ÉCHEC]")} **{etape}** : {detail}");
            sb.AppendLine();
            sb.AppendLine("## Console (erreurs et avertissements, sans doublons)");
            sb.AppendLine();
            if (messages.Count == 0) sb.AppendLine("Aucun.");
            int k = 0;
            foreach (var m in messages)
            {
                if (++k > 40) { sb.AppendLine($"- … et {messages.Count - 40} autres"); break; }
                sb.AppendLine($"- {m.Key}{(m.Value > 1 ? $" ×{m.Value}" : "")}");
            }
            sb.AppendLine();
            sb.AppendLine("## Captures");
            sb.AppendLine();
            foreach (var c in captures) sb.AppendLine($"- docs/captures/{c}");

            string texte = sb.ToString();
            try
            {
                Directory.CreateDirectory(dossierDocs);
                File.WriteAllText(Path.Combine(dossierDocs, "RAPPORT-TEST.md"), texte, new UTF8Encoding(false));
            }
            catch (Exception e) { Debug.LogWarning("[Test] rapport non écrit : " + e.Message); }
            GUIUtility.systemCopyBuffer = texte;
            if (echecs > 0 || erreurs > 0) Debug.LogError("[Test] " + texte);
            else Debug.Log("[Test] " + texte);
            StartCoroutine(Arreter());
        }

        IEnumerator Arreter()
        {
            yield return new WaitForSecondsRealtime(1.5f);   // laisse la dernière capture s'écrire
            UnityEditor.SessionState.EraseBool(Cle);
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
}
#endif
