using System.Collections.Generic;
using EchosEther.Jeu;
using EchosEther.Prologue;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchosEther.Editeur
{
    /// <summary>
    /// Menu "Echos d'Ether/Prologue" : construit en un clic la scène d'essai des vues, comme vitrine/vues.html.
    /// Un hangar du Colosse (acier sombre, porte ouverte sur l'espace tenue par un champ de force), le héros à pied
    /// (vue 3e personne, fusil à deux mains) et la navette (vue poursuite et vue cockpit), avec leurs effets.
    /// Le héros prend le mannequin du studio (EE_Animations.fbx importé en Humanoid) s'il est dans le projet,
    /// sinon une capsule ; on peut ensuite glisser n'importe quel personnage Humanoid à la place.
    /// </summary>
    public static class ConstructeurPrologue
    {
        const string CheminScene = "Assets/_Projet/Scenes/PrologueEssaiVues.unity";
        const string DossierMateriaux = "Assets/_Projet/Materiaux/Prologue";
        const float Largeur = 40f, Profondeur = 30f, Hauteur = 12f;

        [MenuItem("Echos d'Ether/Prologue/Créer la scène d'essai des vues (hangar, navette)", false, 50)]
        public static void Creer()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var acier = Materiau("Acier", new Color(0.16f, 0.17f, 0.2f), 0.75f, 0.5f);
            var acierClair = Materiau("AcierClair", new Color(0.24f, 0.25f, 0.28f), 0.7f, 0.55f);
            var jaune = Materiau("JauneChantier", new Color(0.54f, 0.42f, 0.12f), 0.4f, 0.4f);
            var coque = Materiau("CoqueNavette", new Color(0.29f, 0.31f, 0.35f), 0.75f, 0.62f);
            var sombre = Materiau("Sombre", new Color(0.11f, 0.12f, 0.15f), 0.6f, 0.5f);
            var rouge = Materiau("RougeEmpire", new Color(0.48f, 0.08f, 0.08f), 0.3f, 0.5f);

            // ---- le hangar
            var hangar = new GameObject("Hangar").transform;
            Bloc(hangar, acier, new Vector3(0f, -0.05f, 0f), new Vector3(Largeur, 0.1f, Profondeur), "Sol");
            Bloc(hangar, acier, new Vector3(0f, Hauteur / 2f, Profondeur / 2f + 0.3f), new Vector3(Largeur, Hauteur, 0.6f), "Mur du fond");
            Bloc(hangar, acier, new Vector3(-Largeur / 2f - 0.3f, Hauteur / 2f, 0f), new Vector3(0.6f, Hauteur, Profondeur), "Mur gauche");
            Bloc(hangar, acier, new Vector3(Largeur / 2f + 0.3f, Hauteur / 2f, 0f), new Vector3(0.6f, Hauteur, Profondeur), "Mur droit");
            Bloc(hangar, acier, new Vector3(0f, Hauteur + 0.3f, 0f), new Vector3(Largeur, 0.6f, Profondeur), "Plafond");
            Bloc(hangar, acier, new Vector3(-14.5f, Hauteur / 2f, -Profondeur / 2f - 0.3f), new Vector3(11f, Hauteur, 0.6f), "Porte, battant gauche");
            Bloc(hangar, acier, new Vector3(14.5f, Hauteur / 2f, -Profondeur / 2f - 0.3f), new Vector3(11f, Hauteur, 0.6f), "Porte, battant droit");
            Bloc(hangar, acier, new Vector3(0f, 10f, -Profondeur / 2f - 0.3f), new Vector3(18f, 4f, 0.6f), "Porte, linteau");
            foreach (float x in new[] { -9f, 9f }) Bloc(hangar, jaune, new Vector3(x, 4f, -Profondeur / 2f), new Vector3(0.5f, 8f, 0.8f), "Chambranle");
            foreach (float x in new[] { -13f, -7f, 7f, 13f })
                foreach (float z in new[] { -6f, 6f })
                    Bloc(hangar, acierClair, new Vector3(x, Hauteur / 2f, z), new Vector3(0.9f, Hauteur, 0.9f), "Pilier");
            Bloc(hangar, jaune, new Vector3(-15f, 0.8f, 9f), new Vector3(2.2f, 1.6f, 1.4f), "Caisse");
            Bloc(hangar, acierClair, new Vector3(14f, 1f, 10f), new Vector3(2.4f, 2f, 1.6f), "Caisse");
            Bloc(hangar, acierClair, new Vector3(0f, 6.5f, Profondeur / 2f - 1.4f), new Vector3(Largeur, 0.25f, 2.2f), "Passerelle");
            foreach (float x in new[] { -12f, 0f, 12f })
                foreach (float z in new[] { -8f, 2f, 10f })
                {
                    var l = Formes.Lumiere(hangar, "Lampe au sodium", new Color(1f, 0.66f, 0.38f), 9000f, 22f);
                    l.transform.position = new Vector3(x, Hauteur - 1.2f, z);
                }
            // le sas : la porte ouverte sur l'espace, l'air tenu par un champ de force que la navette traverse
            var sas = new GameObject("Sas (champ de force)").AddComponent<ChampDeForce>();
            sas.transform.SetParent(hangar, false);
            sas.transform.position = new Vector3(0f, 0f, -Profondeur / 2f);
            sas.largeur = 18f; sas.hauteur = 8f; sas.intensite = 0.45f; sas.bloquer = false;
            var zone = new GameObject("Zone du hangar").AddComponent<BoxCollider>();
            zone.transform.SetParent(hangar, false);
            zone.isTrigger = true;
            zone.center = new Vector3(0f, Hauteur / 2f, 0f);
            zone.size = new Vector3(Largeur, Hauteur, Profondeur);
            var gyro = new GameObject("Gyrophare").AddComponent<Gyrophare>();
            gyro.transform.SetParent(hangar, false);
            gyro.transform.position = new Vector3(-9f, 8.6f, -Profondeur / 2f + 0.5f);
            var panne = new GameObject("Court-circuit").AddComponent<EtincellesPanne>();
            panne.transform.SetParent(hangar, false);
            panne.transform.position = new Vector3(15.2f, 2.6f, -2.9f);
            panne.transform.rotation = Quaternion.LookRotation(new Vector3(-0.4f, 0.2f, 1f));

            // ---- la navette
            var navette = new GameObject("Navette");
            navette.transform.position = new Vector3(0f, 1.2f, -4f);
            navette.transform.rotation = Quaternion.Euler(0f, 180f, 0f);        // le nez vers la porte
            var controle = navette.AddComponent<ControleVaisseau>();
            controle.hangar = zone;
            var corps = navette.GetComponent<Rigidbody>();
            corps.mass = 8000f;
            var boite = navette.AddComponent<BoxCollider>();
            boite.center = new Vector3(0f, -0.1f, 0.4f);
            boite.size = new Vector3(2.6f, 1.9f, 9f);
            Piece(navette.transform, PrimitiveType.Cube, coque, new Vector3(0f, 0f, 0f), new Vector3(2.6f, 1.7f, 7.2f), Vector3.zero, "Fuselage");
            Piece(navette.transform, PrimitiveType.Cube, coque, new Vector3(0f, -0.1f, 4.4f), new Vector3(1.8f, 1.2f, 1.8f), new Vector3(0f, 0f, 0f), "Nez");
            Piece(navette.transform, PrimitiveType.Cube, coque, new Vector3(0f, -0.35f, -1f), new Vector3(9.5f, 0.22f, 2.6f), Vector3.zero, "Ailes");
            Piece(navette.transform, PrimitiveType.Cube, coque, new Vector3(0f, 1.4f, -2.6f), new Vector3(0.22f, 1.6f, 2f), new Vector3(-20f, 0f, 0f), "Dérive");
            Piece(navette.transform, PrimitiveType.Cube, rouge, new Vector3(0f, -0.22f, 0.1f), new Vector3(9.6f, 0.06f, 0.25f), Vector3.zero, "Bande rouge");
            var propulseurs = new List<Propulseur>();
            foreach (float x in new[] { -1.7f, 1.7f })
            {
                Piece(navette.transform, PrimitiveType.Cylinder, sombre, new Vector3(x, -0.2f, -3f), new Vector3(1.15f, 1.3f, 1.15f), new Vector3(90f, 0f, 0f), "Tuyère");
                var p = new GameObject("Propulseur").AddComponent<Propulseur>();
                p.transform.SetParent(navette.transform, false);
                p.transform.localPosition = new Vector3(x, -0.2f, -4.3f);
                p.longueur = 3.2f; p.rayon = 0.48f; p.puissance = 0f;
                propulseurs.Add(p);
            }
            controle.propulseurs = propulseurs.ToArray();
            var verre = Materiau("Verriere", new Color(0.04f, 0.08f, 0.12f), 0.1f, 0.95f);
            Piece(navette.transform, PrimitiveType.Sphere, verre, new Vector3(0f, 0.85f, 1.8f), new Vector3(2.1f, 1.6f, 3.5f), Vector3.zero, "Verrière");
            controle.place = Repere(navette.transform, "Yeux du pilote", new Vector3(0f, 1.28f, 1.6f));
            controle.porte = Repere(navette.transform, "Porte", new Vector3(-2.2f, -1.2f, 0.5f));
            // l'intérieur du cockpit : un pupitre incliné vers le pilote, trois écrans, des montants
            var interieur = new GameObject("Intérieur du cockpit");
            interieur.transform.SetParent(navette.transform, false);
            var pupitre = Piece(interieur.transform, PrimitiveType.Cube, sombre, new Vector3(0f, 0.98f, 2.45f), new Vector3(1.7f, 0.08f, 0.5f), new Vector3(34f, 0f, 0f), "Pupitre");
            int k = 0;
            foreach (var c in new[] { new Color(1f, 0.7f, 0.35f), new Color(0.37f, 0.95f, 1f), new Color(1f, 0.35f, 0.29f) })
            {
                var ecran = Piece(pupitre.transform, PrimitiveType.Cube, Materiau("Ecran" + k, Color.black, 0f, 0.9f, c * 3f),
                    new Vector3(-0.32f + k * 0.32f, 0.6f, 0f), new Vector3(0.25f, 0.2f, 0.44f), Vector3.zero, "Écran");
                k++;
            }
            interieur.SetActive(false);

            // ---- le héros
            var heros = new GameObject("Héros");
            heros.transform.position = new Vector3(0f, 0f, 9f);
            heros.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            var cc = heros.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.35f; cc.center = new Vector3(0f, 0.9f, 0f);
            var controleur = heros.AddComponent<HerosPrologue>();
            Animator animateur = null;
            string modele = TrouverModele("EE_Animations");
            if (modele != null)
            {
                var corpsHeros = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modele));
                corpsHeros.transform.SetParent(heros.transform, false);
                animateur = corpsHeros.GetComponentInChildren<Animator>();
                if (animateur == null) animateur = corpsHeros.AddComponent<Animator>();
                animateur.applyRootMotion = false;
                controleur.animateur = animateur;
                controleur.clips = TrouverClips();
            }
            else
            {
                Piece(heros.transform, PrimitiveType.Capsule, Materiau("CombinaisonOrange", new Color(0.78f, 0.4f, 0.16f), 0.05f, 0.3f),
                    new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 0.9f, 0.6f), Vector3.zero, "Silhouette (sans le mannequin du studio)");
                Debug.LogWarning("Prologue : EE_Animations.fbx est introuvable dans le projet. Importer prologue/00-Animations (Humanoid) pour un héros animé.");
            }
            // le fusil et le bot de prise : les mains vont d'elles-mêmes sur la poignée et le garde-main
            var fusil = new GameObject("Fusil").AddComponent<ArmeTenue>();
            fusil.transform.SetParent(heros.transform, false);
            fusil.Fusil();
            var prise = heros.AddComponent<PriseArme>();
            prise.animateur = animateur;
            prise.arme = fusil;
            controleur.fusil = fusil.gameObject;

            // ---- les caméras et la passerelle
            var cam = Object.FindFirstObjectByType<Camera>();
            cam.name = "Caméra à pied";
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 6000f;
            var vue = cam.gameObject.AddComponent<CameraTroisiemePersonne>();
            vue.cible = heros.transform;
            controleur.vue = vue;
            var camVol = new GameObject("Caméra de vol");
            var c2 = camVol.AddComponent<Camera>();
            c2.nearClipPlane = 0.05f;
            c2.farClipPlane = 6000f;
            var vueVol = camVol.AddComponent<CameraVaisseau>();
            vueVol.vaisseau = controle;
            vueVol.interieur = interieur;
            camVol.SetActive(false);
            var passerelle = new GameObject("Passerelle à bord").AddComponent<PasserelleVaisseau>();
            passerelle.vaisseau = controle;
            passerelle.heros = controleur;
            passerelle.vueAPied = vue;
            passerelle.vueEnVol = vueVol;

            var soleil = Object.FindFirstObjectByType<Light>();
            if (soleil != null) soleil.transform.rotation = Quaternion.Euler(35f, 200f, 0f);

            if (!AssetDatabase.IsValidFolder("Assets/_Projet/Scenes")) AssetDatabase.CreateFolder("Assets/_Projet", "Scenes");
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), CheminScene);
            Debug.Log($"Prologue : scène d'essai des vues créée ({CheminScene}). Play, puis clic pour prendre la souris.");
        }

        static GameObject Bloc(Transform parent, Material m, Vector3 position, Vector3 taille, string nom)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = nom;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = taille;
            go.GetComponent<MeshRenderer>().sharedMaterial = m;
            return go;
        }

        static GameObject Piece(Transform parent, PrimitiveType type, Material m, Vector3 position, Vector3 taille, Vector3 rotation, string nom)
        {
            var go = Formes.Primitive(type, parent, m, nom);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(rotation);
            go.transform.localScale = taille;
            return go;
        }

        static Transform Repere(Transform parent, string nom, Vector3 position)
        {
            var t = new GameObject(nom).transform;
            t.SetParent(parent, false);
            t.localPosition = position;
            return t;
        }

        static Material Materiau(string nom, Color couleur, float metal, float lisse, Color? emission = null)
        {
            string chemin = $"{DossierMateriaux}/M_{nom}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(chemin);
            if (m != null) return m;
            if (!AssetDatabase.IsValidFolder("Assets/_Projet/Materiaux")) AssetDatabase.CreateFolder("Assets/_Projet", "Materiaux");
            if (!AssetDatabase.IsValidFolder(DossierMateriaux)) AssetDatabase.CreateFolder("Assets/_Projet/Materiaux", "Prologue");
            m = emission.HasValue ? Formes.Emissif(couleur, emission.Value) : Formes.Opaque(couleur, metal, lisse);
            AssetDatabase.CreateAsset(m, chemin);
            return m;
        }

        static string TrouverModele(string nom)
        {
            foreach (string guid in AssetDatabase.FindAssets(nom + " t:Model"))
                return AssetDatabase.GUIDToAssetPath(guid);
            return null;
        }

        static AnimationClip[] TrouverClips()
        {
            var clips = new List<AnimationClip>();
            foreach (string fichier in new[] { "UAL1_Standard", "UAL2_Standard", "EE_Animations" })
                foreach (string guid in AssetDatabase.FindAssets(fichier + " t:Model"))
                    foreach (var o in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid)))
                        if (o is AnimationClip c && !c.name.StartsWith("__preview__")) clips.Add(c);
            return clips.ToArray();
        }
    }
}
