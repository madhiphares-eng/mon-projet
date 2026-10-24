using EchosEther.Jeu;
using EchosEther.Outils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchosEther.Editeur
{
    /// <summary>
    /// Menu "Echos d'Ether > Test automatique" : passe en mode Play, le jeu se joue tout seul pendant environ
    /// une minute (voir <see cref="TestAutomatique"/>), puis revient en mode édition avec docs/RAPPORT-TEST.md
    /// et des captures dans docs/captures. Pratique pour Claude en local, qui peut lancer ce menu par MCP for Unity.
    /// </summary>
    [InitializeOnLoad]
    public static class MenuTestAutomatique
    {
        static MenuTestAutomatique()
        {
            EditorApplication.playModeStateChanged += etat =>
            {
                if (etat != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(TestAutomatique.Cle, false)) return;
                SessionState.EraseBool(TestAutomatique.Cle);   // test interrompu à la main
            };
        }

        [MenuItem("Echos d'Ether/Test automatique (rapport dans docs)", false, 32)]
        public static void Lancer()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Échos d'Éther", "Arrête d'abord le mode Play, puis relance le test.", "OK");
                return;
            }
            if (Object.FindFirstObjectByType<GestionnaireJeu>() == null)
            {
                EditorUtility.DisplayDialog("Échos d'Éther", "Le jeu n'est pas installé dans la scène ouverte : menu Echos d'Ether > Installer le jeu dans la scène.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            SessionState.SetBool(TestAutomatique.Cle, true);
            Debug.Log("Test automatique : le jeu va se jouer tout seul pendant environ une minute. Ne touche à rien ; le rapport sera dans docs/RAPPORT-TEST.md.");
            EditorApplication.isPlaying = true;
        }
    }
}
