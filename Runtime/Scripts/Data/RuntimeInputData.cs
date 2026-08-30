using System.Collections.Generic;
using NPTP.InputSystemWrapper.Attributes;
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
        [SerializeField] private ControlSchemeDefinition[] controlSchemes;

        /// <summary>
        /// One entry per device used by any control scheme. A device that several schemes share has a
        /// single set of binding data, rather than one copy per scheme.
        /// </summary>
        [SerializeField] private DeviceBindingData[] deviceBindingData;

        [Tooltip("These control paths will not be registered when performing an interactive rebinding. " +
                 "Use for control paths that you don't want to allow the player to use in their own custom bindings.")]
        [ControlPathSelector][SerializeField] private string[] bindingExcludedPaths;
        internal string[] BindingExcludedPaths => bindingExcludedPaths;

        [Tooltip("These control paths will cancel/exit an interactive rebinding. " +
                 "E.g. pressing the Esc key on keyboard will cancel rebinding of a button, without rebinding it to Esc.")]
        [ControlPathSelector][SerializeField] private string[] bindingCancelPaths;
        internal string[] BindingCancelPaths => bindingCancelPaths;

        [SerializeField] private EventSystemOptions eventSystemOptions;
        internal EventSystemOptions EventSystemOptions => eventSystemOptions;

        [SerializeField] private InputContextDefinition[] inputContexts;
        internal InputContextDefinition[] InputContexts => inputContexts;

        [SerializeField] private int defaultContextIndex;
        internal InputContextId DefaultContextId => new(defaultContextIndex);

        [Tooltip("When true, all saved bindings for all players are loaded when this system is initialized.")]
        [SerializeField] private bool loadAllBindingOverridesOnInitialize = true;
        internal bool LoadAllBindingOverridesOnInitialize => loadAllBindingOverridesOnInitialize;

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

        /// <summary>
        /// Resolve a control scheme by its index, which the generated ControlScheme enum's values match.
        /// </summary>
        internal ControlSchemeId GetControlSchemeId(int index)
        {
            return controlSchemes == null || index < 0 || index >= controlSchemes.Length
                ? ControlSchemeId.None
                : controlSchemes[index].ToId(index);
        }

        /// <summary>
        /// Resolve a control scheme by the name it has in the input action asset.
        /// </summary>
        internal ControlSchemeId GetControlSchemeId(string controlSchemeName)
        {
            if (controlSchemes == null || string.IsNullOrEmpty(controlSchemeName))
            {
                return ControlSchemeId.None;
            }

            for (int i = 0; i < controlSchemes.Length; i++)
            {
                if (controlSchemes[i].ControlSchemeName == controlSchemeName)
                {
                    return controlSchemes[i].ToId(i);
                }
            }

            return ControlSchemeId.None;
        }

        /// <summary>
        /// The binding data for a device layout, e.g. "Keyboard". Null when that device has none, which
        /// means its controls cannot produce display names or sprites.
        /// </summary>
        internal BindingData GetBindingData(string deviceLayoutName)
        {
            if (deviceBindingData == null || string.IsNullOrEmpty(deviceLayoutName))
            {
                return null;
            }

            foreach (DeviceBindingData entry in deviceBindingData)
            {
                if (entry.DeviceLayoutName == deviceLayoutName)
                {
                    return entry.BindingData;
                }
            }

            return null;
        }

        internal InputContextDefinition GetContextDefinition(InputContextId inputContextId)
        {
            int index = inputContextId.Index;
            return inputContexts == null || index < 0 || index >= inputContexts.Length ? null : inputContexts[index];
        }

#if UNITY_EDITOR
        internal const string EDITOR_EventSystemOptionsField = nameof(eventSystemOptions);
        internal const string EDITOR_InputContextsField = nameof(inputContexts);
        internal const string EDITOR_DefaultContextIndexField = nameof(defaultContextIndex);
        internal const string EDITOR_LoadAllBindingOverridesOnInitializeField = nameof(loadAllBindingOverridesOnInitialize);
        internal const string EDITOR_ControlSchemesField = nameof(controlSchemes);
        internal const string EDITOR_DeviceBindingDataField = nameof(deviceBindingData);
        internal const string EDITOR_BindingExcludedPathsField = nameof(bindingExcludedPaths);
        internal const string EDITOR_BindingCancelPathsField = nameof(bindingCancelPaths);
#endif
    }
}
