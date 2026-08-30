using System;

namespace NPTP.InputSystemWrapper.Enums
{
    /// <summary>
    /// The device families a control scheme uses, one per device layout the input system registers
    /// directly under InputDevice. A scheme requiring both a pointer and a gamepad uses both, so these
    /// combine rather than being mutually exclusive.
    /// <para>
    /// Families are matched by layout inheritance, so every device falls under one: a mouse, pen or
    /// touchscreen is a pointer, an accelerometer is a sensor, and so on.
    /// </para>
    /// </summary>
    [Flags]
    internal enum ControlSchemeDeviceFamilies
    {
        Undefined = 0,
        UsesPointer = 1 << 0,
        UsesGamepad = 1 << 1,
        UsesKeyboard = 1 << 2,
        UsesJoystick = 1 << 3,
        UsesSensor = 1 << 4,
        UsesTrackedDevice = 1 << 5
    }
}
