using UnityEngine;

namespace UnityInputSystemWrapper.Data
{
    /// <summary>
    /// Input Data used only in constructing classes from inside the editor. Not meant to be accessed at runtime.
    /// </summary>
    public class OfflineInputData : ScriptableObject
    {
#if UNITY_EDITOR
        public const string RUNTIME_INPUT_DATA_PATH = nameof(RuntimeInputData);

        [SerializeField] private bool singlePlayerOnly = true;
        public bool SinglePlayerOnly => singlePlayerOnly;

        [SerializeField] private RuntimeInputData runtimeInputData;
        public RuntimeInputData RuntimeInputData => runtimeInputData;

        [SerializeField] private TextAsset mapActionsTemplateFile;
        public TextAsset MapActionsTemplateFile => mapActionsTemplateFile;
        
        [SerializeField] private TextAsset mapCacheTemplateFile;
        public TextAsset MapCacheTemplateFile => mapCacheTemplateFile;

        [SerializeField] private InputContext defaultContext;
        public InputContext DefaultContext => defaultContext;
        
        [SerializeField] private InputContextInfo[] inputContextInfos;
        public InputContextInfo[] InputContextInfos => inputContextInfos;
#endif
    }
}