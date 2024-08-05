using System.Collections.Generic;
using InputSystemWrapper.Utilities.Editor;
using UnityEngine.InputSystem;
using UnityInputSystemWrapper.Data;

namespace UnityInputSystemWrapper.Editor.ScriptContentBuilders
{
    public static class InputContextContentBuilder
    {
        public static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
        {
            switch (markerName)
            {
                case "Members":
                    OfflineInputData offlineInputData = EditorAssetGetter.GetFirst<OfflineInputData>();
                    foreach (InputContextInfo contextInfo in offlineInputData.InputContextInfos)
                        lines.Add($"        {contextInfo.Name},");
                    break;
            }
        }
    }
}