using NPTP.InputSystemWrapper.Utilities.Extensions;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Enums.NPTP.InputSystemWrapper;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Input Data used only in constructing classes from inside the editor. It lives in the runtime assembly only
    /// because it needs access to internal members of that assembly, but it is NOT to be accessed at runtime.
    /// </summary>
    public class OfflineInputData : ScriptableObject
    {
#if UNITY_EDITOR
        public const string RUNTIME_INPUT_DATA_PATH = nameof(RuntimeInputData);
        public const int MAX_PLAYERS_LIMIT = 4;

        [SerializeField] private string assetsPathToPackage = "Assets/InputSystemWrapper";
        public string AssetsPathToPackage => assetsPathToPackage;

        [SerializeField] private RuntimeInputData runtimeInputData;
        public RuntimeInputData RuntimeInputData => runtimeInputData;

        [SerializeField] private TextAsset actionsTemplateFile;
        public TextAsset ActionsTemplateFile => actionsTemplateFile;
        
        [SerializeField] private InitializationMode initializationMode;
        public InitializationMode InitializationMode => initializationMode;

        [SerializeField] private bool enableMultiplayer;
        public bool EnableMultiplayer => enableMultiplayer;
        
        [SerializeField][Range(2, MAX_PLAYERS_LIMIT)] private int maxPlayers = MAX_PLAYERS_LIMIT;
        public int MaxPlayers => maxPlayers;

        [SerializeField] private InputContext defaultContext;
        public InputContext DefaultContext => defaultContext;
        
        [SerializeField] private InputContextInfo[] inputContexts;
        public InputContextInfo[] InputContexts => inputContexts;

        [SerializeField] private string[] bindingExcludedPaths;
        public string[] BindingExcludedPaths => bindingExcludedPaths;

        [SerializeField] private string[] bindingCancelPaths;
        public string[] BindingCancelPaths => bindingCancelPaths;
        
        // TODO (architecture): these can probably just be ActionReference, now (and change how they get initialized then)
        [Header("Event System Actions")]
        [SerializeField] private InputActionReference point;
        [SerializeField] private InputActionReference leftClick;
        [SerializeField] private InputActionReference middleClick;
        [SerializeField] private InputActionReference rightClick;
        [SerializeField] private InputActionReference scrollWheel;
        [SerializeField] private InputActionReference move;
        [SerializeField] private InputActionReference submit;
        [SerializeField] private InputActionReference cancel;
        [SerializeField] private InputActionReference trackedDevicePosition;
        [SerializeField] private InputActionReference trackedDeviceOrientation;
        public InputActionReference Point => point;
        public InputActionReference LeftClick => leftClick;
        public InputActionReference MiddleClick => middleClick;
        public InputActionReference RightClick => rightClick;
        public InputActionReference ScrollWheel => scrollWheel;
        public InputActionReference Move => move;
        public InputActionReference Submit => submit;
        public InputActionReference Cancel => cancel;
        public InputActionReference TrackedDevicePosition => trackedDevicePosition;
        public InputActionReference TrackedDeviceOrientation => trackedDeviceOrientation;

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