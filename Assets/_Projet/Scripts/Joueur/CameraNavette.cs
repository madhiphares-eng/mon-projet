using EchosEther.Jeu;
using UnityEngine;

namespace EchosEther.Joueur
{
    /// <summary>
    /// Caméra de poursuite de la navette : derrière et au-dessus, alignée sur le "haut" de la planète,
    /// sans jamais passer sous le sol. Activée par le <see cref="GestionnaireJeu"/> pendant le vol.
    /// </summary>
    public class CameraNavette : MonoBehaviour
    {
        public Transform cible;
        public float distance = 24f;
        public float hauteur = 6.5f;
        public float lissage = 6f;

        void LateUpdate()
        {
            if (cible == null) return;
            Vector3 haut = Planete.Haut(cible.position);
            Vector3 avant = Vector3.ProjectOnPlane(cible.forward, haut).normalized;
            if (avant.sqrMagnitude < 0.01f) avant = Vector3.ProjectOnPlane(transform.forward, haut).normalized;

            Vector3 voulu = cible.position - avant * distance + haut * hauteur;
            // jamais sous le relief
            if (Planete.Sol(voulu, out RaycastHit sol, cible) && Vector3.Dot(voulu - sol.point, haut) < 1.5f)
                voulu = sol.point + haut * 1.5f;

            float k = 1f - Mathf.Exp(-lissage * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, voulu, k);
            Vector3 vise = cible.position + avant * 14f + haut * 2f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(vise - transform.position, haut), k);
        }

        /// <summary>Place la caméra directement derrière la cible (au moment d'embarquer).</summary>
        public void Recaler()
        {
            if (cible == null) return;
            Vector3 haut = Planete.Haut(cible.position);
            Vector3 avant = Vector3.ProjectOnPlane(cible.forward, haut).normalized;
            transform.position = cible.position - avant * distance + haut * hauteur;
            transform.rotation = Quaternion.LookRotation(cible.position + avant * 14f - transform.position, haut);
        }
    }
}
