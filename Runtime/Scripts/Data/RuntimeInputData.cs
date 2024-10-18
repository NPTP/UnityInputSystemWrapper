using System;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Input Data used at runtime, containing the input action asset template on which new assets are cloned,
    /// and the data that lets us resolve input bindings to display names & sprites on the UI.
    /// </summary>
    public class RuntimeInputData : ScriptableObject
    {
        [SerializeField] private InputActionAsset inputActionAsset;
        public InputActionAsset InputActionAsset => inputActionAsset;

        [Header("Input Device Binding Data (Auto-Generated List)")] [Space]
        // MARKER.ControlSchemeBindingData.Start
        [SerializeField] private BindingData keyboardMouseBindingData;
        [SerializeField] private BindingData gamepadBindingData;
        [SerializeField] private BindingData touchBindingData;
        [SerializeField] private BindingData joystickBindingData;
        [SerializeField] private BindingData xRBindingData;
        // MARKER.ControlSchemeBindingData.End

        public BindingData GetControlSchemeBindingData(ControlScheme controlScheme)
        {
            return controlScheme switch
            {
                // MARKER.EnumToBindingDataSwitch.Start
                ControlScheme.KeyboardMouse => keyboardMouseBindingData,
                ControlScheme.Gamepad => gamepadBindingData,
                ControlScheme.Touch => touchBindingData,
                ControlScheme.Joystick => joystickBindingData,
                ControlScheme.XR => xRBindingData,
                // MARKER.EnumToBindingDataSwitch.End
                _ => throw new ArgumentOutOfRangeException(nameof(controlScheme), controlScheme, null)
            };
        }
    }
}
