using NPTP.InputSystemWrapper.InputDevices.Layouts;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace NPTP.InputSystemWrapper.InputDevices.Devices
{
    [InputControlLayout(stateType = typeof(GenericUSBGamepadLayout))]
    public class GenericUSBGamepad : Gamepad
    {
        internal static void Register()
        {
            InputSystem.RegisterLayout<GenericUSBGamepad>(
                "Generic USB Gamepad",
                new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithProduct("Generic.+USB.+Joystick.*"));
        }
    }
}