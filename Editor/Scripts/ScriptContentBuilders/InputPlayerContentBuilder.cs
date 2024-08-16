using System.Collections.Generic;
using System.Linq;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Data;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    public static class InputPlayerContentBuilder
    {
        public static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
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
                case "MapActionsProperties":
                    foreach (string mapName in Helper.GetMapNames(asset))
                        lines.Add($"        public {mapName.AsType()}Actions {mapName.AsType()}" + " { get; }");
                    break;
                case "MapCacheFields":
                    foreach (string map in Helper.GetMapNames(asset))
                        lines.Add($"        private readonly {map.AsType()}MapCache {map.AsField()}Map;");
                    break;
                case "MapAndActionsInstantiation":
                    foreach (string map in Helper.GetMapNames(asset))
                    {
                        lines.Add($"            {map.AsType()} = new {map.AsType()}Actions();");
                        lines.Add($"            {map.AsField()}Map = new {map.AsType()}MapCache(asset);");
                    }
                    break;
                case "EventSystemActions":
                    OfflineInputData oid = Helper.OfflineInputData;
                    addActionReference(oid.Point, nameof(oid.Point).AsField());
                    addActionReference(oid.MiddleClick, nameof(oid.MiddleClick).AsField());
                    addActionReference(oid.RightClick, nameof(oid.RightClick).AsField());
                    addActionReference(oid.ScrollWheel, nameof(oid.ScrollWheel).AsField());
                    addActionReference(oid.Move, nameof(oid.Move).AsField());
                    addActionReference(oid.Submit, nameof(oid.Submit).AsField());
                    addActionReference(oid.Cancel, nameof(oid.Cancel).AsField());
                    addActionReference(oid.TrackedDevicePosition, nameof(oid.TrackedDevicePosition).AsField());
                    addActionReference(oid.TrackedDeviceOrientation, nameof(oid.TrackedDeviceOrientation).AsField());
                    addActionReference(oid.LeftClick, nameof(oid.LeftClick).AsField());
                    
                    void addActionReference(InputActionReference inputActionReference, string inputModulePropertyName)
                    {
                        if (inputActionReference == null)
                            return;
                        
                        lines.Add($"            uiInputModule.{inputModulePropertyName} = createLocalAssetReference(\"{inputActionReference.action.id}\");");
                    }
                    
                    break;
                case "MapActionsRemoveCallbacks":
                    foreach (string map in Helper.GetMapNames(asset))
                        lines.Add($"            {map.AsField()}Map.RemoveCallbacks({map.AsType()});");
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
                            lines.Add($"                    {map.AsField()}Map.{(enable ? "Enable" : "Disable")}();");
                            lines.Add($"                    {map.AsField()}Map.{(enable ? "Add" : "Remove")}Callbacks({map.AsType()});");
                        }
                        
                        lines.Add($"                    break;");
                    }
                    break;
                case "ControlSchemeSwitch":
                    foreach (InputControlScheme controlScheme in asset.controlSchemes)
                        lines.Add($"                \"{controlScheme.name}\" => {nameof(ControlScheme)}.{controlScheme.name.AsEnumMember()},");
                    break;
                case "ChangeSubscriptionIfStatements":
                    int i = 0;
                    foreach (string map in Helper.GetMapNames(asset))
                    {
                        string ifElse = i == 0 ? "if" : "else if";
                        string mapField = $"{map.AsField()}Map";
                        lines.Add($"            {ifElse} ({mapField}.ActionMap == map)");
                        lines.Add("            {");
                        int j = 0;
                        foreach (InputAction action in asset.FindActionMap(map).actions)
                        {
                            string actionType = action.name.AsType();
                            string eventName = $"On{actionType}";
                            string mapType = map.AsType();
                            string ifElseInner = j == 0 ? "if" : "else if";
                            lines.Add($"                {ifElseInner} (action == {mapField}.{actionType})");
                            lines.Add("                {");
                            lines.Add($"                    {mapType}.{eventName} -= callback;");
                            lines.Add($"                    if (subscribe) {mapType}.{eventName} += callback;");
                            lines.Add("                }");
                            j++;
                        }
                        lines.Add("            }");
                        i++;
                    }
                    break;
            }
        }
    }
}
