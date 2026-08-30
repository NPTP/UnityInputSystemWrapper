using System;

namespace NPTP.InputSystemWrapper.Enums
{
    /// <summary>
    /// The device families a control scheme is built on, one per device layout the input system registers
    /// directly under InputDevice. A scheme requiring both a pointer and a gamepad is both, so these
    /// combine rather than being mutually exclusive.
    /// <para>
    /// Families are matched by layout inheritance, so every device falls under one: a mouse, pen or
    /// touchscreen is a pointer, an accelerometer is a sensor, and so on.
    /// </para>
    /// </summary>
    [Flags]
    internal enum ControlSchemeBasisSpec
    {
        Undefined = 0,
        IsPointerBased = 1 << 0,
        IsGamepadBased = 1 << 1,
        IsKeyboardBased = 1 << 2,
        IsJoystickBased = 1 << 3,
        IsSensorBased = 1 << 4,
        IsTrackedDeviceBased = 1 << 5
    }
}
