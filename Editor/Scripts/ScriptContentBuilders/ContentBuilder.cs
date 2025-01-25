using NPTP.InputSystemWrapper.Data;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal abstract class ContentBuilder
    {
        protected InputActionAsset Asset { get; }
        protected OfflineInputData Data { get; }
        
        internal ContentBuilder(OfflineInputData offlineInputData)
        {
            Data = offlineInputData;
            Asset = offlineInputData.RuntimeInputData.InputActionAsset;
        }

        internal abstract void AddContent(InputScriptGeneratorMarkerInfo info);
    }
}