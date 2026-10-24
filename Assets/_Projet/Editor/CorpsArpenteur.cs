using System.Linq;
using EchosEther.Joueur;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchosEther.Editeur
{
    /// <summary>
    /// Menus pour habiller l'Arpenteur : le modèle sélectionné (combinaison .glb, .fbx ou prefab) devient son corps,
    /// mis à 1,80 m, pieds au bas de la capsule, capsule cachée, animation et couleurs branchées.
    /// </summary>
    public static class CorpsArpenteur
    {
        const string NomModele = "Modele";
        const float Taille = 1.8f;

        [MenuItem("Echos d'Ether/Utiliser le modèle sélectionné comme corps de l'Arpenteur", false, 22)]
        public static void Utiliser()
        {
            var source = Selection.activeObject as GameObject;
            if (source == null)
            {
                EditorUtility.DisplayDialog("Échos d'Éther", "Sélectionne d'abord un modèle dans la fenêtre Project (combinaison .glb, .fbx ou prefab), puis relance ce menu.", "OK");
                return;
            }
            var joueur = Object.FindFirstObjectByType<ControleurArpenteur>();
            if (joueur == null)
            {
                EditorUtility.DisplayDialog("Échos d'Éther", "Pas d'Arpenteur (ControleurArpenteur) dans la scène ouverte.", "OK");
                return;
            }

            var ancien = joueur.transform.Find(NomModele);
            if (ancien != null) Undo.DestroyObjectImmediate(ancien.gameObject);

            GameObject corps = EditorUtility.IsPersistent(source) ? (GameObject)PrefabUtility.InstantiatePrefab(source) : Object.Instantiate(source);
            Undo.RegisterCreatedObjectUndo(corps, "Corps de l'Arpenteur");
            corps.name = NomModele;
            foreach (var c in corps.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);   // la capsule de l'Arpenteur suffit

            // mesure à l'origine, puis mise à l'échelle et pieds au bas de la capsule
            corps.transform.SetParent(null);
            corps.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            corps.transform.localScale = Vector3.one;
            var b = Limites(corps);
            float echelle = Taille / Mathf.Max(0.01f, b.size.y);
            corps.transform.localScale = Vector3.one * echelle;
            b = Limites(corps);
            Vector3 echelleJoueur = joueur.transform.lossyScale;
            Undo.SetTransformParent(corps.transform, joueur.transform, "Corps de l'Arpenteur");
            corps.transform.localRotation = Quaternion.identity;
            corps.transform.localScale = new Vector3(echelle / echelleJoueur.x, echelle / echelleJoueur.y, echelle / echelleJoueur.z);
            corps.transform.localPosition = new Vector3(-b.center.x / echelleJoueur.x, (-joueur.demiHauteur - b.min.y) / echelleJoueur.y, -b.center.z / echelleJoueur.z);

            // la capsule grise disparaît, le modèle prend sa place
            var rendu = joueur.GetComponent<MeshRenderer>();
            if (rendu != null) { Undo.RecordObject(rendu, "Corps de l'Arpenteur"); rendu.enabled = false; }
            var anim = joueur.GetComponent<AnimateurArpenteur>();
            if (anim == null) anim = Undo.AddComponent<AnimateurArpenteur>(joueur.gameObject);
            Undo.RecordObject(anim, "Corps de l'Arpenteur");
            anim.modele = corps.transform;
            var apparence = joueur.GetComponent<ApparenceArpenteur>();
            if (apparence == null) apparence = Undo.AddComponent<ApparenceArpenteur>(joueur.gameObject);
            Undo.RecordObject(apparence, "Corps de l'Arpenteur");
            apparence.rendus = null;   // vide = tous les rendus du nouveau corps

            EditorSceneManager.MarkSceneDirty(joueur.gameObject.scene);
            Selection.activeGameObject = corps;

            var peaux = corps.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            int os = peaux.SelectMany(p => p.bones ?? new Transform[0]).Where(t => t != null).Distinct().Count();
            bool clips = corps.GetComponentInChildren<Animation>(true) != null || corps.GetComponentInChildren<Animator>(true) != null;
            string squelette = os > 0
                ? $"Squelette : {os} os. {(clips ? "Le modèle a des animations : elles seront jouées." : "Pas d'animation fournie : le squelette sera animé par le code.")}"
                : "Pas de squelette : seul le corps entier sera animé. Pour des jambes et des bras animés, passe le modèle dans Mixamo (gratuit) puis réimporte-le.";
            EditorUtility.DisplayDialog("Échos d'Éther",
                $"« {source.name} » est maintenant le corps de l'Arpenteur (1,80 m, échelle ×{echelle:0.###}).\n\n{squelette}\n\n" +
                "S'il regarde en arrière : menu Echos d'Ether > Retourner le corps de l'Arpenteur.", "OK");
        }

        [MenuItem("Echos d'Ether/Retourner le corps de l'Arpenteur (180°)", false, 23)]
        public static void Retourner()
        {
            var joueur = Object.FindFirstObjectByType<ControleurArpenteur>();
            var corps = joueur != null ? joueur.transform.Find(NomModele) : null;
            if (corps == null)
            {
                EditorUtility.DisplayDialog("Échos d'Éther", "Pas de corps sous l'Arpenteur : utilise d'abord « Utiliser le modèle sélectionné comme corps de l'Arpenteur ».", "OK");
                return;
            }
            Undo.RecordObject(corps, "Retourner le corps");
            corps.localRotation = Quaternion.Euler(0f, 180f, 0f) * corps.localRotation;
            Vector3 p = corps.localPosition;
            corps.localPosition = new Vector3(-p.x, p.y, -p.z);
            EditorSceneManager.MarkSceneDirty(joueur.gameObject.scene);
        }

        static Bounds Limites(GameObject go)
        {
            var rendus = go.GetComponentsInChildren<Renderer>(true);
            if (rendus.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var b = rendus[0].bounds;
            foreach (var r in rendus) b.Encapsulate(r.bounds);
            return b;
        }
    }
}
