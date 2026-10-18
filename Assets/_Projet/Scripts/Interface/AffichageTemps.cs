using EchosEther.Physique;
using UnityEngine;

namespace EchosEther.Interface
{
    /// <summary>
    /// Affichage provisoire (OnGUI) du temps vécu en surface et du temps écoulé sur le vaisseau-mère.
    /// Sera remplacé par une vraie interface plus tard.
    /// </summary>
    public class AffichageTemps : MonoBehaviour
    {
        public DilatationTemporelle dilatation;

        GUIStyle style;

        void OnGUI()
        {
            if (dilatation == null) return;

            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = 18 };
                style.normal.textColor = new Color(1f, 0.85f, 0.6f);
            }

            string texte =
                $"Temps vécu : {DilatationTemporelle.Formater(dilatation.TempsPropre)}\n" +
                $"Vaisseau-mère : {DilatationTemporelle.Formater(dilatation.TempsOrbital)}\n" +
                $"dτ/dt : {dilatation.Facteur:0.0000}";
            GUI.Label(new Rect(20, 20, 500, 90), texte, style);
        }
    }
}
