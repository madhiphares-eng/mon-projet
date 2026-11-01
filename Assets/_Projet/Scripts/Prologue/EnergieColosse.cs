using System;
using EchosEther.Joueur;
using UnityEngine;

namespace EchosEther.Prologue
{
    /// <summary>L'énergie du Colosse : tout marche ; l'alarme (l'attaque des pirates, l'évasion) ; la panne (le cœur touché).</summary>
    public enum Energie { Normal, Alarme, Panne }

    /// <summary>
    /// L'état d'énergie de tout le vaisseau. Les objets animés (<see cref="ObjetAnime"/>) le suivent : portes verrouillées
    /// en alarme, bloquées à demi en panne ; lumières rouges qui battent, ou qui grésillent ; machines qui s'emballent, ou
    /// qui ralentissent. Le scénario du prologue le change avec <see cref="Changer"/> ; pour l'essayer, la touche F8 le fait
    /// tourner (composant <see cref="EnergieColosseEssai"/>).
    /// </summary>
    public static class EnergieColosse
    {
        public static Energie Etat { get; private set; } = Energie.Normal;
        public static event Action<Energie> Change;

        public static void Changer(Energie e)
        {
            if (e == Etat) return;
            Etat = e;
            Change?.Invoke(e);
        }
    }

    /// <summary>Pour essayer : F8 fait passer le Colosse de normal à alarme, puis à panne, puis de nouveau à normal.</summary>
    public class EnergieColosseEssai : MonoBehaviour
    {
        public Energie depart = Energie.Normal;

        void Start() => EnergieColosse.Changer(depart);

        void Update()
        {
            if (!EntreesJeu.ChangerEnergie()) return;
            EnergieColosse.Changer((Energie)(((int)EnergieColosse.Etat + 1) % 3));
            Debug.Log($"Énergie du Colosse : {EnergieColosse.Etat}");
        }
    }
}
