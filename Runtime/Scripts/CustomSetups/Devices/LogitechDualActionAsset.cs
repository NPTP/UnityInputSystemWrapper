using UnityEngine.InputSystem.Layouts;

namespace NPTP.InputSystemWrapper.CustomSetups.Devices
{
    public class LogitechDualActionAsset : CustomLayout<LogitechDualAction>
    {
        protected override string Name => "Logitech Dual Action (XInput)";
        protected override InputDeviceMatcher Matches => new InputDeviceMatcher()
            .WithInterface("HID")
            .WithCapability("vendorId", 0x46D)
            .WithCapability("productId", 0xC216);
        
        // Alternatively, manufacturer and product name can be used.
        // .WithManufacturer("Logitech")
        // .WithProduct("Logitech.+Dual.+Action.*"));
    }
}
