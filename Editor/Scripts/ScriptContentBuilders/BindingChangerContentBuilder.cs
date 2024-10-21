using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class BindingChangerContentBuilder
    {
        internal static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
        {
            OfflineInputData offlineInputData = Helper.OfflineInputData;
            
            switch (markerName)
            {
                case "BindingExcludedPaths":
                    addStringElements(offlineInputData.BindingExcludedPaths);
                    break;
                case "BindingCancelPaths":
                    addStringElements(offlineInputData.BindingCancelPaths);
                    break;
                
                void addStringElements(string[] source)
                {
                    for (int i = 0; i < source.Length; i++)
                    {
                        string element = $"            \"{source[i]}\"";
                        if (i < source.Length - 1) element += ",";
                        lines.Add(element);
                    }
                }
            }
        }
    }
}