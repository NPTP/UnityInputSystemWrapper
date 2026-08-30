using System.Collections.Generic;
using NPTP.InputSystemWrapper.Attributes;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.CustomSetups;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

#if UNITY_EDITOR
using NPTP.InputSystemWrapper.Utilities.Extensions;
#endif

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Everything the designer authors and everything the runtime reads.
    /// <para>
    /// Fields the runtime can use as authored are read directly. Fields it cannot - chiefly
    /// InputActionReferences, which name actions in the source asset rather than in a player's clone of it
    /// - are editor-only and baked by code generation into the generated fields. Editor-only fields are
    /// inside UNITY_EDITOR, so nothing they reference is pulled into a build.
    /// </para>
    /// </summary>
    internal class InputData : ScriptableObject
    {
        #region Authored And Used Directly

        [SerializeField] private InputActionAsset inputActionAsset;
        internal InputActionAsset InputActionAsset => inputActionAsset;

        [SerializeField] private CustomLayout[] customLayouts;
        [SerializeField] private CustomBinding[] customBindings;
        [SerializeField] private CustomInteraction[] customInteractions;

        [Tooltip("These control paths will not be registered when performing an interactive rebinding. " +
                 "Use for control paths that you don't want to allow the player to use in their own custom bindings.")]
        [ControlPathSelector][SerializeField] private string[] bindingExcludedPaths;
        internal string[] BindingExcludedPaths => bindingExcludedPaths;

        [Tooltip("These control paths will cancel/exit an interactive rebinding. " +
                 "E.g. pressing the Esc key on keyboard will cancel rebinding of a button, without rebinding it to Esc.")]
        [ControlPathSelector][SerializeField] private string[] bindingCancelPaths;
        internal string[] BindingCancelPaths => bindingCancelPaths;

        [SerializeField] private int defaultContextIndex;
        internal InputContextId DefaultContextId => new(defaultContextIndex);

        [Tooltip("When true, all saved bindings for all players are loaded when this system is initialized.")]
        [SerializeField] private bool loadAllBindingOverridesOnInitialize = true;
        internal bool LoadAllBindingOverridesOnInitialize => loadAllBindingOverridesOnInitialize;

        #endregion

        #region Authored, Editor Only, Baked Into The Generated Fields

#if UNITY_EDITOR
        [SerializeField] private InitializationMode initializationMode = InitializationMode.BeforeSceneLoad;
        internal InitializationMode InitializationMode => initializationMode;

        [SerializeField] private InputContextInfo[] authoredContexts;
        internal InputContextInfo[] AuthoredContexts => authoredContexts;

        [SerializeField] private ControlSchemeBasis[] controlSchemeBases;
        internal ControlSchemeBasis[] ControlSchemeBases => controlSchemeBases;

        [Header("Global Event System Options")]
        [SerializeField] private float moveRepeatDelay = 0.5f;
        internal float MoveRepeatDelay => moveRepeatDelay;

        [SerializeField] private float moveRepeatRate = 0.1f;
        internal float MoveRepeatRate => moveRepeatRate;

        [SerializeField] private bool deselectOnBackgroundClick;
        internal bool DeselectOnBackgroundClick => deselectOnBackgroundClick;

        [SerializeField] private UIPointerBehavior pointerBehavior = UIPointerBehavior.SingleMouseOrPenButMultiTouchAndTrack;
        internal UIPointerBehavior PointerBehavior => pointerBehavior;

        [SerializeField] private InputSystemUIInputModule.CursorLockBehavior cursorLockBehavior = InputSystemUIInputModule.CursorLockBehavior.OutsideScreen;
        internal InputSystemUIInputModule.CursorLockBehavior CursorLockBehavior => cursorLockBehavior;

        // TODO (architecture): these can probably just be ActionReference, now (and change how they get initialized then)
        [Header("Default Event System Actions")]
        [SerializeField] private InputActionReference point;
        internal InputActionReference Point => point;
        [SerializeField] private InputActionReference leftClick;
        internal InputActionReference LeftClick => leftClick;
        [SerializeField] private InputActionReference middleClick;
        internal InputActionReference MiddleClick => middleClick;
        [SerializeField] private InputActionReference rightClick;
        internal InputActionReference RightClick => rightClick;
        [SerializeField] private InputActionReference scrollWheel;
        internal InputActionReference ScrollWheel => scrollWheel;
        [SerializeField] private InputActionReference move;
        internal InputActionReference Move => move;
        [SerializeField] private InputActionReference submit;
        internal InputActionReference Submit => submit;
        [SerializeField] private InputActionReference cancel;
        internal InputActionReference Cancel => cancel;
        [SerializeField] private InputActionReference trackedDevicePosition;
        internal InputActionReference TrackedDevicePosition => trackedDevicePosition;
        [SerializeField] private InputActionReference trackedDeviceOrientation;
        internal InputActionReference TrackedDeviceOrientation => trackedDeviceOrientation;
#endif

        #endregion

        #region Generated

        [SerializeField] private ControlSchemeDefinition[] controlSchemes;

        /// <summary>One entry per device used by any control scheme.</summary>
        [SerializeField] private DeviceBindingData[] deviceBindingData;

        [SerializeField] private EventSystemOptions eventSystemOptions;
        internal EventSystemOptions EventSystemOptions => eventSystemOptions;

        [SerializeField] private InputContextDefinition[] contextDefinitions;
        internal InputContextDefinition[] ContextDefinitions => contextDefinitions;

        #endregion

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
            return contextDefinitions == null || index < 0 || index >= contextDefinitions.Length ? null : contextDefinitions[index];
        }

#if UNITY_EDITOR
        internal const string EDITOR_EventSystemOptionsField = nameof(eventSystemOptions);
        internal const string EDITOR_ContextDefinitionsField = nameof(contextDefinitions);
        internal const string EDITOR_DefaultContextIndexField = nameof(defaultContextIndex);
        internal const string EDITOR_LoadAllBindingOverridesOnInitializeField = nameof(loadAllBindingOverridesOnInitialize);
        internal const string EDITOR_ControlSchemesField = nameof(controlSchemes);
        internal const string EDITOR_DeviceBindingDataField = nameof(deviceBindingData);
        internal const string EDITOR_BindingExcludedPathsField = nameof(bindingExcludedPaths);
        internal const string EDITOR_BindingCancelPathsField = nameof(bindingCancelPaths);

        private void OnValidate()
        {
            if (authoredContexts == null)
            {
                return;
            }

            foreach (InputContextInfo inputContextInfo in authoredContexts)
            {
                inputContextInfo.EDITOR_SetName(inputContextInfo.Name.AlphaNumericCharactersOnly().AllWhitespaceTrimmed());
            }
        }
#endif
    }
}
