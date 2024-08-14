using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities.Editor;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    public static class InputContextContentBuilder
    {
        public static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
        {
            switch (markerName)
            {
                case "Members":
                    OfflineInputData offlineInputData = EditorAssetGetter.GetFirst<OfflineInputData>();
                    foreach (InputContextInfo contextInfo in offlineInputData.InputContexts)
                        lines.Add($"        {contextInfo.Name},");
                    break;
            }
        }
    }
}