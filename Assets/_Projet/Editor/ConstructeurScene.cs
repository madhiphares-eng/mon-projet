using EchosEther.Interface;
using EchosEther.Joueur;
using EchosEther.Physique;
using EchosEther.Rendu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace EchosEther.Editeur
{
    /// <summary>
    /// Menu "Echos d'Ether" : construit la scène de test de la Phase 1 en un clic.
    /// </summary>
    public static class ConstructeurScene
    {
        const string CheminScene = "Assets/_Projet/Scenes/LisiereHorizon.unity";
        const string CheminMateriau = "Assets/_Projet/Materiaux/M_Lentille.mat";
        const string NomShaderLentille = "FullScreen/LentilleGravitationnelle";

        const float RayonPlanete = 500f;
        const float RayonSchwarzschild = 1000f;

        [MenuItem("Echos d'Ether/Créer la scène Lisière de l'Horizon")]
        public static void Creer()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Planète
            var planete = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planete.name = "Planete";
            planete.transform.localScale = Vector3.one * RayonPlanete * 2f;
            var source = planete.AddComponent<SourceGravite>();
            source.rayonSurface = RayonPlanete;
            source.porteeMax = RayonPlanete * 10f;

            // Arpenteur (le joueur)
            var joueur = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            joueur.name = "Arpenteur";
            joueur.transform.position = new Vector3(0f, RayonPlanete + 2f, 0f);
            var corps = joueur.AddComponent<Rigidbody>();
            corps.mass = 80f;
            corps.interpolation = RigidbodyInterpolation.Interpolate;
            corps.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            joueur.AddComponent<CorpsGravitationnel>();
            var controleur = joueur.AddComponent<ControleurArpenteur>();

            // Trou noir, haut dans le ciel devant le joueur
            var trouNoir = new GameObject("TrouNoir");
            trouNoir.transform.position = new Vector3(0f, 6000f, 12000f);
            trouNoir.AddComponent<TrouNoir>().rayonSchwarzschild = RayonSchwarzschild;

            var dilatation = joueur.AddComponent<DilatationTemporelle>();
            dilatation.trouNoir = trouNoir.transform;
            dilatation.rayonSchwarzschild = RayonSchwarzschild;

            // Caméra
            var camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 100000f;
                camera.gameObject.AddComponent<CameraOrbitale>().cible = joueur.transform;
                camera.gameObject.AddComponent<AffichageTemps>().dilatation = dilatation;
                controleur.cameraRef = camera.transform;
            }

            // Soleil
            var soleil = Object.FindFirstObjectByType<Light>();
            if (soleil != null) soleil.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Lentille gravitationnelle (passe plein écran)
            var volume = new GameObject("LentilleGravitationnelle").AddComponent<CustomPassVolume>();
            volume.injectionPoint = CustomPassInjectionPoint.BeforePostProcess;
            var passe = new FullScreenCustomPass
            {
                name = "Lentille",
                fullscreenPassMaterial = ObtenirMateriauLentille(),
                materialPassName = "Custom Pass 0",
                fetchColorBuffer = true,
                targetColorBuffer = CustomPass.TargetBuffer.Camera,
            };
            volume.customPasses.Add(passe);

            AssurerDossier("Assets/_Projet", "Scenes");
            EditorSceneManager.SaveScene(scene, CheminScene);
            Selection.activeGameObject = joueur;
            Debug.Log("Scène Lisière de l'Horizon créée : " + CheminScene + ". Appuie sur Play.");
        }

        static Material ObtenirMateriauLentille()
        {
            var materiau = AssetDatabase.LoadAssetAtPath<Material>(CheminMateriau);
            if (materiau != null) return materiau;

            var shader = Shader.Find(NomShaderLentille);
            if (shader == null)
            {
                Debug.LogError("Shader introuvable : " + NomShaderLentille);
                return null;
            }

            AssurerDossier("Assets/_Projet", "Materiaux");
            materiau = new Material(shader);
            AssetDatabase.CreateAsset(materiau, CheminMateriau);
            return materiau;
        }

        static void AssurerDossier(string parent, string nom)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + nom))
                AssetDatabase.CreateFolder(parent, nom);
        }
    }
}
