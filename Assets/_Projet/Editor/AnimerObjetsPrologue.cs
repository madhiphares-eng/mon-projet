using System;
using System.IO;
using System.Text.RegularExpressions;
using EchosEther.Prologue;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchosEther.Editeur
{
    /// <summary>
    /// Menu "Echos d'Ether/Prologue/Animer les objets du décor" : pose un <see cref="ObjetAnime"/> sur chaque objet de la
    /// scène ouverte qui a une recette dans docs/prologue/objets-animes.json (les portes, sas, armoires, consoles,
    /// machines, reliques et drones du Colosse), réglé comme le bot des objets l'a trouvé (vitrine/banc-objets.html).
    /// Un objet est reconnu à son nom : celui de son modèle (« porte-coulissante », « porte-coulissante (2) »…).
    /// Le relancer remet les recettes à jour. Ajoute aussi l'essai de l'énergie (F8) s'il manque.
    /// </summary>
    public static class AnimerObjetsPrologue
    {
        [Serializable] class Recette { public string cle; public string categorie; public MouvementObjet[] mouvements; public string[] lumieres; }
        [Serializable] class Fichier { public Recette[] objets; }

        [MenuItem("Echos d'Ether/Prologue/Animer les objets du décor", false, 52)]
        static void Animer()
        {
            string chemin = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs", "prologue", "objets-animes.json"));
            if (!File.Exists(chemin))
            {
                EditorUtility.DisplayDialog("Animer les objets", $"Le fichier des recettes manque :\n{chemin}\n\nRelancer l'installateur, qui met à jour la documentation du jeu.", "OK");
                return;
            }
            var fichier = JsonUtility.FromJson<Fichier>(File.ReadAllText(chemin));
            int poses = 0;
            var noms = new System.Text.StringBuilder();
            foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                foreach (var r in fichier.objets)
                {
                    string nom = r.cle.Substring(r.cle.LastIndexOf('/') + 1);
                    if (!Regex.IsMatch(t.name, "^" + Regex.Escape(nom) + @"(\s*\(\d+\))?$")) continue;
                    var objet = t.GetComponent<ObjetAnime>() ?? Undo.AddComponent<ObjetAnime>(t.gameObject);
                    Undo.RecordObject(objet, "Animer les objets du décor");
                    objet.cle = r.cle;
                    objet.mouvements = new System.Collections.Generic.List<MouvementObjet>(r.mouvements ?? new MouvementObjet[0]);
                    objet.lumieres = r.lumieres ?? new string[0];
                    EditorUtility.SetDirty(objet);
                    poses++;
                    noms.Append(t.name).Append(", ");
                    break;
                }
            if (UnityEngine.Object.FindFirstObjectByType<EnergieColosseEssai>() == null && poses > 0)
                Undo.RegisterCreatedObjectUndo(new GameObject("Énergie du Colosse (F8)", typeof(EnergieColosseEssai)), "Énergie du Colosse");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"Objets du décor animés : {poses}. {noms}");
            EditorUtility.DisplayDialog("Animer les objets", poses > 0
                ? $"{poses} objets animés. En jeu, F8 fait passer le Colosse en alarme, puis en panne."
                : "Aucun objet de la scène ne porte le nom d'un modèle qui bouge (porte-coulissante, porte-sas, console-murale…).", "OK");
        }
    }
}
