using System.Collections.Generic;
using System.Linq;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Data;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class InputPlayerContentBuilder
    {
        internal static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
        {
            switch (markerName)
            {
                case "ControlSchemeEventDefinition":
                    lines.Add(Helper.OfflineInputData.EnableMultiplayer
                        ? $"        public event Action<{nameof(InputPlayer)}> OnControlSchemeChanged;"
                        : $"        public event Action<{nameof(ControlScheme)}> OnControlSchemeChanged;");
                    break;
                case "ControlSchemeEventInvocation":
                    lines.Add(Helper.OfflineInputData.EnableMultiplayer
                    ? "                    OnControlSchemeChanged?.Invoke(this);"
                    : "                    OnControlSchemeChanged?.Invoke(controlScheme);");
                    break;
                case "ActionsProperties":
                    foreach (string mapName in Helper.GetMapNames(asset))
                        lines.Add($"        public {mapName.AsProperty()}Actions {mapName.AsProperty()}" + " { get; }");
                    break;
                case "ActionsInstantiation":
                    foreach (string map in Helper.GetMapNames(asset))
                        lines.Add($"            {map.AsProperty()} = new {map.AsType()}Actions(Asset);");
                    break;
                case "EventSystemActions":
                    OfflineInputData oid = Helper.OfflineInputData;
                    addActionReference(oid.Point, nameof(oid.Point).AsField());
                    addActionReference(oid.LeftClick, nameof(oid.LeftClick).AsField());
                    addActionReference(oid.MiddleClick, nameof(oid.MiddleClick).AsField());
                    addActionReference(oid.RightClick, nameof(oid.RightClick).AsField());
                    addActionReference(oid.ScrollWheel, nameof(oid.ScrollWheel).AsField());
                    addActionReference(oid.Move, nameof(oid.Move).AsField());
                    addActionReference(oid.Submit, nameof(oid.Submit).AsField());
                    addActionReference(oid.Cancel, nameof(oid.Cancel).AsField());
                    addActionReference(oid.TrackedDevicePosition, nameof(oid.TrackedDevicePosition).AsField());
                    addActionReference(oid.TrackedDeviceOrientation, nameof(oid.TrackedDeviceOrientation).AsField());
                    
                    void addActionReference(InputActionReference inputActionReference, string inputModulePropertyName)
                    {
                        if (inputActionReference == null)
                            return;
                        
                        lines.Add($"            uiInputModule.{inputModulePropertyName} = createLocalAssetReference(\"{inputActionReference.action.id}\");");
                    }
                    
                    break;
                case "DisableAllMapsAndRemoveCallbacksBody":
                    foreach (string map in Helper.GetMapNames(asset))
                        lines.Add($"            {map.AsProperty()}.DisableAndUnregisterCallbacks();");
                    break;
                case "EnableContextSwitchMembers":
                    OfflineInputData inputData = Helper.OfflineInputData;
                    foreach (InputContextInfo contextInfo in inputData.InputContexts)
                    {
                        lines.Add($"                case {nameof(InputContext)}.{contextInfo.Name}:");
                        lines.Add($"                    {(contextInfo.EnableKeyboardTextInput ? "Enable" : "Disable")}KeyboardTextInput();");
                        foreach (string map in Helper.GetMapNames(asset))
                        {
                            bool enable = contextInfo.ActiveMaps.Any(activeMapName => map == activeMapName);
                            lines.Add($"                    {map.AsProperty()}.{(enable ? "EnableAndRegisterCallbacks" : "DisableAndUnregisterCallbacks")}();");
                        }
                        lines.Add($"                    break;");
                    }
                    break;
                case "FindActionWrapperIfElse":
                    int i = 0;
                    foreach (string map in Helper.GetMapNames(asset))
                    {
                        string ifElse = i == 0 ? "if" : "else if";
                        string mapProperty = map.AsProperty();
                        lines.Add($"            {ifElse} ({mapProperty}.ActionMap == map)");
                        lines.Add("            {");
                        foreach (InputAction action in asset.FindActionMap(map).actions)
                        {
                            string actionProperty = action.name.AsProperty();
                            lines.Add($"                if (action == {mapProperty}.{actionProperty}.InputAction) return {mapProperty}.{actionProperty};");
                        }
                        lines.Add("            }");
                        i++;
                    }
                    break;
            }
        }
    }
}
