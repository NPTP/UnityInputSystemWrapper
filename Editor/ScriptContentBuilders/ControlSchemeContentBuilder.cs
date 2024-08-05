using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace UnityInputSystemWrapper.Editor.ScriptContentBuilders
{
    public static class ControlSchemeContentBuilder
    {
        public static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
        {
            switch (markerName)
            {
                case "Members":
                    foreach (InputControlScheme inputControlScheme in asset.controlSchemes)
                        lines.Add($"        {inputControlScheme.name.AsEnumMember()},");
                    break;
            }
        }
    }
}