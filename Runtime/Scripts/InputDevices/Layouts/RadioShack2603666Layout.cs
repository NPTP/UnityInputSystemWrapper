using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace NPTP.InputSystemWrapper.InputDevices.Layouts
{
    [StructLayout(LayoutKind.Explicit, Size = 7)]
    internal struct RadioShack2603666Layout : IInputStateTypeInfo
    {
        // Standard Xbox-style layout:
        // north button - trigger
        // west button - button4
        // east button - button2
        // south button - button3
        // dpad - hat (matches Logitech Dual Action)
        // left stick - stick (matches Logitech Dual Action)
        // right stick - rz is y, z is x (matches Logitech Dual Action)
        // L stick press - button11 (matches Logitech Dual Action)
        // R stick press - button12 (matches Logitech Dual Action)
        // L bumper - button5 (matches Logitech Dual Action)
        // L trigger - button7 (matches Logitech Dual Action)
        // R bumper - button6 (matches Logitech Dual Action)
        // R trigger - button8 (matches Logitech Dual Action)
        // start - button9
        // select - button10
        
        public FourCC format => new FourCC('H', 'I', 'D');

        [FieldOffset(0)] public byte garbageByte;

        [InputControl(name = "leftStick", layout = "Stick", format = "VC2B")]
        [InputControl(name = "leftStick/x", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "leftStick/left", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
        [InputControl(name = "leftStick/right", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1")]
        [FieldOffset(1)] public byte leftStickX;
        
        [InputControl(name = "leftStick/y", offset = 1, format = "BYTE", parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "leftStick/up", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
        [InputControl(name = "leftStick/down", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1,invert=false")]
        [FieldOffset(2)] public byte leftStickY;

        [InputControl(name = "rightStick", layout = "Stick", format = "VC2B")]
        [InputControl(name = "rightStick/x", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "rightStick/left", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
        [InputControl(name = "rightStick/right", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1")]
        [FieldOffset(3)] public byte rightStickX;
        
        [InputControl(name = "rightStick/y", offset = 1, format = "BYTE", parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "rightStick/up", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
        [InputControl(name = "rightStick/down", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1,invert=false")]
        [FieldOffset(4)] public byte rightStickY;

        [InputControl(name = "dpad", format = "BIT", layout = "Dpad", sizeInBits = 4, defaultState = 8)]
        [InputControl(name = "dpad/up", format = "BIT", layout = "DiscreteButton", parameters = "minValue=7,maxValue=1,nullValue=8,wrapAtValue=7", bit = 0, sizeInBits = 4)]
        [InputControl(name = "dpad/right", format = "BIT", layout = "DiscreteButton", parameters = "minValue=1,maxValue=3", bit = 0, sizeInBits = 4)]
        [InputControl(name = "dpad/down", format = "BIT", layout = "DiscreteButton", parameters = "minValue=3,maxValue=5", bit = 0, sizeInBits = 4)]
        [InputControl(name = "dpad/left", format = "BIT", layout = "DiscreteButton", parameters = "minValue=5, maxValue=7", bit = 0, sizeInBits = 4)]
        [InputControl(name = "buttonWest", displayName = "One", bit = 4)]
        [InputControl(name = "buttonSouth", displayName = "Two", bit = 5)]
        [InputControl(name = "buttonEast", displayName = "Three", bit = 6)]
        [InputControl(name = "buttonNorth", displayName = "Four", bit = 7)]
        [FieldOffset(5)] public byte buttons1;

        [InputControl(name = "leftShoulder", displayName = "Five", bit = 0)]
        [InputControl(name = "rightShoulder", displayName = "Six", bit = 1)]
        [InputControl(name = "leftTriggerButton", displayName = "Seven", layout = "Button", bit = 2)]
        [InputControl(name = "rightTriggerButton", displayName = "Eight", layout = "Button", bit = 3)]
        [InputControl(name = "leftTrigger", displayName = "Seven", layout = "Button", bit = 2, format = "BIT")]
        [InputControl(name = "rightTrigger", displayName = "Eight", layout = "Button", bit = 3, format = "BIT")]
        [InputControl(name = "select", displayName = "Nine", bit = 4)]
        [InputControl(name = "start", displayName = "Ten", bit = 5)]
        [InputControl(name = "leftStickPress", bit = 6)]
        [InputControl(name = "rightStickPress", bit = 7)]
        [FieldOffset(6)] public byte buttons2;
    }
}