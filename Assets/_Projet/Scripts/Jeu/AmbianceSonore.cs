using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Ambiance sonore générée en direct (comme dans la maquette) : vent qui forcit avec la vitesse,
    /// nappe grave qui descend quand le temps ralentit dans une bulle, grondement pendant les
    /// tremblements de la Ruche et la poussée de la navette. Aucun fichier son nécessaire.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AmbianceSonore : MonoBehaviour
    {
        [Range(0f, 1f)] public float volumeVent = 0.5f;
        [Range(0f, 1f)] public float volumeNappe = 0.5f;
        [Range(0f, 1f)] public float volumeGrondement = 0.8f;

        // écrits par le thread principal, lus par le thread audio
        volatile float cibleVent, cibleNappe, cibleGrondement, cibleHauteur = 1f;

        // état du thread audio
        float vent, nappe, grondement, hauteur = 1f;
        float filtreVent, filtreVent2, filtreGrondement;
        double phase1, phase2, phase3, phaseLfo;
        int frequence = 48000;
        uint graine = 22222;

        static readonly float[] Frequences = { 55f, 82.4f, 110.6f };

        void Start()
        {
            frequence = AudioSettings.outputSampleRate;
            var source = GetComponent<AudioSource>();
            source.clip = AudioClip.Create("Ambiance", frequence * 2, 1, frequence, true, Lire);
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 1f;
            source.Play();
        }

        void Update()
        {
            var jeu = GestionnaireJeu.Instance;
            if (jeu == null) return;
            bool enPartie = jeu.Etat == EtatJeu.Jeu || jeu.Etat == EtatJeu.Vol || jeu.Etat == EtatJeu.Largage || jeu.Etat == EtatJeu.Extraction;
            float vitesse = 0f, poussee = 0f;
            if (jeu.NavetteEnVol != null) { vitesse = jeu.NavetteEnVol.Vitesse * 0.4f; poussee = 0.4f + Mathf.Min(1f, jeu.NavetteEnVol.Vitesse / 60f); }
            else if (jeu.etatJoueur != null && jeu.etatJoueur.isActiveAndEnabled) vitesse = jeu.etatJoueur.Vitesse;
            if (jeu.Etat == EtatJeu.Largage || jeu.Etat == EtatJeu.Extraction) poussee = Mathf.Max(poussee, 0.8f);
            float tremblement = Joueur.CameraSecousse.Instance != null ? Joueur.CameraSecousse.Instance.Continue : 0f;
            float bulle = BulleTemporelle.PresenceLente(jeu.PositionObservateur);

            cibleVent = (enPartie || jeu.Etat == EtatJeu.Menu || jeu.Etat == EtatJeu.Enrolement ? 0.35f + Mathf.Min(vitesse, 20f) * 0.06f : 0.2f) * volumeVent;
            cibleNappe = volumeNappe * (jeu.Etat == EtatJeu.Pause ? 0.4f : 1f);
            cibleGrondement = Mathf.Max(tremblement * 0.9f, poussee * 0.3f) * volumeGrondement;
            cibleHauteur = 1f - 0.35f * bulle;
        }

        // thread audio : pas d'API Unity ici
        void Lire(float[] donnees)
        {
            float lissage = 1f / (frequence * 0.3f);
            for (int i = 0; i < donnees.Length; i++)
            {
                vent += (cibleVent - vent) * lissage;
                nappe += (cibleNappe - nappe) * lissage;
                grondement += (cibleGrondement - grondement) * (lissage * 2f);
                hauteur += (cibleHauteur - hauteur) * (lissage * 0.5f);

                float bruit = Aleatoire();
                // vent : bruit blanc passe-bande autour de 400 Hz
                filtreVent += (bruit - filtreVent) * 0.06f;
                filtreVent2 += (filtreVent - filtreVent2) * 0.02f;
                float v = (filtreVent - filtreVent2) * 2.2f * vent;
                // grondement : bruit passe-bas autour de 60 Hz
                filtreGrondement += (bruit - filtreGrondement) * 0.008f;
                float g = filtreGrondement * 3f * grondement;
                // nappe : trois notes graves qui respirent lentement
                phaseLfo += 0.07 / frequence;
                float respiration = 0.7f + 0.3f * Mathf.Sin((float)(phaseLfo * 2.0 * System.Math.PI));
                phase1 += Frequences[0] * hauteur / frequence;
                phase2 += Frequences[1] * hauteur / frequence;
                phase3 += Frequences[2] * hauteur / frequence;
                float n = (Triangle(phase1) * 0.05f + Sinus(phase2) * 0.035f + Sinus(phase3) * 0.015f) * respiration * nappe;

                donnees[i] = Mathf.Clamp(v + g + n, -1f, 1f);
            }
            phase1 %= 1.0; phase2 %= 1.0; phase3 %= 1.0; phaseLfo %= 1.0;
        }

        float Aleatoire()
        {
            graine ^= graine << 13; graine ^= graine >> 17; graine ^= graine << 5;
            return (graine & 0xFFFFFF) / 8388608f - 1f;
        }

        static float Sinus(double phase) => Mathf.Sin((float)(phase * 2.0 * System.Math.PI));
        static float Triangle(double phase) { float p = (float)(phase % 1.0); return 4f * Mathf.Abs(p - 0.5f) - 1f; }
    }
}
