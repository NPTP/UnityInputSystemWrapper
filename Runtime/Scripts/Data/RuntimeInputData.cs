using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.CustomSetups;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Input Data used at runtime, containing the input action asset template on which new assets are cloned,
    /// and the data that lets us resolve input bindings to display names & sprites on the UI.
    /// </summary>
    internal class RuntimeInputData : ScriptableObject
    {
        [SerializeField] private InputActionAsset inputActionAsset;
        internal InputActionAsset InputActionAsset => inputActionAsset;

        [SerializeField] private CustomLayout[] customLayouts;
        [SerializeField] private CustomBinding[] customBindings;
        [SerializeField] private CustomInteraction[] customInteractions;

        public IEnumerable<CustomSetup> AllCustomSetups
        {
            get
            {
                List<CustomSetup> customSetups = new();
                customSetups.AddRange(customLayouts);
                customSetups.AddRange(customBindings);
                customSetups.AddRange(customInteractions);
                return customSetups;
            }
        }
        
        // MARKER.ControlSchemeBindingData.Start
        // MARKER.ControlSchemeBindingData.End

        internal BindingData GetControlSchemeBindingData(ControlScheme controlScheme)
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
