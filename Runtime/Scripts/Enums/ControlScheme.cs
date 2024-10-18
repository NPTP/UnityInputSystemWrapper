using System;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Enums
{
    public enum ControlScheme
    {
        // MARKER.Members.Start
        KeyboardMouse,
        Gamepad,
        Touch,
        Joystick,
        XR,
        // MARKER.Members.End
    }

    internal static class ControlSchemeExtensions
    {
        /// <summary>
        /// Convert the enum to the string name in the asset from which the control scheme originates,
        /// so the string name can be used in the Input System API.
        /// </summary>
        internal static string ToInputAssetName(this ControlScheme controlSchemeEnum)
        {
            return controlSchemeEnum switch
            {
                // MARKER.EnumToStringSwitch.Start
                ControlScheme.KeyboardMouse => "Keyboard Mouse",
                ControlScheme.Gamepad => "Gamepad",
                ControlScheme.Touch => "Touch",
                ControlScheme.Joystick => "Joystick",
                ControlScheme.XR => "XR",
                // MARKER.EnumToStringSwitch.End
                _ => throw new ArgumentOutOfRangeException(nameof(controlSchemeEnum), controlSchemeEnum, null)
            };
        }

        /// <summary>
        /// Convert the control scheme asset name to the corresponding enum value.
        /// </summary>
        internal static ControlScheme ToControlSchemeEnum(this string controlSchemeName)
        {
            return controlSchemeName switch
            {
                // MARKER.StringToEnumSwitch.Start
                "Keyboard Mouse" => ControlScheme.KeyboardMouse,
                "Gamepad" => ControlScheme.Gamepad,
                "Touch" => ControlScheme.Touch,
                "Joystick" => ControlScheme.Joystick,
                "XR" => ControlScheme.XR,
                // MARKER.StringToEnumSwitch.End
                _ => throw new ArgumentOutOfRangeException(nameof(controlSchemeName), controlSchemeName, null)
            };
        }

        internal static InputBinding ToBindingMask(this ControlScheme controlScheme)
        {
            return new InputBinding(groups: controlScheme.ToInputAssetName(), path: default);
        }
    }
}
