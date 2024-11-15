using NPTP.InputSystemWrapper.InputDevices.Layouts;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace NPTP.InputSystemWrapper.InputDevices.Devices
{
    [InputControlLayout(stateType = typeof(LogitechDualActionLayout))]
    public class LogitechDualAction : Gamepad
    {
        internal static void Register()
        {
            InputSystem.RegisterLayout<LogitechDualAction>(
                "Logitech Dual Action",
                new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithCapability("vendorId", 0x46D)
                    .WithCapability("productId", 0xC216));

            // Alternatively, manufacturer and product name can be used.
            // .WithManufacturer("Logitech")
            // .WithProduct("Logitech.+Dual.+Action.*"));
        }
    }
}