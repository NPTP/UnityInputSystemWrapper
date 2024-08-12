using System;
using InputSystemWrapper.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityInputSystemWrapper.Data
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