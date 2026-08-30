using NPTP.InputSystemWrapper.Utilities.Extensions;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Enums.NPTP.InputSystemWrapper;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Input Data used only in constructing classes from inside the editor. It lives in the runtime assembly only
    /// because it needs access to internal members of that assembly, but it is NOT to be accessed at runtime.
    /// </summary>
    internal class OfflineInputData : ScriptableObject
    {
#if UNITY_EDITOR

        #region Fields Hidden From User
        
        [SerializeField] private RuntimeInputData runtimeInputData;
        internal RuntimeInputData RuntimeInputData => runtimeInputData;

#if UNITY_EDITOR
        internal const string EDITOR_RuntimeInputDataField = nameof(runtimeInputData);
#endif

        [SerializeField] private TextAsset actionsTemplateFile;
        internal TextAsset ActionsTemplateFile => actionsTemplateFile;

        #endregion

        [SerializeField] private InitializationMode initializationMode = InitializationMode.BeforeSceneLoad;
        internal InitializationMode InitializationMode => initializationMode;

        [SerializeField] private int defaultContextIndex;
        internal int DefaultContextIndex => defaultContextIndex;
        
        [SerializeField] private InputContextInfo[] inputContexts;
        internal InputContextInfo[] InputContexts => inputContexts;
        
        [SerializeField] private ControlSchemeBasis[] controlSchemeBases;
        internal ControlSchemeBasis[] ControlSchemeBases => controlSchemeBases;

        [Tooltip("When true, all saved bindings for all players are loaded when this system is initialized. Set false if you want more precise control over when this happens and to make the call to load bindings yourself.")]
        [SerializeField] private bool loadAllBindingOverridesOnInitialize = true;
        internal bool LoadAllBindingOverridesOnInitialize => loadAllBindingOverridesOnInitialize;

        [Tooltip("These control paths will not be registered when performing an interactive rebinding. " +
                 "Use for control paths that you don't want to allow the player to use in their own custom bindings.")]
        [SerializeField] private string[] bindingExcludedPaths;
        internal string[] BindingExcludedPaths => bindingExcludedPaths;

        [Tooltip("These control paths will cancel/exit an interact rebinding. " +
                 "E.g. pressing the Esc key on keyboard will cancel rebinding of a button, without rebinding it to Esc.")]
        [SerializeField] private string[] bindingCancelPaths;
        internal string[] BindingCancelPaths => bindingCancelPaths;

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

        private void OnValidate()
        {
            foreach (InputContextInfo inputContextInfo in inputContexts)
            {
                inputContextInfo.EDITOR_SetName(inputContextInfo.Name.AlphaNumericCharactersOnly().AllWhitespaceTrimmed());
            }
        }
#endif
    }
}