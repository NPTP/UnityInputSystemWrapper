using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper
{
    /// <summary>
    /// String container with a custom property drawer that lets us select from every device supported by
    /// Unity's Input System. Made this its own struct so it can be used as a generic argument in a serialized
    /// dictionary and the custom property drawer works, as well as comparing two equivalent InputDeviceReferences
    /// by value to find them from the name of an actual InputDevice (see the static New method).
    /// </summary>
    [Serializable]
    public struct InputDeviceReference
    {
        [SerializeField] private string deviceTypeName;
        public string DeviceTypeName => deviceTypeName;

        public static InputDeviceReference New<TDevice>(TDevice device) where TDevice : InputDevice
        {
            return new InputDeviceReference(typeof(TDevice).Name);
        }
        
        private InputDeviceReference(string deviceTypeName)
        {
            this.deviceTypeName = deviceTypeName;
        }
    }
}
