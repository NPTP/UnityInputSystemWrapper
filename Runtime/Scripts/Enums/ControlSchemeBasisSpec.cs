using System;

namespace NPTP.InputSystemWrapper.Enums
{
    /// <summary>
    /// The device families a control scheme is built on. A scheme requiring both a pointer and a gamepad
    /// is both, so these combine rather than being mutually exclusive.
    /// </summary>
    [Flags]
    internal enum ControlSchemeBasisSpec
    {
        Undefined = 0,
        IsPointerBased = 1 << 0,
        IsGamepadBased = 1 << 1
    }
}
