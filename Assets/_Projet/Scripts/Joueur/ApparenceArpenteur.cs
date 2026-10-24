using System.Collections.Generic;
using EchosEther.Jeu;
using UnityEngine;

namespace EchosEther.Joueur
{
    /// <summary>
    /// Applique les couleurs choisies à l'enrôlement au modèle de l'Arpenteur (HDRP/Lit ou autre).
    /// Les matériaux dont le nom contient un des mots de <see cref="motsVisiere"/> sont traités comme la visière
    /// (couleur émissive) ; les autres reçoivent la teinte de la combinaison, multipliée par leur couleur d'origine.
    /// </summary>
    public class ApparenceArpenteur : MonoBehaviour
    {
        [Tooltip("Rendus à teinter. Vide = tous les rendus enfants.")]
        public Renderer[] rendus;

        [Tooltip("Mots qui désignent la visière dans le nom des matériaux.")]
        public string[] motsVisiere = { "visor", "visiere", "visière", "glass", "verre", "astnt1_2", "rocktrack" };

        [Tooltip("Mots des matériaux à ne jamais teinter (peau, logos...).")]
        public string[] motsIgnores = { "skin", "peau", "face", "logo", "flag", "drapeau" };

        [Tooltip("Intensité de la lueur de la visière (HDRP : valeur émissive).")]
        public float intensiteVisiere = 6f;

        static readonly int IdBase = Shader.PropertyToID("_BaseColor");
        static readonly int IdCouleur = Shader.PropertyToID("_Color");
        static readonly int IdEmission = Shader.PropertyToID("_EmissiveColor");
        static readonly int IdEmissionBuiltin = Shader.PropertyToID("_EmissionColor");

        readonly Dictionary<Material, Color> couleursOrigine = new Dictionary<Material, Color>();
        readonly List<Material> combinaison = new List<Material>();
        readonly List<Material> visiere = new List<Material>();
        bool pret;

        void Start() => Appliquer();

        void Preparer()
        {
            if (pret) return;
            pret = true;
            if (rendus == null || rendus.Length == 0) rendus = GetComponentsInChildren<Renderer>(true);
            foreach (var r in rendus)
            {
                if (r == null || r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
                foreach (var m in r.materials)      // copies propres à ce joueur
                {
                    string n = m.name.ToLowerInvariant();
                    if (Contient(n, motsIgnores)) continue;
                    if (Contient(n, motsVisiere)) visiere.Add(m);
                    else
                    {
                        combinaison.Add(m);
                        couleursOrigine[m] = m.HasProperty(IdBase) ? m.GetColor(IdBase) : m.HasProperty(IdCouleur) ? m.GetColor(IdCouleur) : Color.white;
                    }
                }
            }
        }

        /// <summary>À appeler après un changement de profil (l'aperçu de l'enrôlement le fait en direct).</summary>
        public void Appliquer()
        {
            Preparer();
            var profil = ProfilArpenteur.Actuel;
            Color teinte = profil.CouleurCombinaison;
            foreach (var m in combinaison)
            {
                Color c = couleursOrigine[m] * teinte;
                c.a = couleursOrigine[m].a;
                if (m.HasProperty(IdBase)) m.SetColor(IdBase, c);
                else if (m.HasProperty(IdCouleur)) m.SetColor(IdCouleur, c);
            }
            Color lueur = profil.CouleurVisiere * intensiteVisiere;
            foreach (var m in visiere)
            {
                if (m.HasProperty(IdEmission)) m.SetColor(IdEmission, lueur);
                else if (m.HasProperty(IdEmissionBuiltin)) { m.EnableKeyword("_EMISSION"); m.SetColor(IdEmissionBuiltin, lueur); }
            }
        }

        static bool Contient(string texte, string[] mots)
        {
            foreach (var mot in mots) if (!string.IsNullOrEmpty(mot) && texte.Contains(mot)) return true;
            return false;
        }
    }
}
