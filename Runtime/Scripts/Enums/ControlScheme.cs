using System;

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

    public static class ControlSchemeExtensions
    {
        /// <summary>
        /// Convert the enum to the string name in the asset from which the control scheme originates,
        /// so the string name can be used in the Input System API.
        /// </summary>
        public static string ToNameInInputActionAsset(this ControlScheme controlSchemeEnum)
        {
            return controlSchemeEnum switch
            {
                // MARKER.EnumToStringSwitch.Start
                ControlScheme.KeyboardMouse => "KeyboardMouse",
                ControlScheme.Gamepad => "KeyboardMouse",
                ControlScheme.Touch => "KeyboardMouse",
                ControlScheme.Joystick => "KeyboardMouse",
                ControlScheme.XR => "KeyboardMouse",
                // MARKER.EnumToStringSwitch.End
                _ => throw new ArgumentOutOfRangeException(nameof(controlSchemeEnum), controlSchemeEnum, null)
            };
        }

        /// <summary>
        /// Convert the control scheme asset name to the corresponding enum value.
        /// </summary>
        public static ControlScheme ToControlSchemeEnum(this string controlSchemeName)
        {
            return controlSchemeName switch
            {
                // MARKER.StringToEnumSwitch.Start
                "KeyboardMouse" => ControlScheme.KeyboardMouse,
                "Gamepad" => ControlScheme.Gamepad,
                _ => throw new ArgumentOutOfRangeException(nameof(controlSchemeName), controlSchemeName, null)
                // MARKER.StringToEnumSwitch.End
            };
        }
    }
}
