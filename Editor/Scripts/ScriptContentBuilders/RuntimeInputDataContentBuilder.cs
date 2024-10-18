using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class RuntimeInputDataContentBuilder
    {
        internal static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
        {
            switch (markerName)
            {
                case "ControlSchemeBindingData":
                    foreach (InputControlScheme controlScheme in asset.controlSchemes)
                        lines.Add($"        [SerializeField] private {nameof(BindingData)} {controlScheme.name.AsField()}BindingData;");
                    break;
                case "EnumToBindingDataSwitch":
                    foreach (InputControlScheme controlScheme in asset.controlSchemes)
                        lines.Add($"                {nameof(ControlScheme)}.{controlScheme.name.AsEnumMember()} => {controlScheme.name.AsField()}BindingData,");
                    break;
            }
        }
    }
}