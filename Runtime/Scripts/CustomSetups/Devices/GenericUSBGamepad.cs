using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace NPTP.InputSystemWrapper.CustomSetups.Devices
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
        
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        private struct GenericUSBGamepadLayout : IInputStateTypeInfo
        {
            public FourCC format => new FourCC('H', 'I', 'D');
            
            [FieldOffset(0)] public byte reportId;
            
            [InputControl(name = "leftStick", layout = "Stick", format = "VC2B")]
            [InputControl(name = "leftStick/x", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
            [InputControl(name = "leftStick/left", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
            [InputControl(name = "leftStick/right", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1")]
            [InputControl(name = "leftStick/y", offset = 1, format = "BYTE", parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
            [InputControl(name = "leftStick/up", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
            [InputControl(name = "leftStick/down", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1,invert=false")]
            [FieldOffset(1)] public byte leftStickX;
            [FieldOffset(2)] public byte leftStickY;

            [InputControl(name = "rightStick", layout = "Stick", format = "VC2B")]
            [InputControl(name = "rightStick/x", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
            [InputControl(name = "rightStick/left", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
            [InputControl(name = "rightStick/right", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1")]
            [InputControl(name = "rightStick/y", offset = 1, format = "BYTE", parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
            [InputControl(name = "rightStick/up", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
            [InputControl(name = "rightStick/down", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1,invert=false")]
            [FieldOffset(4)] public byte rightStickX;
            [FieldOffset(5)] public byte rightStickY;

            [InputControl(name = "dpad", format = "BIT", layout = "Dpad", sizeInBits = 4, defaultState = 8)]
            [InputControl(name = "dpad/up", format = "BIT", layout = "DiscreteButton", parameters = "minValue=7,maxValue=1,nullValue=8,wrapAtValue=7", bit = 0, sizeInBits = 4)]
            [InputControl(name = "dpad/right", format = "BIT", layout = "DiscreteButton", parameters = "minValue=1,maxValue=3", bit = 0, sizeInBits = 4)]
            [InputControl(name = "dpad/down", format = "BIT", layout = "DiscreteButton", parameters = "minValue=3,maxValue=5", bit = 0, sizeInBits = 4)]
            [InputControl(name = "dpad/left", format = "BIT", layout = "DiscreteButton", parameters = "minValue=5, maxValue=7", bit = 0, sizeInBits = 4)]
            [InputControl(name = "buttonNorth", displayName = "Triangle", bit = 4)]
            [InputControl(name = "buttonEast", displayName = "Circle", bit = 5)]
            [InputControl(name = "buttonSouth", displayName = "Cross", bit = 6)]
            [InputControl(name = "buttonWest", displayName = "Square", bit = 7)]
            [FieldOffset(6)] public byte buttons1;

            [InputControl(name = "leftShoulder", bit = 0)]
            [InputControl(name = "rightShoulder", bit = 1)]
            [InputControl(name = "leftTriggerButton", layout = "Button", bit = 2)]
            [InputControl(name = "rightTriggerButton", layout = "Button", bit = 3)]
            [InputControl(name = "select", bit = 4)]
            [InputControl(name = "start", bit = 5)]
            [InputControl(name = "leftStickPress", bit = 6)]
            [InputControl(name = "rightStickPress", bit = 7)]
            [FieldOffset(7)] public byte buttons2;
        }
    }
}