using EchosEther.Combat;
using EchosEther.Jeu;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace EchosEther.Prologue
{
    /// <summary>
    /// Le gyrophare d'alarme : deux faisceaux opposés qui tournent dans la poussière, et la lumière qui balaie les murs.
    /// </summary>
    public class Gyrophare : MonoBehaviour
    {
        public Color couleur = new Color(1f, 0.16f, 0.1f);
        [Tooltip("Degrés par seconde")] public float vitesse = 180f;
        public float portee = 9f;
        [Tooltip("Lumière du projecteur (lumens)")] public float lumens = 30000f;
        public bool actif = true;
        Transform tete;
        Light projecteur;
        Material corps;

        void Start()
        {
            corps = Formes.Emissif(couleur * 0.4f, couleur * 8f);
            var boitier = Formes.Primitive(PrimitiveType.Cylinder, transform, corps, "Boîtier");
            boitier.transform.localScale = new Vector3(0.18f, 0.08f, 0.18f);
            tete = new GameObject("Tête").transform;
            tete.SetParent(transform, false);
            var faisceau = EffetsColosse.Volume(couleur, couleur * 1.3f, 1.5f, 1f, 0.02f, 0.25f, 0.6f);
            foreach (float s in new[] { 1f, -1f })
            {
                var go = new GameObject("Faisceau");
                go.transform.SetParent(tete, false);
                go.transform.localPosition = new Vector3(s * portee * 0.7f, 0f, 0f);
                go.transform.localRotation = Quaternion.Euler(0f, 0f, s * 90f);     // le sommet du cône au gyrophare
                go.AddComponent<MeshFilter>().sharedMesh = EffetsColosse.Tronc(portee * 0.22f, 0.01f, portee * 0.7f, 24);
                var r = go.AddComponent<MeshRenderer>();
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.sharedMaterial = faisceau;
            }
            var lp = new GameObject("Projecteur");
            lp.transform.SetParent(tete, false);
            lp.transform.localRotation = Quaternion.LookRotation(new Vector3(1f, -0.15f, 0f));
            lp.AddComponent<HDAdditionalLightData>();
            projecteur = lp.GetComponent<Light>();
            projecteur.type = LightType.Spot;
            projecteur.color = couleur;
            projecteur.range = portee;
            projecteur.spotAngle = 50f;
            projecteur.intensity = lumens;
            projecteur.shadows = LightShadows.None;
        }

        void Update()
        {
            tete.gameObject.SetActive(actif);
            projecteur.enabled = actif;
            if (actif) tete.Rotate(0f, vitesse * Time.deltaTime, 0f, Space.Self);
        }
    }
}
