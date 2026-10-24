using EchosEther.Joueur;
using EchosEther.Physique;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Outils de placement sur la planète principale (la <see cref="SourceGravite"/> la plus grande) :
    /// "haut" local, sol sous un point, directions le long de la surface.
    /// </summary>
    public static class Planete
    {
        static SourceGravite principale;

        public static SourceGravite Principale
        {
            get
            {
                if (principale != null && principale.isActiveAndEnabled) return principale;
                principale = null;
                foreach (var s in SourceGravite.Toutes)
                    if (principale == null || s.rayonSurface > principale.rayonSurface) principale = s;
                if (principale == null) principale = Object.FindFirstObjectByType<SourceGravite>();
                return principale;
            }
        }

        public static Vector3 Centre => Principale != null ? Principale.transform.position : Vector3.down * 1e6f;
        public static float Rayon => Principale != null ? Principale.rayonSurface : 500f;

        /// <summary>Verticale locale (opposée à la gravité de la planète).</summary>
        public static Vector3 Haut(Vector3 position)
        {
            if (Principale == null) return Vector3.up;
            Vector3 d = position - Principale.transform.position;
            return d.sqrMagnitude > 1e-6f ? d.normalized : Vector3.up;
        }

        /// <summary>
        /// Sol sous un point (rayon vers le centre de la planète). Ignore les déclencheurs, le joueur
        /// et, si fourni, l'objet <paramref name="ignorer"/> et ses enfants.
        /// </summary>
        public static bool Sol(Vector3 depuis, out RaycastHit sol, Transform ignorer = null, float hauteurDepart = 60f, float portee = 400f)
        {
            Vector3 haut = Haut(depuis);
            Vector3 origine = depuis + haut * hauteurDepart;
            // appelé plusieurs fois par image : tampon réutilisé, pas d'allocation
            int n = Physics.RaycastNonAlloc(origine, -haut, touches, hauteurDepart + portee, ~0, QueryTriggerInteraction.Ignore);
            int meilleur = -1;
            for (int i = 0; i < n; i++)
            {
                var t = touches[i];
                if (meilleur >= 0 && t.distance >= touches[meilleur].distance) continue;
                if (ignorer != null && t.transform.IsChildOf(ignorer)) continue;
                if (t.collider.GetComponentInParent<ControleurArpenteur>() != null) continue;
                meilleur = i;
            }
            sol = meilleur >= 0 ? touches[meilleur] : default;
            return meilleur >= 0;
        }

        static readonly RaycastHit[] touches = new RaycastHit[32];

        /// <summary>Point du sol dans une direction donnée depuis le centre de la planète.</summary>
        public static Vector3 PointSurface(Vector3 direction, Transform ignorer = null)
        {
            Vector3 d = direction.normalized;
            Vector3 approx = Centre + d * Rayon;
            return Sol(approx, out RaycastHit sol, ignorer, 300f, 600f) ? sol.point : approx;
        }

        /// <summary>
        /// Direction (depuis le centre) d'un point situé à <paramref name="distance"/> mètres de <paramref name="depart"/>
        /// le long de la surface, dans la direction <paramref name="angle"/> (degrés) par rapport à <paramref name="avant"/>.
        /// </summary>
        public static Vector3 Direction(Vector3 depart, Vector3 avant, float distance, float angle)
        {
            Vector3 haut = Haut(depart);
            Vector3 t0 = Vector3.ProjectOnPlane(avant, haut).normalized;
            if (t0.sqrMagnitude < 0.01f) t0 = Vector3.ProjectOnPlane(Vector3.forward, haut).normalized;
            Vector3 tangente = Quaternion.AngleAxis(angle, haut) * t0;
            Vector3 axe = Vector3.Cross(haut, tangente);
            float degres = distance / Mathf.Max(1f, Rayon) * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(degres, axe) * haut;
        }

        /// <summary>Rotation "debout" sur la planète en un point, tournée vers <paramref name="avant"/>.</summary>
        public static Quaternion Debout(Vector3 position, Vector3 avant)
        {
            Vector3 haut = Haut(position);
            Vector3 a = Vector3.ProjectOnPlane(avant, haut);
            if (a.sqrMagnitude < 1e-4f) a = Vector3.ProjectOnPlane(Vector3.forward, haut);
            if (a.sqrMagnitude < 1e-4f) a = Vector3.ProjectOnPlane(Vector3.right, haut);
            return Quaternion.LookRotation(a.normalized, haut);
        }

        /// <summary>Dénivelé maximal (m) du sol dans un cercle de <paramref name="rayon"/> autour d'un point.</summary>
        public static float Denivele(Vector3 point, float rayon, Transform ignorer = null)
        {
            Vector3 haut = Haut(point);
            if (!Sol(point, out RaycastHit centre, ignorer)) return 0f;
            Vector3 e1 = Vector3.ProjectOnPlane(Vector3.right, haut).normalized;
            if (e1.sqrMagnitude < 0.01f) e1 = Vector3.ProjectOnPlane(Vector3.forward, haut).normalized;
            Vector3 e2 = Vector3.Cross(haut, e1);
            float max = 0f;
            for (int k = 0; k < 8; k++)
            {
                float a = k * Mathf.PI / 4f;
                Vector3 p = centre.point + (e1 * Mathf.Cos(a) + e2 * Mathf.Sin(a)) * rayon;
                if (Sol(p, out RaycastHit h, ignorer))
                    max = Mathf.Max(max, Mathf.Abs(Vector3.Dot(h.point - centre.point, haut)));
            }
            return max;
        }
    }
}
