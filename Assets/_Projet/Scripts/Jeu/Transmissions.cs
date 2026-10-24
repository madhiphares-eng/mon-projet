using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchosEther.Jeu
{
    /// <summary>
    /// Messages affichés en bas à gauche (Empire, instructrice, alertes), échos poétiques au centre,
    /// et journal de bord (touche J). Les messages de l'Empire arrivent selon le temps du vaisseau-mère.
    /// </summary>
    public class Transmissions : MonoBehaviour
    {
        public readonly struct Message
        {
            public readonly string titre, texte;
            public readonly bool personnel;
            public readonly float duree;
            public Message(string titre, string texte, bool personnel, float duree)
            {
                this.titre = titre; this.texte = texte; this.personnel = personnel; this.duree = duree;
            }
        }

        public readonly struct EntreeJournal
        {
            public readonly string titre, texte;
            public readonly bool echo;
            public EntreeJournal(string titre, string texte, bool echo) { this.titre = titre; this.texte = texte; this.echo = echo; }
        }

        public static event Action<Message> MessageRecu;
        public static event Action<string, float> EchoRecu;
        public static event Action JournalModifie;
        public static readonly List<EntreeJournal> Journal = new List<EntreeJournal>();

        // Envoyées en temps du vaisseau (jours). {nom} est remplacé par le nom de l'Arpenteur.
        static readonly (float jours, string texte, bool personnel)[] Empire =
        {
            (0.3f, "Arpenteur {nom} ! Le Bureau du Quota vous rappelle : cinq cristaux d'Éther, ni plus ni moins. Toute curiosité sera déduite de votre solde.", false),
            (1.5f, "Rappel amical : le bruit attire la faune locale. La faune locale n'est pas couverte par votre assurance.", false),
            (3f, "Félicitations ! Votre unité a été élue « Arpenteur du mois ». Votre prime sera versée à votre retour, ou à vos héritiers.", false),
            (6f, "Le Bureau constate un retard. Nous vous rappelons que le temps de l'Empire est précieux. Le vôtre l'est moins.", false),
            (10f, "Votre sœur Élan demande de vos nouvelles. Nous lui avons répondu que vous étiez en pleine forme.", false),
            (16f, "Changement d'administration. Le nouveau Bureau ne retrouve aucune trace de votre mission. Veuillez confirmer votre existence.", false),
            (25f, "« Si tu reçois ça… le ciel ici a changé de couleur. Les enfants demandent quand tu reviens. Reviens vite. — É. »", true),
            (40f, "AVIS : les réserves d'Éther du vaisseau diminuent. Les sauts deviennent… étranges. Certains équipages reviennent avant d'être partis.", false),
            (65f, "Nous captons des échos de votre voix sur des fréquences anciennes. Vous nous parlez depuis des années que nous n'avons pas encore vécues.", false),
            (100f, "Transmission automatique. Ce vaisseau est en orbite stationnaire. Il n'y a plus personne pour lire votre rapport. Gloire à l'Empire.", false),
        };

        public static int NombreEmpire => Empire.Length;
        public int Recues { get; private set; }
        bool messagesEmpire;

        /// <summary>Nouvelle partie : vide le journal ; les messages de l'Empire seulement en mission.</summary>
        public void Demarrer(bool avecMessagesEmpire)
        {
            Journal.Clear();
            JournalModifie?.Invoke();
            Recues = 0;
            messagesEmpire = avecMessagesEmpire;
        }

        public static void Envoyer(string titre, string texte, bool personnel = false, float duree = 11f, bool noter = true)
        {
            MessageRecu?.Invoke(new Message(titre, texte, personnel, duree));
            BanqueSons.Jouer(personnel ? BanqueSons.Instance?.messagePersonnel : BanqueSons.Instance?.transmission, 0.6f);
            if (noter) Noter(titre, texte, false);
        }

        /// <summary>Écho : phrase poétique au centre de l'écran, notée au journal.</summary>
        public static void Echo(string titre, string texte, float duree = 8f)
        {
            EchoRecu?.Invoke(texte, duree);
            Noter(titre, texte, true);
        }

        public static void Noter(string titre, string texte, bool echo)
        {
            Journal.Insert(0, new EntreeJournal(titre, texte, echo));
            JournalModifie?.Invoke();
        }

        public static bool DejaNote(string titre)
        {
            foreach (var e in Journal) if (e.titre == titre) return true;
            return false;
        }

        void Update()
        {
            var horloges = Horloges.Instance;
            if (!messagesEmpire || horloges == null) return;
            while (Recues < Empire.Length && horloges.TempsVaisseau >= Empire[Recues].jours * 86400.0)
            {
                var m = Empire[Recues];
                string titre = $"{(m.personnel ? "Message personnel" : "Transmission impériale")} · émise {Horloges.FormatVaisseau(m.jours * 86400.0)}";
                Envoyer(titre, ProfilArpenteur.Actuel.Personnaliser(m.texte), m.personnel);
                Recues++;
            }
        }
    }
}
