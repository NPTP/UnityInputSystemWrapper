﻿using NPTP.InputSystemWrapper.InputDevices.Layouts;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace NPTP.InputSystemWrapper.InputDevices.Devices
{
    [InputControlLayout(stateType = typeof(RadioShack2603666Layout))]
    public class RadioShack2603666 : Gamepad
    {
        internal static void RegisterLayout()
        {
            InputSystem.RegisterLayout<RadioShack2603666>(
                "RadioShack 2603666",
                new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithCapability("vendorId", 0x6708)
                    .WithCapability("productId", 0x2666));
        }
    }
}