using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace NPTP.InputSystemWrapper.InputDevices
{
    [InputControlLayout(stateType = typeof(LogitechDualActionHIDInputReport))]
    public class LogitechDualAction : InputSystemWrapperGamepad
    {
        public static void Init() { }
        
        static LogitechDualAction()
        {
            RegisterLayoutWithVendorAndProductID();
        }

        private static void RegisterLayoutWithVendorAndProductID()
        {
            InputSystem.RegisterLayout<LogitechDualAction>(
                null,
                new InputDeviceMatcher()
            .WithInterface("HID")
            .WithCapability("vendorId", 0x46D)
            .WithCapability("productId", 0xC216));
            
            // Alternatively, manufacturer and product name can be used.
            // .WithManufacturer("Logitech")
            // .WithProduct("Logitech Dual Action"));
        }
    }
}