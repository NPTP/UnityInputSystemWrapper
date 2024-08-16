using NPTP.InputSystemWrapper.Utilities.Extensions;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Input Data used only in constructing classes from inside the editor. Not meant to be accessed at runtime.
    /// </summary>
    public class OfflineInputData : ScriptableObject
    {
#if UNITY_EDITOR
        public const string RUNTIME_INPUT_DATA_PATH = nameof(RuntimeInputData);
        public const int MAX_PLAYERS_LIMIT = 4;
        
        [SerializeField] private bool enableMultiplayer;
        public bool EnableMultiplayer => enableMultiplayer;

        [SerializeField][Range(2, MAX_PLAYERS_LIMIT)] private int maxPlayers = MAX_PLAYERS_LIMIT;
        public int MaxPlayers => maxPlayers;

        [SerializeField] private RuntimeInputData runtimeInputData;
        public RuntimeInputData RuntimeInputData => runtimeInputData;

        [SerializeField] private TextAsset mapActionsTemplateFile;
        public TextAsset MapActionsTemplateFile => mapActionsTemplateFile;
        
        [SerializeField] private TextAsset mapCacheTemplateFile;
        public TextAsset MapCacheTemplateFile => mapCacheTemplateFile;

        [SerializeField] private InputContext defaultContext;
        public InputContext DefaultContext => defaultContext;
        
        [FormerlySerializedAs("inputContextInfos")] [SerializeField] private InputContextInfo[] inputContexts;
        public InputContextInfo[] InputContexts => inputContexts;
        
        [Header("Event System Actions")]
        [SerializeField] private InputActionReference point;
        [SerializeField] private InputActionReference middleClick;
        [SerializeField] private InputActionReference rightClick;
        [SerializeField] private InputActionReference scrollWheel;
        [SerializeField] private InputActionReference move;
        [SerializeField] private InputActionReference submit;
        [SerializeField] private InputActionReference cancel;
        [SerializeField] private InputActionReference trackedDevicePosition;
        [SerializeField] private InputActionReference trackedDeviceOrientation;
        [SerializeField] private InputActionReference leftClick;
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