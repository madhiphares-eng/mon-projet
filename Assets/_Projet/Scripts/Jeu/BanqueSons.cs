using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Sons du jeu (packs Kenney CC0 branchés par l'installateur), pas de l'Arpenteur et nappe d'ambiance.
    /// Tous les champs sont facultatifs : un son manquant est simplement ignoré.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class BanqueSons : MonoBehaviour
    {
        public static BanqueSons Instance { get; private set; }

        [Header("Interface")]
        public AudioClip clic;
        public AudioClip transmission;
        public AudioClip messagePersonnel;
        public AudioClip etapeValidee;
        public AudioClip finFormation;

        [Header("Mécaniques")]
        public AudioClip scanner;
        public AudioClip cristal;
        public AudioClip quotaAtteint;
        public AudioClip tremblement;
        public AudioClip attaqueVer;
        public AudioClip impact;
        public AudioClip moteurNavette;
        public AudioClip atterrissageNavette;
        public AudioClip monolithe;
        public AudioClip[] pas;

        [Header("Pas")]
        [Tooltip("Distance parcourue entre deux bruits de pas (marche).")]
        public float foulee = 1.9f;

        AudioSource source;
        float distancePas;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        void OnEnable() => Instance = this;
        void OnDisable() { if (Instance == this) Instance = null; }

        /// <summary>Joue un son d'interface ou d'événement (sans position).</summary>
        public static void Jouer(AudioClip clip, float volume = 1f)
        {
            if (clip != null && Instance != null) Instance.source.PlayOneShot(clip, volume);
        }

        /// <summary>Joue un son placé dans le monde (grondement, navette...).</summary>
        public static void JouerA(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip != null) AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        void Update()
        {
            var jeu = GestionnaireJeu.Instance;
            if (pas == null || pas.Length == 0 || jeu == null || jeu.Etat != EtatJeu.Jeu || jeu.etatJoueur == null) return;
            var e = jeu.etatJoueur;
            if (!e.AuSol || !e.Bouge) { distancePas = 0f; return; }
            distancePas += e.Vitesse * Time.deltaTime;
            float foul = e.Course ? foulee * 1.5f : foulee;
            if (distancePas < foul) return;
            distancePas = 0f;
            float volume = e.Silence ? 0.12f : e.Course ? 0.55f : 0.32f;
            source.PlayOneShot(pas[Random.Range(0, pas.Length)], volume * ProfilArpenteur.Actuel.Donnees.bruitPas);
        }
    }
}
