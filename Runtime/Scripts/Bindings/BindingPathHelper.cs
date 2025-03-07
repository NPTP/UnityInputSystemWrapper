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
        
        internal static string[] MouseControlPaths => new[]
        {
            "leftButton",
            "rightButton",
            "middleButton",
            "forwardButton",
            "backButton",
            "scroll",
            "scroll/up",
            "scroll/down",
            "position",
            "position/x",
            "position/y"
        };
        
        internal static string[] KeyboardControlPaths => new []
        {
            "anyKey",
            "escape",
            "space",
            "enter",
            "tab",
            "backquote",
            "quote",
            "semicolon",
            "comma",
            "period",
            "slash",
            "backslash",
            "leftBracket",
            "rightBracket",
            "minus",
            "equals",
            "upArrow",
            "downArrow",
            "leftArrow",
            "rightArrow",
            "a",
            "b",
            "c",
            "d",
            "e",
            "f",
            "g",
            "h",
            "i",
            "j",
            "k",
            "l",
            "m",
            "n",
            "o",
            "p",
            "q",
            "r",
            "s",
            "t",
            "u",
            "v",
            "w",
            "x",
            "y",
            "z",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "0",
            "leftShift",
            "rightShift",
            "shift",
            "leftAlt",
            "rightAlt",
            "alt",
            "leftCtrl",
            "rightCtrl",
            "ctrl",
            "leftMeta",
            "rightMeta",
            "contextMenu",
            "backspace",
            "pageDown",
            "pageUp",
            "home",
            "end",
            "insert",
            "delete",
            "capsLock",
            "numLock",
            "printScreen",
            "scrollLock",
            "pause",
            "numpadEnter",
            "numpadDivide",
            "numpadMultiply",
            "numpadPlus",
            "numpadMinus",
            "numpadPeriod",
            "numpadEquals",
            "numpad1",
            "numpad2",
            "numpad3",
            "numpad4",
            "numpad5",
            "numpad6",
            "numpad7",
            "numpad8",
            "numpad9",
            "numpad0",
            "f1",
            "f2",
            "f3",
            "f4",
            "f5",
            "f6",
            "f7",
            "f8",
            "f9",
            "f10",
            "f11",
            "f12",
        };

        internal static string[] GamepadControlPaths => new[]
        {
            "start",
            "select",
            "leftShoulder",
            "leftTrigger",
            "rightShoulder",
            "rightTrigger",
            "buttonSouth",
            "buttonEast",
            "buttonWest",
            "buttonNorth",
            "leftStick",
            "leftStickPress",
            "leftStick/x",
            "leftStick/y",
            "leftStick/up",
            "leftStick/down",
            "leftStick/left",
            "leftStick/right",
            "rightStick",
            "rightStickPress",
            "rightStick/x",
            "rightStick/y",
            "rightStick/up",
            "rightStick/down",
            "rightStick/left",
            "rightStick/right",
            "dpad",
            "dpad/x",
            "dpad/y",
            "dpad/up",
            "dpad/down",
            "dpad/left",
            "dpad/right"
        };

        internal static string[] JoystickControlPaths => new[]
        {
            "stick",
            "stick/up",
            "stick/x",
            "stick/y",
            "stick/down",
            "stick/left",
            "stick/right",
            "button2",
            "button3",
            "button4",
            "button5",
            "button6",
            "button7",
            "button8",
            "button9",
            "button10",
            "button11",
            "button12",
            "trigger",
            "rz",
            "z",
            "hat",
            "hat/x",
            "hat/y",
            "hat/up",
            "hat/down",
            "hat/left",
            "hat/right",
        };
    }
}