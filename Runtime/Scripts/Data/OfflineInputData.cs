using System.Linq;
using InputSystemWrapper.Utilities.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

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
        
        private void OnValidate()
        {
            VerifyEventSystemActions();
        }
        
        private void VerifyEventSystemActions()
        {
            foreach (InputContextInfo contextInfo in inputContextInfos)
            {
                contextInfo.EDITOR_SetName(contextInfo.Name.AllWhitespaceTrimmed().CapitalizeFirst());

                InputActionReference[] inputActionReferences = contextInfo.EventSystemActions.AllInputActionReferences;
                foreach (InputActionReference inputActionReference in inputActionReferences)
                {
                    if (inputActionReference == null)
                    {
                        continue;
                    }

                    foreach (string mapName in contextInfo.ActiveMaps)
                    {
                        InputActionMap map = runtimeInputData.InputActionAsset.FindActionMap(mapName);
                        if (map.actions.Contains(inputActionReference.action))
                        {
                            continue;
                        }
                        
                        // NP TODO: Actually prevent the field from being set
                        Debug.LogError($"Action {inputActionReference.action.name} is not a part of the maps in the context {contextInfo.Name}! This will not work!");
                        return;
                    }
                }
            }
        }
#endif
    }
}