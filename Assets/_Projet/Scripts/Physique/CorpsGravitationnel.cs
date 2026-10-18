using UnityEngine;

namespace EchosEther.Physique
{
    /// <summary>
    /// Applique la somme des SourceGravite à un Rigidbody et l'oriente "debout" sur la planète.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CorpsGravitationnel : MonoBehaviour
    {
        [Tooltip("Aligne l'axe vertical de l'objet sur la gravité (personnages, véhicules).")]
        public bool alignerSurGravite = true;

        [Tooltip("Vitesse de réalignement, en degrés par seconde.")]
        public float vitesseAlignement = 360f;

        public Vector3 GraviteActuelle { get; private set; }
        public Vector3 Haut => GraviteActuelle.sqrMagnitude > 0.0001f ? -GraviteActuelle.normalized : Vector3.up;

        Rigidbody corps;

        void Awake()
        {
            corps = GetComponent<Rigidbody>();
            corps.useGravity = false;
            if (alignerSurGravite) corps.constraints |= RigidbodyConstraints.FreezeRotation;
        }

        void FixedUpdate()
        {
            Vector3 g = Vector3.zero;
            foreach (var source in SourceGravite.Toutes)
                g += source.Acceleration(corps.position);
            GraviteActuelle = g;

            corps.AddForce(g, ForceMode.Acceleration);

            if (alignerSurGravite && g.sqrMagnitude > 0.0001f)
            {
                Quaternion cible = Quaternion.FromToRotation(transform.up, Haut) * corps.rotation;
                corps.MoveRotation(Quaternion.RotateTowards(corps.rotation, cible, vitesseAlignement * Time.fixedDeltaTime));
            }
        }
    }
}
