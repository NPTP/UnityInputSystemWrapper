using UnityEngine.InputSystem.Layouts;

namespace NPTP.InputSystemWrapper.CustomSetups.Devices
{
    public sealed class GenericUSBGamepadAsset : CustomLayout<GenericUSBGamepad>
    {
        protected override string Name => "Generic USB Gamepad";
        protected override InputDeviceMatcher Matches => new InputDeviceMatcher()
            .WithInterface("HID")
            .WithProduct("Generic.+USB.+Joystick.*");
    }
}
