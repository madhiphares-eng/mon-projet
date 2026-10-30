using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Prologue
{
    /// <summary>
    /// La caméra d'un vaisseau, deux vues (V, manette : croix haut) :
    /// - poursuite : derrière et au-dessus ; l'orientation suit avec retard (le vaisseau pivote dans l'image),
    ///   la caméra recule un peu et le champ de vision s'ouvre avec la vitesse ;
    /// - cockpit : les yeux du pilote, la tête qui encaisse les accélérations ; l'intérieur du cockpit n'est montré qu'ici.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraVaisseau : MonoBehaviour
    {
        public enum Vue { Poursuite, Cockpit }
        public ControleVaisseau vaisseau;
        public Vue vue = Vue.Poursuite;
        public Vector3 decalagePoursuite = new Vector3(0f, 3f, -11.5f);
        public float viseeDevant = 16f;
        [Tooltip("Plus c'est grand, plus l'orientation suit vite")] public float suivi = 4.5f;
        public float champ = 60f, champVitesse = 14f, champCockpit = 74f;
        [Tooltip("L'intérieur du cockpit (montré seulement en vue cockpit)")] public GameObject interieur;

        Camera cam;
        Quaternion orientation;
        float recul;
        Vector3 tete, vitessePrecedente;
        bool pret;

        void OnEnable() => pret = false;

        void LateUpdate()
        {
            if (vaisseau == null) return;
            if (cam == null) cam = GetComponent<Camera>();
            if (EntreesJeu.ChangerVue()) vue = vue == Vue.Poursuite ? Vue.Cockpit : Vue.Poursuite;
            if (interieur != null) interieur.SetActive(vue == Vue.Cockpit);
            float dt = Mathf.Max(Time.deltaTime, 1e-4f);
            var t = vaisseau.transform;
            float v = vaisseau.Vitesse.magnitude, boost = vaisseau.Boost ? 1f : 0f;
            if (!pret) { orientation = t.rotation; vitessePrecedente = vaisseau.Vitesse; pret = true; }

            if (vue == Vue.Poursuite)
            {
                orientation = Quaternion.Slerp(orientation, t.rotation, 1f - Mathf.Exp(-suivi * dt));
                recul = Mathf.Lerp(recul, Mathf.Min(6f, v * 0.04f) + boost * 1.5f, 1f - Mathf.Exp(-2f * dt));
                transform.position = t.position + orientation * (decalagePoursuite + Vector3.back * recul);
                Vector3 vise = t.position + orientation * new Vector3(0f, 1f, viseeDevant);
                transform.rotation = Quaternion.LookRotation(vise - transform.position, orientation * Vector3.up);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, champ + Mathf.Min(champVitesse, v * 0.15f) + boost * 5f, 1f - Mathf.Exp(-3f * dt));
            }
            else
            {
                // la tête recule quand on accélère, part de côté dans les virages
                Vector3 acceleration = (vaisseau.Vitesse - vitessePrecedente) / dt;
                Vector3 locale = Quaternion.Inverse(t.rotation) * acceleration * -0.0025f;
                tete = Vector3.Lerp(tete, Vector3.ClampMagnitude(locale, 0.08f), 1f - Mathf.Exp(-5f * dt));
                var place = vaisseau.place != null ? vaisseau.place : t;
                transform.position = place.position + t.rotation * tete;
                transform.rotation = place.rotation;
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, champCockpit + boost * 5f, 1f - Mathf.Exp(-3f * dt));
            }
            vitessePrecedente = vaisseau.Vitesse;
        }
    }
}
