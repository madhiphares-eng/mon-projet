using System.Collections.Generic;
using EchosEther.Jeu;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Combat
{
    /// <summary>
    /// Projectile visible qui avance en ligne droite (éclair du pistolet Ardent, crachat d'une créature).
    /// Les collisions sont testées par rayons à chaque image : pas de Rigidbody, pas de traversée à grande vitesse.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        static readonly Dictionary<Color, Material> materiaux = new Dictionary<Color, Material>();
        static readonly RaycastHit[] touches = new RaycastHit[16];

        Vector3 vitesse;
        float degats, portee, parcouru, recul, ralenti, gravite;
        Color couleur;
        bool parJoueur;
        Transform ignorer;

        /// <summary>Tire un projectile. <paramref name="ignorer"/> : le tireur (ses colliders ne bloquent pas).</summary>
        public static Projectile Tirer(Vector3 depart, Vector3 direction, float vitesse, float degats, float portee, Color couleur,
            bool parJoueur, Transform ignorer, float recul = 2f, float ralenti = 0f, float taille = 1f, float gravite = 0f)
        {
            var go = new GameObject(parJoueur ? "Eclair" : "Crachat");
            go.transform.SetPositionAndRotation(depart, Quaternion.LookRotation(direction));
            if (!materiaux.TryGetValue(couleur, out var m) || m == null) materiaux[couleur] = m = Formes.Lueur(couleur, 0f, 1f);
            var corps = Formes.Primitive(PrimitiveType.Sphere, go.transform, m, "Corps").transform;
            corps.localScale = new Vector3(0.11f, 0.11f, 0.8f) * taille;
            var halo = Formes.Primitive(PrimitiveType.Sphere, go.transform, m, "Halo").transform;
            halo.localScale = Vector3.one * 0.28f * taille;
            var p = go.AddComponent<Projectile>();
            p.vitesse = direction.normalized * vitesse;
            p.degats = degats; p.portee = portee; p.couleur = couleur; p.parJoueur = parJoueur;
            p.ignorer = ignorer; p.recul = recul; p.ralenti = ralenti; p.gravite = gravite;
            return p;
        }

        void Update()
        {
            float dt = Time.deltaTime * BulleTemporelle.Ecoulement(transform.position);
            if (gravite > 0f) vitesse += Planete.Gravite(transform.position) * (gravite * dt);
            Vector3 pas = vitesse * dt;
            float l = pas.magnitude;
            if (l < 1e-5f) return;
            Vector3 dir = pas / l;
            int n = Physics.RaycastNonAlloc(transform.position, dir, touches, l, ~0, QueryTriggerInteraction.Ignore);
            int meilleur = -1;
            for (int i = 0; i < n; i++)
            {
                if (touches[i].transform.IsChildOf(transform)) continue;
                if (ignorer != null && touches[i].transform.IsChildOf(ignorer)) continue;
                if (meilleur < 0 || touches[i].distance < touches[meilleur].distance) meilleur = i;
            }
            if (meilleur >= 0) { Toucher(touches[meilleur], dir); return; }
            transform.position += pas;
            transform.rotation = Quaternion.LookRotation(dir);
            parcouru += l;
            if (parcouru > portee) { EffetsCombat.Etincelle(transform.position, couleur, 0.2f); Destroy(gameObject); }
        }

        void Toucher(RaycastHit h, Vector3 dir)
        {
            var cible = Combat.De(h.collider);
            if (cible != null) Combat.Frapper(cible, new Degats(degats, h.point, dir, recul, parJoueur, ralenti));
            EffetsCombat.Impact(h.point, h.normal, couleur, cible != null ? 10 : 6);
            BanqueSons.JouerA(BanqueSons.Instance?.impactArme, h.point, 0.5f);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Grenade lancée en cloche : rebondit, clignote de plus en plus vite, explose, et son cri appelle la Ruche
    /// à cet endroit (grenade leurre Écho). Trajectoire calculée à la main, avec la gravité de la planète.
    /// </summary>
    public class Grenade : MonoBehaviour
    {
        static readonly RaycastHit[] touches = new RaycastHit[16];

        Vector3 vitesse;
        float minuterie, degats, rayon, recul;
        Color couleur;
        Transform ignorer, lueur;
        Material materiauLueur;
        bool posee;
        float rotation;

        public static Grenade Lancer(Vector3 depart, Vector3 vitesse, DefinitionArme def, GameObject modele, Transform ignorer, float bonusRayon = 1f)
        {
            var go = new GameObject("Grenade Écho");
            go.transform.position = depart;
            var g = go.AddComponent<Grenade>();
            g.vitesse = vitesse;
            g.minuterie = 2.1f;
            g.degats = def.degats;
            g.rayon = def.rayonExplosion * bonusRayon;
            g.recul = def.reculCible;
            g.couleur = def.couleur;
            g.ignorer = ignorer;
            if (modele != null)
            {
                var m = Instantiate(modele, go.transform);
                m.transform.localPosition = Vector3.zero;
                m.transform.localScale = Vector3.one * 0.75f;
                foreach (var c in m.GetComponentsInChildren<Collider>()) Destroy(c);
            }
            else
            {
                var coque = Formes.Primitive(PrimitiveType.Sphere, go.transform, Formes.Opaque(new Color(0.2f, 0.22f, 0.25f), 0.7f, 0.5f), "Coque").transform;
                coque.localScale = Vector3.one * 0.14f;
            }
            g.materiauLueur = Formes.Lueur(def.couleur, 0f, 1f);
            g.lueur = Formes.Primitive(PrimitiveType.Sphere, go.transform, g.materiauLueur, "Voyant").transform;
            g.lueur.localScale = Vector3.one * 0.09f;
            g.lueur.localPosition = Vector3.up * 0.08f;
            return g;
        }

        void Update()
        {
            float dt = Time.deltaTime * BulleTemporelle.Ecoulement(transform.position);
            minuterie -= dt;
            float clignote = Mathf.Repeat(minuterie * (minuterie < 0.8f ? 9f : 3f), 1f) < 0.5f ? 2.5f : 0.3f;
            materiauLueur.SetFloat("_Intensite", clignote);
            if (minuterie <= 0f) { Exploser(); return; }
            if (posee) return;

            Vector3 haut = Planete.Haut(transform.position);
            vitesse += Planete.Gravite(transform.position) * dt;
            Vector3 pas = vitesse * dt;
            float l = pas.magnitude;
            if (l > 1e-5f)
            {
                int n = Physics.RaycastNonAlloc(transform.position, pas / l, touches, l + 0.08f, ~0, QueryTriggerInteraction.Ignore);
                int meilleur = -1;
                for (int i = 0; i < n; i++)
                {
                    if (touches[i].transform.IsChildOf(transform)) continue;
                if (ignorer != null && touches[i].transform.IsChildOf(ignorer)) continue;
                    if (meilleur < 0 || touches[i].distance < touches[meilleur].distance) meilleur = i;
                }
                if (meilleur >= 0)
                {
                    var h = touches[meilleur];
                    transform.position = h.point + h.normal * 0.08f;
                    vitesse = Vector3.Reflect(vitesse, h.normal) * 0.42f;
                    vitesse -= Vector3.Project(vitesse, h.normal) * 0.2f;
                    if (vitesse.magnitude > 1.5f) BanqueSons.JouerA(BanqueSons.Instance?.rebond, h.point, 0.4f);
                    if (vitesse.magnitude < 1.2f && Vector3.Dot(h.normal, haut) > 0.6f) posee = true;
                }
                else transform.position += pas;
            }
            rotation += dt * vitesse.magnitude * 120f;
            transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(vitesse.sqrMagnitude > 0.01f ? vitesse : transform.forward, haut) + haut * 0.001f, haut)
                * Quaternion.Euler(rotation, 0f, 0f);
        }

        void Exploser()
        {
            Vector3 p = transform.position;
            EffetsCombat.Explosion(p, rayon, couleur);
            BanqueSons.JouerA(BanqueSons.Instance?.explosion, p, 1f);
            Combat.Zone(p, rayon, degats, recul);
            // l'Arpenteur n'est pas immunisé à ses propres grenades (dégâts réduits)
            var jeu = GestionnaireJeu.Instance;
            if (jeu != null && jeu.joueur != null)
            {
                var vie = jeu.joueur.GetComponent<VieArpenteur>();
                float d = Vector3.Distance(jeu.joueur.transform.position, p);
                if (vie != null && d < rayon) vie.Subir(new Degats(degats * 0.4f * (1f - d / rayon), p, (jeu.joueur.transform.position - p).normalized, 6f, false));
            }
            JaugeBruit.Signaler(p, 60f);
            bool ver = JaugeBruit.Leurre(p);
            if (ver) Transmissions.Echo("Leurre", "Le sable a entendu. Il vient là où la grenade a crié.", 4f);
            Destroy(gameObject);
        }
    }
}
