using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class ControlSchemeContentBuilder
    {
        internal static void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "Members":
                    foreach (InputControlScheme controlScheme in info.InputActionAsset.controlSchemes)
                        info.NewLines.Add($"        {controlScheme.name.AsEnumMember()},");
                    break;
                case "EnumToStringSwitch":
                    foreach (InputControlScheme controlScheme in info.InputActionAsset.controlSchemes)
                        info.NewLines.Add($"                {nameof(ControlScheme)}.{controlScheme.name.AsEnumMember()} => \"{controlScheme.name}\",");
                    break;
                case "StringToEnumSwitch":
                    foreach (InputControlScheme controlScheme in info.InputActionAsset.controlSchemes)
                        info.NewLines.Add($"                \"{controlScheme.name}\" => {nameof(ControlScheme)}.{controlScheme.name.AsEnumMember()},");
                    break;
            }
        }
    }
}