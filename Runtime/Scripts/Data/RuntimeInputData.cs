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

        // MARKER.ControlSchemeBindingData.Start
        // MARKER.ControlSchemeBindingData.End

        public BindingData GetControlSchemeBindingData(ControlScheme controlScheme)
        {
            return controlScheme switch
            {
                // MARKER.EnumToBindingDataSwitch.Start
                // MARKER.EnumToBindingDataSwitch.End
                _ => throw new ArgumentOutOfRangeException(nameof(controlScheme), controlScheme, null)
            };
        }
    }
}
