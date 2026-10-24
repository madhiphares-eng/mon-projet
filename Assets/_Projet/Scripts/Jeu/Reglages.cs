using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>Options du joueur, enregistrées dans les PlayerPrefs.</summary>
    public static class Reglages
    {
        const string Prefixe = "EchosEther.";

        /// <summary>Multiplicateur de la sensibilité de la souris (1 = normal).</summary>
        public static float Sensibilite
        {
            get => PlayerPrefs.GetFloat(Prefixe + "Sensibilite", 1f);
            set => PlayerPrefs.SetFloat(Prefixe + "Sensibilite", Mathf.Clamp(value, 0.2f, 3f));
        }

        public static bool InverserY
        {
            get => PlayerPrefs.GetInt(Prefixe + "InverserY", 0) == 1;
            set => PlayerPrefs.SetInt(Prefixe + "InverserY", value ? 1 : 0);
        }

        /// <summary>Niveau de qualité Unity (Project Settings > Quality).</summary>
        public static int Qualite
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(Prefixe + "Qualite", QualitySettings.GetQualityLevel()), 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            set => PlayerPrefs.SetInt(Prefixe + "Qualite", value);
        }

        public static bool Son
        {
            get => PlayerPrefs.GetInt(Prefixe + "Son", 1) == 1;
            set => PlayerPrefs.SetInt(Prefixe + "Son", value ? 1 : 0);
        }

        public static float Volume
        {
            get => PlayerPrefs.GetFloat(Prefixe + "Volume", 0.8f);
            set => PlayerPrefs.SetFloat(Prefixe + "Volume", Mathf.Clamp01(value));
        }

        public static void Appliquer()
        {
            if (QualitySettings.names.Length > 0 && QualitySettings.GetQualityLevel() != Qualite)
                QualitySettings.SetQualityLevel(Qualite, true);
            AudioListener.volume = Son ? Volume : 0f;
        }

        public static void Sauver() => PlayerPrefs.Save();
    }
}
