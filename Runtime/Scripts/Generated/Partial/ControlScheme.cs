using System;
using UnityEngine.InputSystem;

// ReSharper disable once CheckNamespace
namespace NPTP.InputSystemWrapper.Enums
{
    public enum ControlScheme
    {
        /// <summary>
        /// Corresponds to "Null" string for newly created, unassigned players in Unity's PlayerInput.
        /// </summary>
        None = -1,
        
        // MARKER.Members.Start
        KeyboardMouse,
        Gamepad,
        Touch,
        Joystick,
        XR,
        // MARKER.Members.End
    }

    public static class PublicControlSchemeExtensions
    {
        public static bool IsMouseBased(this ControlScheme controlScheme)
        {
            return controlScheme switch
            {
                // MARKER.IsMouseBased.Start
                ControlScheme.KeyboardMouse => false,
                ControlScheme.Gamepad => false,
                ControlScheme.Touch => false,
                ControlScheme.Joystick => false,
                ControlScheme.XR => false,
                // MARKER.IsMouseBased.End
                _ => throw new ArgumentOutOfRangeException(nameof(controlScheme), controlScheme, null)
            };
        }
        
        public static bool IsGamepadBased(this ControlScheme controlScheme)
        {
            return controlScheme switch
            {
                // MARKER.IsGamepadBased.Start
                ControlScheme.KeyboardMouse => false,
                ControlScheme.Gamepad => false,
                ControlScheme.Touch => false,
                ControlScheme.Joystick => false,
                ControlScheme.XR => false,
                // MARKER.IsGamepadBased.End
                _ => throw new ArgumentOutOfRangeException(nameof(controlScheme), controlScheme, null)
            };
        }
    }

    internal static class InternalControlSchemeExtensions
    {
        internal static InputBinding ToBindingMask(this ControlScheme controlScheme)
        {
            return new InputBinding(groups: controlScheme.ToInputAssetName(), path: default);
        }
        
        /// <summary>
        /// Convert the enum to the string name in the asset from which the control scheme originates,
        /// so the string name can be used in the Input System API.
        /// </summary>
        internal static string ToInputAssetName(this ControlScheme controlSchemeEnum)
        {
            return controlSchemeEnum switch
            {
                // MARKER.EnumToStringSwitch.Start
                ControlScheme.KeyboardMouse => "Keyboard&Mouse",
                ControlScheme.Gamepad => "Gamepad",
                ControlScheme.Touch => "Touch",
                ControlScheme.Joystick => "Joystick",
                ControlScheme.XR => "XR",
                // MARKER.EnumToStringSwitch.End
                _ => throw new ArgumentOutOfRangeException(nameof(controlSchemeEnum), controlSchemeEnum, null)
            };
        }

        /// <summary>
        /// Try to convert the control scheme name from the input actions asset,
        /// used internally by Unity's input system, to its corresponding enum value.
        /// </summary>
        internal static ControlScheme ToControlSchemeEnum(this string controlSchemeName)
        {
            return controlSchemeName switch
            {
                // MARKER.StringToEnumSwitch.Start
                "Keyboard&Mouse" => ControlScheme.KeyboardMouse,
                "Gamepad" => ControlScheme.Gamepad,
                "Touch" => ControlScheme.Touch,
                "Joystick" => ControlScheme.Joystick,
                "XR" => ControlScheme.XR,
                // MARKER.StringToEnumSwitch.End
                _ => ControlScheme.None
            };
        }
    }
}
