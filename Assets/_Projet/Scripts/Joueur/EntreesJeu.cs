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
#endif
    }
}
