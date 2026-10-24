using System.Collections.Generic;
using System.Linq;
using EchosEther.Interface;
using EchosEther.Jeu;
using EchosEther.Joueur;
using EchosEther.Physique;
using EchosEther.Rendu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchosEther.Editeur
{
    /// <summary>
    /// Menu "Echos d'Ether > Installer le jeu dans la scène" : ajoute à la scène ouverte (créée par
    /// "Créer la scène Lisière de l'Horizon" puis décorée) tout ce qu'il faut pour jouer :
    /// menus, enrôlement, didacticiel, mission, cristaux, bulles, monolithe, balise, navette et sons.
    /// Les modèles téléchargés (outils/telecharger-assets.ps1) sont utilisés s'ils sont présents.
    /// Relancer le menu complète l'installation sans rien déplacer.
    /// </summary>
    public static class InstallateurJeu
    {
        const string Externes = "Assets/_Projet/Externes";
        const string NomSystemes = "Systemes de jeu";
        const string NomLieux = "Lieux de jeu";

        // (distance au sol en m, angle en degrés par rapport au regard du joueur) : repris de la maquette
        static readonly Vector2[] Cristaux =
        {
            new Vector2(26, -90), new Vector2(70, -20), new Vector2(95, -135), new Vector2(130, 40), new Vector2(170, 172),
            new Vector2(205, -104), new Vector2(240, 12), new Vector2(300, 128), new Vector2(380, -32),
        };
        static readonly Vector2 BulleLente = new Vector2(165, -62);
        static readonly Vector2 BulleRapide = new Vector2(265, 105);
        static readonly Vector2 Balise = new Vector2(340, 200);
        static readonly Vector2 PositionNavette = new Vector2(13, 32);

        [MenuItem("Echos d'Ether/Installer le jeu dans la scène (menus, didacticiel, mission)", false, 20)]
        public static void Installer()
        {
            var scene = SceneManager.GetActiveScene();
            var joueur = Object.FindFirstObjectByType<ControleurArpenteur>();
            var planete = Object.FindObjectsByType<SourceGravite>(FindObjectsSortMode.None).OrderByDescending(s => s.rayonSurface).FirstOrDefault();
            var camera = Object.FindFirstObjectByType<CameraOrbitale>();
            if (joueur == null || planete == null || camera == null)
            {
                EditorUtility.DisplayDialog("Échos d'Éther",
                    "Il manque l'Arpenteur (ControleurArpenteur), la planète (SourceGravite) ou la caméra (CameraOrbitale).\n\n" +
                    "Lance d'abord : Echos d'Ether > Créer la scène Lisière de l'Horizon.", "OK");
                return;
            }
            Physics.SyncTransforms();
            var rapport = new List<string>();

            // --- l'Arpenteur et la caméra
            var etat = Ajouter<EtatArpenteur>(joueur.gameObject);
            var jauge = Ajouter<JaugeBruit>(joueur.gameObject);
            Ajouter<Scanner>(joueur.gameObject);
            var apparence = Ajouter<ApparenceArpenteur>(joueur.gameObject);
            var cameraNavette = Ajouter<CameraNavette>(camera.gameObject);
            Ajouter<CameraSecousse>(camera.gameObject);
            var provisoire = Object.FindFirstObjectByType<AffichageTemps>();
            if (provisoire != null) provisoire.enabled = false;

            // --- les systèmes
            var systemes = Racine(NomSystemes);
            var gestionnaire = Ajouter<GestionnaireJeu>(systemes);
            var ui = Ajouter<InterfaceJeu>(systemes);
            var horloges = Ajouter<Horloges>(systemes);
            var transmissions = Ajouter<Transmissions>(systemes);
            var mission = Ajouter<Mission>(systemes);
            var ruche = Ajouter<Ruche>(systemes);
            var didacticiel = Ajouter<Didacticiel>(systemes);
            var sons = Ajouter<BanqueSons>(systemes);
            horloges.dilatation = Object.FindFirstObjectByType<DilatationTemporelle>();

            // --- les lieux, posés sur la planète autour du point de départ
            Vector3 depart = joueur.transform.position;
            Vector3 avant = camera.transform.forward;
            var lieux = Racine(NomLieux);

            var groupeCristaux = Enfant(lieux.transform, "Cristaux d'Ether");
            for (int i = 0; i < Cristaux.Length; i++)
            {
                var c = Enfant(groupeCristaux, $"Cristal {i + 1}", out bool nouveau);
                var cristal = Ajouter<CristalEther>(c.gameObject);
                cristal.graine = i;
                if (nouveau) Poser(c, depart, avant, Cristaux[i]);
            }

            var lente = Enfant(lieux.transform, "Bulle lente", out bool nouvelleLente);
            var bulleLente = Ajouter<BulleTemporelle>(lente.gameObject);
            if (nouvelleLente)
            {
                Poser(lente, depart, avant, BulleLente);
                bulleLente.rapport = 0.05f; bulleLente.rayon = 15f; bulleLente.respire = true;
            }
            var mono = Enfant(lente, "Monolithe", out bool nouveauMono);
            Ajouter<Monolithe>(mono.gameObject);
            if (nouveauMono) { mono.localPosition = Vector3.down * 0.5f; mono.localRotation = Quaternion.identity; }

            var rapide = Enfant(lieux.transform, "Bulle rapide", out bool nouvelleRapide);
            var bulleRapide = Ajouter<BulleTemporelle>(rapide.gameObject);
            if (nouvelleRapide)
            {
                Poser(rapide, depart, avant, BulleRapide);
                bulleRapide.rapport = 4f; bulleRapide.rayon = 11f; bulleRapide.respire = false;
                bulleRapide.couleur = new Color(1.4f, 0.8f, 0.4f);
            }

            var balise = Enfant(lieux.transform, "Balise d'extraction", out bool nouvelleBalise);
            var compBalise = Ajouter<BaliseExtraction>(balise.gameObject);
            if (nouvelleBalise) Poser(balise, depart, avant, Balise);
            mission.balise = compBalise;

            // --- la navette (modèle Sketchfab si présent)
            var navette = Object.FindFirstObjectByType<EchosEther.Jeu.Navette>();
            if (navette == null)
            {
                var n = Enfant(lieux.transform, "Navette", out _);
                navette = Ajouter<EchosEther.Jeu.Navette>(n.gameObject);
                Poser(n, depart, avant, PositionNavette);
                n.position += Planete.Haut(n.position) * navette.hauteurPosee;
                var modele = TrouverModele(Externes + "/Sketchfab/Vaisseaux", "drop-ship-or-light-attack", "sci-fi-dropship", "transporter-spaceship", "dropship");
                if (modele != null)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(modele, n);
                    instance.name = "Modele";
                    Normaliser(instance, 14f);
                    rapport.Add($"Navette : modèle {AssetDatabase.GetAssetPath(modele)} (tourner l'objet « Modele » si le nez n'est pas vers l'avant bleu).");
                }
                else
                {
                    Silhouette(n);
                    rapport.Add("Navette : aucun modèle Sketchfab trouvé, silhouette simple à la place.");
                }
            }

            // --- le ver (modèle Sketchfab si présent)
            if (ruche.modeleVer == null)
            {
                var ver = TrouverModele(Externes + "/Sketchfab/Personnages", "sandworm", "sand-worm", "worm");
                if (ver != null)
                {
                    ruche.modeleVer = ver;
                    ruche.echelleModele = Echelle(ver, 30f);
                    rapport.Add($"Ver : modèle {AssetDatabase.GetAssetPath(ver)}.");
                }
            }

            // --- les sons Kenney
            sons.clic = Son("click_");
            sons.transmission = Son("computerNoise_");
            sons.messagePersonnel = Son("glass_");
            sons.etapeValidee = Son("confirmation_");
            sons.finFormation = Son("jingles_HIT0");
            sons.scanner = Son("forceField_");
            sons.cristal = Son("jingles_STEEL0");
            sons.quotaAtteint = Son("jingles_HIT1");
            sons.tremblement = Son("lowFrequency_explosion_");
            sons.attaqueVer = Son("explosionCrunch_");
            sons.impact = Son("impactPunch_heavy_");
            sons.moteurNavette = Son("spaceEngineLow_");
            sons.atterrissageNavette = Son("impactMetal_heavy_");
            sons.monolithe = Son("forceField_004");
            sons.pas = Sons("footstep_snow_");
            int nbSons = new Object[] { sons.clic, sons.transmission, sons.scanner, sons.cristal, sons.moteurNavette }.Count(s => s != null);
            rapport.Add(nbSons > 0 ? $"Sons Kenney branchés ({nbSons}/5 principaux, {sons.pas.Length} pas)." : "Sons : pack Kenney absent (lancer outils/telecharger-assets.ps1, groupe 6).");

            // --- le chef d'orchestre
            gestionnaire.joueur = joueur;
            gestionnaire.etatJoueur = etat;
            gestionnaire.jauge = jauge;
            gestionnaire.apparence = apparence;
            gestionnaire.cameraOrbitale = camera;
            gestionnaire.cameraNavette = cameraNavette;
            gestionnaire.interfaceJeu = ui;
            gestionnaire.horloges = horloges;
            gestionnaire.transmissions = transmissions;
            gestionnaire.mission = mission;
            gestionnaire.didacticiel = didacticiel;
            gestionnaire.navette = navette;
            var tn = Object.FindFirstObjectByType<TrouNoir>();
            if (tn != null) gestionnaire.trouNoir = tn.transform;

            // --- sauvegarde et Build Settings (pour recharger la scène en jeu)
            EditorSceneManager.MarkSceneDirty(scene);
            if (string.IsNullOrEmpty(scene.path))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Projet/Scenes")) AssetDatabase.CreateFolder("Assets/_Projet", "Scenes");
                EditorSceneManager.SaveScene(scene, "Assets/_Projet/Scenes/EchosEther.unity");
            }
            else EditorSceneManager.SaveScene(scene);
            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(s => s.path == scene.path))
            {
                scenes.Insert(0, new EditorBuildSettingsScene(scene.path, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }

            Selection.activeGameObject = systemes;
            Debug.Log("Échos d'Éther installé dans " + scene.path + ". Appuie sur Play : menu, enrôlement, didacticiel et mission.\n- " + string.Join("\n- ", rapport));
        }

        [MenuItem("Echos d'Ether/Replacer les lieux de jeu autour du joueur", false, 21)]
        public static void Replacer()
        {
            var lieux = GameObject.Find(NomLieux);
            if (lieux != null && !EditorUtility.DisplayDialog("Échos d'Éther", "Supprimer les cristaux, bulles, balise et navette actuels pour les replacer autour du joueur ?", "Replacer", "Annuler")) return;
            if (lieux != null) Object.DestroyImmediate(lieux);
            Installer();
        }

        [MenuItem("Echos d'Ether/Effacer le profil et les réglages enregistrés", false, 40)]
        public static void EffacerProfil()
        {
            foreach (var cle in new[] { "Profil", "Sensibilite", "InverserY", "Qualite", "Son", "Volume" }) PlayerPrefs.DeleteKey("EchosEther." + cle);
            PlayerPrefs.Save();
            Debug.Log("Profil d'Arpenteur et réglages effacés.");
        }

        // ------------------------------------------------------------------ outils

        static T Ajouter<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        static GameObject Racine(string nom)
        {
            var go = GameObject.Find(nom);
            return go != null ? go : new GameObject(nom);
        }

        static Transform Enfant(Transform parent, string nom) => Enfant(parent, nom, out _);

        static Transform Enfant(Transform parent, string nom, out bool nouveau)
        {
            var t = parent.Find(nom);
            nouveau = t == null;
            if (t != null) return t;
            t = new GameObject(nom).transform;
            t.SetParent(parent, false);
            return t;
        }

        /// <summary>Pose un objet au sol, à (distance, angle) du départ, debout sur la planète.</summary>
        static void Poser(Transform t, Vector3 depart, Vector3 avant, Vector2 distanceAngle)
        {
            Vector3 direction = Planete.Direction(depart, avant, distanceAngle.x, distanceAngle.y);
            Vector3 point = Planete.PointSurface(direction, t);
            t.position = point;
            t.rotation = Planete.Debout(point, avant);
        }

        static GameObject TrouverModele(string dossier, params string[] mots)
        {
            if (!AssetDatabase.IsValidFolder(dossier)) return null;
            var chemins = AssetDatabase.FindAssets("t:GameObject", new[] { dossier }).Select(g => AssetDatabase.GUIDToAssetPath(g)).ToList();
            foreach (var mot in mots)
                foreach (var chemin in chemins)
                    if (chemin.ToLowerInvariant().Contains(mot)) return AssetDatabase.LoadAssetAtPath<GameObject>(chemin);
            return null;
        }

        static Bounds Limites(GameObject go)
        {
            var rendus = go.GetComponentsInChildren<Renderer>();
            if (rendus.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var b = rendus[0].bounds;
            foreach (var r in rendus) b.Encapsulate(r.bounds);
            return b;
        }

        /// <summary>Met un modèle à la bonne taille (plus grande dimension horizontale), centré et posé sur l'origine de son parent.</summary>
        static void Normaliser(GameObject instance, float taille)
        {
            var parent = instance.transform.parent;
            Vector3 position = parent.position;
            Quaternion rotation = parent.rotation;
            parent.SetPositionAndRotation(Vector3.zero, Quaternion.identity);   // mesures dans le repère du parent
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            var b = Limites(instance);
            instance.transform.localScale = Vector3.one * (taille / Mathf.Max(b.size.x, b.size.z, 0.001f));
            b = Limites(instance);
            instance.transform.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z);
            parent.SetPositionAndRotation(position, rotation);
        }

        static float Echelle(GameObject modele, float longueur)
        {
            var temp = Object.Instantiate(modele);
            var b = Limites(temp);
            Object.DestroyImmediate(temp);
            return longueur / Mathf.Max(b.size.x, b.size.y, b.size.z, 0.001f);
        }

        /// <summary>Navette de remplacement si aucun modèle n'a été téléchargé.</summary>
        static void Silhouette(Transform parent)
        {
            var mat = new Material(Shader.Find("HDRP/Lit")) { name = "CoqueNavette" };
            mat.SetColor("_BaseColor", new Color(0.62f, 0.6f, 0.56f));
            mat.SetFloat("_Metallic", 0.6f);
            mat.SetFloat("_Smoothness", 0.45f);
            var corps = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            corps.name = "Coque";
            corps.transform.SetParent(parent, false);
            corps.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            corps.transform.localScale = new Vector3(3.2f, 6f, 2.4f);
            corps.transform.localPosition = Vector3.up * 1.6f;
            corps.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(corps.GetComponent<Collider>());
            var ailes = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ailes.name = "Ailes";
            ailes.transform.SetParent(parent, false);
            ailes.transform.localScale = new Vector3(11f, 0.35f, 3.2f);
            ailes.transform.localPosition = new Vector3(0f, 1.4f, 1.2f);
            ailes.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(ailes.GetComponent<Collider>());
        }

        static AudioClip[] Clips(string prefixe)
        {
            if (!AssetDatabase.IsValidFolder(Externes + "/Sons")) return new AudioClip[0];
            return AssetDatabase.FindAssets("t:AudioClip", new[] { Externes + "/Sons" })
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Where(p => System.IO.Path.GetFileNameWithoutExtension(p).StartsWith(prefixe, System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p)
                .Select(p => AssetDatabase.LoadAssetAtPath<AudioClip>(p))
                .Where(c => c != null)
                .ToArray();
        }

        static AudioClip Son(string prefixe) => Clips(prefixe).FirstOrDefault();
        static AudioClip[] Sons(string prefixe) => Clips(prefixe);
    }
}
