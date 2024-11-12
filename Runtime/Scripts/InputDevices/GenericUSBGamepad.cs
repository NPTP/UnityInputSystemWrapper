using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace NPTP.InputSystemWrapper.InputDevices
{
    [InputControlLayout(stateType = typeof(GenericUSBGamepadInputReport))]
    public class GenericUSBGamepad : InputSystemWrapperGamepad
    {
        public static void Init() { }
        
        static GenericUSBGamepad()
        {
            InputSystem.RegisterLayout<GenericUSBGamepad>("Generic USB Gamepad",
                new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithProduct("Generic.+USB.+Joystick.*"));
        }
    }
}
