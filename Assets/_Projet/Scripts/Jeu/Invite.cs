using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Invite d'action au centre de l'écran ("Maintenir E · récolter l'Éther").
    /// Chaque objet la propose pendant son Update ; l'interface affiche la plus prioritaire en LateUpdate.
    /// </summary>
    public static class Invite
    {
        static int image = -1;
        static float priorite;
        static string texte;
        static float progression;

        /// <param name="progressionActuelle">Entre 0 et 1, ou négatif pour ne pas afficher de barre.</param>
        public static void Proposer(string texteInvite, float progressionActuelle = -1f, float prioriteInvite = 0f)
        {
            if (image != Time.frameCount)
            {
                image = Time.frameCount;
                texte = null;
                priorite = float.MinValue;
            }
            if (prioriteInvite < priorite) return;
            priorite = prioriteInvite;
            texte = texteInvite;
            progression = progressionActuelle;
        }

        /// <summary>Invite de l'image en cours, s'il y en a une.</summary>
        public static bool Lire(out string texteInvite, out float progressionActuelle)
        {
            bool actuelle = image == Time.frameCount && texte != null;
            texteInvite = actuelle ? texte : null;
            progressionActuelle = actuelle ? progression : -1f;
            return actuelle;
        }
    }
}
