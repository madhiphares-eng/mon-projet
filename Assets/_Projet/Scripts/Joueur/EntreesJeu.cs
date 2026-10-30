using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace EchosEther.Joueur
{
    /// <summary>
    /// Touches d'action du jeu (complète <see cref="Entrees"/>). Positions physiques avec le nouveau
    /// Input System : sur un clavier AZERTY, E, F, C, N et J sont aux mêmes places qu'en QWERTY.
    /// M (son) est cherché par son nom sur la disposition actuelle.
    /// Manette (nouveau système) : X interagir, Y scanner, B silence, Start pause, Select journal,
    /// croix droite passer une étape ; en navette A/RB monter, B/LB descendre, gâchette droite boost.
    /// Combat : clic gauche / gâchette droite tirer, clic droit / gâchette gauche viser, R / croix gauche recharger,
    /// 1 à 6, molette ou croix haut/bas pour les armes, G / LB grenade, V / R3 coup, A en AZERTY (Q en QWERTY) / RB compétence,
    /// K talents.
    /// Prologue : C s'accroupir (manette B), X changer d'épaule (croix droite), V vue cockpit ou poursuite (croix haut),
    /// 1 sortir ou ranger le fusil.
    /// </summary>
    public static class EntreesJeu
    {
#if ENABLE_INPUT_SYSTEM
        static Keyboard K => Keyboard.current;
        static Gamepad P => Gamepad.current;
        static bool Tenue(ButtonControl t) => t != null && t.isPressed;
        static bool Pressee(ButtonControl t) => t != null && t.wasPressedThisFrame;

        public static bool InteragirMaintenu() => (K != null && Tenue(K.eKey)) || (P != null && Tenue(P.buttonWest));
        public static bool Interagir() => (K != null && Pressee(K.eKey)) || (P != null && Pressee(P.buttonWest));
        public static bool Scanner() => (K != null && Pressee(K.fKey)) || (P != null && Pressee(P.buttonNorth));
        public static bool Silence() => (K != null && Tenue(K.cKey)) || (P != null && Tenue(P.buttonEast));
        public static bool PasserEtape() => (K != null && Pressee(K.nKey)) || (P != null && Pressee(P.dpad.right));
        public static bool Journal() => (K != null && Pressee(K.jKey)) || (P != null && Pressee(P.selectButton));
        public static bool Son() => K != null && Pressee(K.FindKeyOnCurrentKeyboardLayout("m"));
        public static bool Pause() => (K != null && Pressee(K.escapeKey)) || (P != null && Pressee(P.startButton));
        public static bool Monter() => (K != null && Tenue(K.spaceKey)) || (P != null && (Tenue(P.buttonSouth) || Tenue(P.rightShoulder)));
        public static bool Descendre() => (K != null && (Tenue(K.cKey) || Tenue(K.leftCtrlKey))) || (P != null && (Tenue(P.buttonEast) || Tenue(P.leftShoulder)));
        public static bool Boost() => (K != null && Tenue(K.leftShiftKey)) || (P != null && (Tenue(P.rightTrigger) || Tenue(P.leftStickButton)));
        public static bool Valider() => (K != null && (Pressee(K.enterKey) || Pressee(K.spaceKey))) || (P != null && Pressee(P.buttonSouth));

        static Mouse S => Mouse.current;
        public static bool Tirer() => (S != null && S.leftButton.isPressed) || (P != null && Tenue(P.rightTrigger));
        public static bool TirerAppui() => (S != null && S.leftButton.wasPressedThisFrame) || (P != null && Pressee(P.rightTrigger));
        public static bool Viser() => (S != null && S.rightButton.isPressed) || (P != null && Tenue(P.leftTrigger));
        public static bool Recharger() => (K != null && Pressee(K.rKey)) || (P != null && Pressee(P.dpad.left));
        public static bool Melee() => (K != null && Pressee(K.vKey)) || (P != null && Pressee(P.rightStickButton));
        public static bool Grenade() => (K != null && Pressee(K.gKey)) || (P != null && Pressee(P.leftShoulder));
        public static bool Competence() => (K != null && Pressee(K.qKey)) || (P != null && Pressee(P.rightShoulder));
        public static bool Talents() => K != null && Pressee(K.kKey);

        // le prologue (à pied dans le Colosse, puis en navette)
        public static bool Accroupir() => (K != null && Pressee(K.cKey)) || (P != null && Pressee(P.buttonEast));
        public static bool ChangerEpaule() => (K != null && Pressee(K.xKey)) || (P != null && Pressee(P.dpad.right));
        public static bool ChangerVue() => (K != null && Pressee(K.vKey)) || (P != null && Pressee(P.dpad.up));

        /// <summary>Arme choisie au clavier (touches 1 à 6 de la rangée du haut), -1 sinon.</summary>
        public static int ChoixArme()
        {
            if (K == null) return -1;
            if (Pressee(K.digit1Key)) return 0;
            if (Pressee(K.digit2Key)) return 1;
            if (Pressee(K.digit3Key)) return 2;
            if (Pressee(K.digit4Key)) return 3;
            if (Pressee(K.digit5Key)) return 4;
            if (Pressee(K.digit6Key)) return 5;
            return -1;
        }

        /// <summary>Arme suivante (+1) ou précédente (-1) : molette ou croix haut/bas.</summary>
        public static int DefilementArme()
        {
            float molette = S != null ? S.scroll.ReadValue().y : 0f;
            if (molette > 0.1f || (P != null && Pressee(P.dpad.up))) return -1;
            if (molette < -0.1f || (P != null && Pressee(P.dpad.down))) return 1;
            return 0;
        }

        /// <summary>Nom de la touche sur le clavier actuel (la compétence est "A" en AZERTY, "Q" en QWERTY).</summary>
        public static string NomTouche(string action)
        {
            KeyControl t = null;
            if (K != null)
                switch (action)
                {
                    case "competence": t = K.qKey; break;
                    case "talents": t = K.kKey; break;
                    case "grenade": t = K.gKey; break;
                    case "melee": t = K.vKey; break;
                    case "recharger": t = K.rKey; break;
                }
            if (t != null && !string.IsNullOrEmpty(t.displayName)) return t.displayName.ToUpperInvariant();
            return action == "competence" ? "Q" : action == "talents" ? "K" : action == "grenade" ? "G" : action == "melee" ? "V" : "R";
        }
#else
        public static bool InteragirMaintenu() => Input.GetKey(KeyCode.E);
        public static bool Interagir() => Input.GetKeyDown(KeyCode.E);
        public static bool Scanner() => Input.GetKeyDown(KeyCode.F);
        public static bool Silence() => Input.GetKey(KeyCode.C);
        public static bool PasserEtape() => Input.GetKeyDown(KeyCode.N);
        public static bool Journal() => Input.GetKeyDown(KeyCode.J);
        public static bool Son() => Input.GetKeyDown(KeyCode.M);
        public static bool Pause() => Input.GetKeyDown(KeyCode.Escape);
        public static bool Monter() => Input.GetKey(KeyCode.Space);
        public static bool Descendre() => Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl);
        public static bool Boost() => Input.GetKey(KeyCode.LeftShift);
        public static bool Valider() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);

        public static bool Tirer() => Input.GetMouseButton(0);
        public static bool TirerAppui() => Input.GetMouseButtonDown(0);
        public static bool Viser() => Input.GetMouseButton(1);
        public static bool Recharger() => Input.GetKeyDown(KeyCode.R);
        public static bool Melee() => Input.GetKeyDown(KeyCode.V);
        public static bool Grenade() => Input.GetKeyDown(KeyCode.G);
        public static bool Competence() => Input.GetKeyDown(KeyCode.Q);
        public static bool Talents() => Input.GetKeyDown(KeyCode.K);
        public static bool Accroupir() => Input.GetKeyDown(KeyCode.C);
        public static bool ChangerEpaule() => Input.GetKeyDown(KeyCode.X);
        public static bool ChangerVue() => Input.GetKeyDown(KeyCode.V);

        public static int ChoixArme()
        {
            for (int i = 0; i < 6; i++) if (Input.GetKeyDown(KeyCode.Alpha1 + i)) return i;
            return -1;
        }

        public static int DefilementArme()
        {
            float molette = Input.mouseScrollDelta.y;
            return molette > 0.1f ? -1 : molette < -0.1f ? 1 : 0;
        }

        public static string NomTouche(string action) =>
            action == "competence" ? "Q" : action == "talents" ? "K" : action == "grenade" ? "G" : action == "melee" ? "V" : "R";
#endif
    }
}
