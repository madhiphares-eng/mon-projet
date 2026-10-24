using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EchosEther.Joueur
{
    /// <summary>
    /// Lecture clavier/souris compatible avec le nouveau Input System et l'ancien Input Manager.
    /// Avec le nouveau système, les touches suivent leur position physique :
    /// ZQSD sur clavier AZERTY = WASD sur clavier QWERTY. La manette marche aussi (nouveau système) :
    /// stick gauche pour avancer, stick droit pour regarder, A pour sauter, L3 pour courir.
    /// </summary>
    public static class Entrees
    {
        /// <summary>Vitesse du regard au stick droit poussé à fond (unités de <see cref="Regard"/> par seconde).</summary>
        public static float VitesseRegardManette = 80f;

        public static Vector2 Deplacement()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 v = Vector2.zero;
            var k = Keyboard.current;
            if (k != null)
            {
                v.x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1f : 0f) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1f : 0f);
                v.y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1f : 0f) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1f : 0f);
            }
            if (Gamepad.current != null) v += Gamepad.current.leftStick.ReadValue();
            return Vector2.ClampMagnitude(v, 1f);
#else
            return Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
#endif
        }

        /// <summary>Mouvement de la souris depuis la dernière image.</summary>
        public static Vector2 Regard()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 r = Mouse.current != null ? Mouse.current.delta.ReadValue() * 0.1f : Vector2.zero;
            if (Gamepad.current != null) r += Gamepad.current.rightStick.ReadValue() * (VitesseRegardManette * Time.deltaTime);
            return r;
#else
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
        }

        public static bool Saut()
        {
#if ENABLE_INPUT_SYSTEM
            return (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.Space);
#endif
        }

        public static bool Course()
        {
#if ENABLE_INPUT_SYSTEM
            return (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
                || (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed);
#else
            return Input.GetKey(KeyCode.LeftShift);
#endif
        }

        public static bool LibererCurseur()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        public static bool CapturerCurseur()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }
    }
}
