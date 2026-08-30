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

        [Header("Input Device Binding Data (Auto-Generated List)")]
        [SerializeField] private ControlSchemeBindingDataEntry[] controlSchemeBindingData;

        [Tooltip("These control paths will not be registered when performing an interactive rebinding. " +
                 "Use for control paths that you don't want to allow the player to use in their own custom bindings.")]
        [SerializeField] private string[] bindingExcludedPaths;
        internal string[] BindingExcludedPaths => bindingExcludedPaths;

        [Tooltip("These control paths will cancel/exit an interactive rebinding. " +
                 "E.g. pressing the Esc key on keyboard will cancel rebinding of a button, without rebinding it to Esc.")]
        [SerializeField] private string[] bindingCancelPaths;
        internal string[] BindingCancelPaths => bindingCancelPaths;

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

        internal BindingData GetControlSchemeBindingData(ControlScheme controlScheme)
        {
            if (controlScheme is ControlScheme.None || controlSchemeBindingData == null)
            {
                return null;
            }

            string controlSchemeName = controlScheme.ToInputAssetName();
            foreach (ControlSchemeBindingDataEntry entry in controlSchemeBindingData)
            {
                if (entry.ControlSchemeName == controlSchemeName)
                {
                    return entry.BindingData;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        internal const string EDITOR_ControlSchemeBindingDataField = nameof(controlSchemeBindingData);
        internal const string EDITOR_BindingExcludedPathsField = nameof(bindingExcludedPaths);
        internal const string EDITOR_BindingCancelPathsField = nameof(bindingCancelPaths);
#endif
    }
}
