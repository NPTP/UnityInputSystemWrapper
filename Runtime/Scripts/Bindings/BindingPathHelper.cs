using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingPathHelper
    {
        internal static string GetDeviceControlPath<TDevice>() where TDevice : InputDevice
        {
            Type deviceType = typeof(TDevice);
            if (deviceType == typeof(Mouse))
                return "<Mouse>";
            if (deviceType == typeof(Keyboard))
                return "<Keyboard>";
            if (deviceType == typeof(Gamepad))
                return "<Gamepad>";
            if (deviceType == typeof(XInputController))
                return "<XInputController>";
            if (deviceType == typeof(DualShockGamepad))
                return "<DualShockGamepad>";

            return string.Empty;
        }
    }
}