using System.Collections.Generic;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class ControlSchemeContentBuilder
    {
        internal static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
        {
            switch (markerName)
            {
                case "Members":
                    foreach (InputControlScheme controlScheme in asset.controlSchemes)
                        lines.Add($"        {controlScheme.name.AsEnumMember()},");
                    break;
                case "EnumToStringSwitch":
                    foreach (InputControlScheme controlScheme in asset.controlSchemes)
                        lines.Add($"                {nameof(ControlScheme)}.{controlScheme.name.AsEnumMember()} => \"{controlScheme.name}\",");
                    break;
                case "StringToEnumSwitch":
                    foreach (InputControlScheme controlScheme in asset.controlSchemes)
                        lines.Add($"                \"{controlScheme.name}\" => {nameof(ControlScheme)}.{controlScheme.name.AsEnumMember()},");
                    break;
            }
        }
    }
}