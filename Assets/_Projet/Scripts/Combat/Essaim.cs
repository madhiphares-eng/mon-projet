using System.Collections.Generic;
using EchosEther.Jeu;
using UnityEngine;

namespace EchosEther.Combat
{
    /// <summary>
    /// L'essaim des Échardes : chaque gisement de cristaux a ses gardiennes, qui sortent du sable quand
    /// l'Arpenteur approche. De temps en temps, une embuscade surgit derrière lui (plus souvent s'il fait du bruit).
    /// Quand le quota est atteint, un Colosse d'Éther garde la balise. En formation, seules les créatures
    /// demandées par le didacticiel apparaissent.
    /// </summary>
    public class Essaim : MonoBehaviour
    {
        public static Essaim Instance { get; private set; }

        [Tooltip("Distance à laquelle les gardiennes d'un gisement sortent du sable.")]
        public float distanceReveil = 70f;
        [Tooltip("Secondes de calme au début de la mission.")]
        public float calmeInitial = 25f;
        public Vector2 intervalleEmbuscade = new Vector2(70f, 120f);
        public int maximum = 14;
        public bool colosseALaBalise = true;

        readonly HashSet<CristalEther> reveilles = new HashSet<CristalEther>();
        float prochaineEmbuscade = -1f, debut;
        bool colosse;

        void OnEnable() => Instance = this;
        void OnDisable() { if (Instance == this) Instance = null; }

        bool Actif
        {
            get
            {
                var jeu = GestionnaireJeu.Instance;
                return jeu != null && jeu.Etat == EtatJeu.Jeu && jeu.Type == TypePartie.Mission;
            }
        }

        void Update()
        {
            var jeu = GestionnaireJeu.Instance;
            if (!Actif || jeu.joueur == null) { debut = Time.time; return; }
            if (prochaineEmbuscade < 0f) { prochaineEmbuscade = Time.time + calmeInitial + Random.Range(intervalleEmbuscade.x, intervalleEmbuscade.y); }
            if (Time.time - debut < calmeInitial * 0.5f) return;
            Vector3 joueur = jeu.joueur.transform.position;

            foreach (var c in CristalEther.Tous)
            {
                if (reveilles.Contains(c) || Echarde.Toutes.Count >= maximum) continue;
                if (Vector3.Distance(c.transform.position, joueur) > distanceReveil) continue;
                reveilles.Add(c);
                Gardiennes(c);
            }

            if (Time.time >= prochaineEmbuscade && Echarde.Toutes.Count < maximum)
            {
                float bruit = jeu.jauge != null ? jeu.jauge.Bruit : 0f;
                prochaineEmbuscade = Time.time + Random.Range(intervalleEmbuscade.x, intervalleEmbuscade.y) * Mathf.Lerp(1f, 0.5f, bruit / 100f);
                Embuscade(joueur);
            }

            var mission = Mission.Instance;
            if (colosseALaBalise && !colosse && mission != null && mission.QuotaAtteint && mission.balise != null)
            {
                colosse = true;
                Vector3 b = mission.balise.transform.position;
                Vector3 dir = Vector3.ProjectOnPlane(Random.onUnitSphere, Planete.Haut(b)).normalized;
                Faire(TypeEcharde.Colosse, b + dir * 14f, b);
                Transmissions.Envoyer("Vaisseau-mère", "Les capteurs signalent une forme massive près de la balise. Le Bureau vous souhaite un excellent retour.", duree: 8f);
            }
        }

        void Gardiennes(CristalEther c)
        {
            Vector3 centre = c.transform.position;
            int n = Random.Range(1, 4);
            for (int i = 0; i < n; i++)
            {
                Vector3 dir = Vector3.ProjectOnPlane(Random.onUnitSphere, Planete.Haut(centre)).normalized;
                var type = i == 2 || Random.value < 0.22f ? TypeEcharde.Cracheuse : TypeEcharde.Rodeuse;
                Faire(type, centre + dir * Random.Range(6f, 11f), centre);
            }
        }

        void Embuscade(Vector3 joueur)
        {
            Vector3 haut = Planete.Haut(joueur);
            var cam = Camera.main;
            Vector3 derriere = cam != null ? -Vector3.ProjectOnPlane(cam.transform.forward, haut).normalized : Vector3.ProjectOnPlane(Random.onUnitSphere, haut).normalized;
            int n = Random.Range(2, 4);
            for (int i = 0; i < n; i++)
            {
                Vector3 dir = Quaternion.AngleAxis(Random.Range(-50f, 50f), haut) * derriere;
                var e = Faire(i == 0 && Random.value < 0.35f ? TypeEcharde.Cracheuse : TypeEcharde.Rodeuse, joueur + dir * Random.Range(13f, 20f), joueur);
                e.Revele = 6f;
            }
            Transmissions.Echo("Le sable remue", "Quelque chose sort du sol, derrière vous.", 4f);
        }

        /// <summary>Fait sortir une créature du sable (au sol sous <paramref name="position"/>).</summary>
        public static Echarde Faire(TypeEcharde type, Vector3 position, Vector3 nid)
        {
            Vector3 sol = Planete.Sol(position, out RaycastHit h) ? h.point : position;
            return Echarde.Creer(type, sol, nid);
        }
    }

    /// <summary>Cellule d'énergie lâchée par une créature : munitions et un peu d'intégrité au contact.</summary>
    public class CelluleEnergie : MonoBehaviour
    {
        static Material materiau;
        float age;
        Vector3 sol;

        public static void Deposer(Vector3 position)
        {
            Vector3 p = Planete.Sol(position, out RaycastHit h) ? h.point : position;
            var go = new GameObject("Cellule d'énergie");
            go.transform.position = p;
            if (materiau == null) materiau = Formes.Lueur(new Color(0.5f, 1.8f, 2.6f), 1.2f, 0.5f);
            var noyau = Formes.Primitive(PrimitiveType.Capsule, go.transform, materiau, "Noyau").transform;
            noyau.localScale = new Vector3(0.18f, 0.16f, 0.18f);
            var c = go.AddComponent<CelluleEnergie>();
            c.sol = p;
        }

        void Update()
        {
            age += Time.deltaTime;
            Vector3 haut = Planete.Haut(sol);
            transform.SetPositionAndRotation(sol + haut * (0.45f + Mathf.Sin(age * 3f) * 0.1f), Quaternion.AngleAxis(age * 90f, haut) * Quaternion.FromToRotation(Vector3.up, haut));
            var jeu = GestionnaireJeu.Instance;
            if (jeu != null && jeu.Etat == EtatJeu.Jeu && jeu.joueur != null && Vector3.Distance(jeu.joueur.transform.position, transform.position) < 1.9f)
            {
                Arsenal.Instance?.AjouterMunitions(0.2f);
                jeu.joueur.GetComponent<VieArpenteur>()?.Soigner(8f);
                EffetsCombat.Etincelle(transform.position, new Color(0.5f, 1.8f, 2.6f), 0.4f);
                BanqueSons.Jouer(BanqueSons.Instance?.ramassage, 0.6f);
                Destroy(gameObject);
                return;
            }
            if (age > 60f) Destroy(gameObject);
        }
    }
}
