using System;
using NPTP.InputSystemWrapper.Enums;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingDeviceHelper
    {
        private const string MOUSE = "Mouse";
        private const string KEYBOARD = "Keyboard";
        private const string GAMEPAD = "Gamepad";
        private const string XBOX = "XInputController";
        private const string PLAYSTATION = "DualShockGamepad";

        private static readonly string[] mouseKeyboardStrings = { MOUSE, KEYBOARD };
        private static readonly string[] supportedGamepadStrings = { GAMEPAD, XBOX, PLAYSTATION };

        internal static bool TryGetSupportedDeviceFromBindingPath(string bindingPath, out SupportedDevice supportedDevice)
        {
            supportedDevice = SupportedDevice.MouseKeyboard;

            if (string.IsNullOrEmpty(bindingPath) || !bindingPath.StartsWith('<') || !bindingPath.Contains('>'))
                return false;

            string devicePartOfPath = bindingPath.Substring(1, bindingPath.IndexOf('>') - 1);

            switch (devicePartOfPath)
            {
                case MOUSE or KEYBOARD:
                    supportedDevice = SupportedDevice.MouseKeyboard;
                    return true;
                case XBOX:
                    supportedDevice = SupportedDevice.Xbox;
                    return true;
                case PLAYSTATION:
                    supportedDevice = SupportedDevice.PlayStation;
                    return true;
                case GAMEPAD:
                    supportedDevice = SupportedDevice.Gamepad;
                    return true;
            }

            return false;
        }
        
        internal static string[] GetDevicePathStrings(SupportedDevice device)
        {
            switch (device)
            {
                case SupportedDevice.MouseKeyboard:
                    return mouseKeyboardStrings;
                case SupportedDevice.Gamepad:
                case SupportedDevice.Xbox:
                case SupportedDevice.PlayStation:
                    return supportedGamepadStrings;
                default:
                    throw new ArgumentOutOfRangeException(nameof(device), device, null);
            }
        }
    }
}