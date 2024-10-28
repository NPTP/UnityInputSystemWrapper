using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class ActionWrapperContentBuilder
    {
        internal static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
        {
            OfflineInputData offlineInputData = Helper.OfflineInputData;

            switch (markerName)
            {
                case "ActionReferenceProperty":
                    if (offlineInputData.ActionReferencesInActionWrappers)
                        lines.Add($"        public {nameof(ActionReference)} ActionReference " + "{ get; }");
                    break;
                case "ActionReferenceInitializer":
                    if (offlineInputData.ActionReferencesInActionWrappers)
                        lines.Add($"            ActionReference = inputAction;");
                    break;
            }
        }
    }
}