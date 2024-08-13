using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityInputSystemWrapper.Data;

namespace UnityInputSystemWrapper.Editor.ScriptContentBuilders
{
    public static class InputPlayerContentBuilder
    {
        public static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
        {
            switch (markerName)
            {
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
                    OfflineInputData offlineInputData = Helper.OfflineInputData;
                    addActionReference(offlineInputData.Point, nameof(offlineInputData.Point).AsField());
                    addActionReference(offlineInputData.MiddleClick, nameof(offlineInputData.MiddleClick).AsField());
                    addActionReference(offlineInputData.RightClick, nameof(offlineInputData.RightClick).AsField());
                    addActionReference(offlineInputData.ScrollWheel, nameof(offlineInputData.ScrollWheel).AsField());
                    addActionReference(offlineInputData.Move, nameof(offlineInputData.Move).AsField());
                    addActionReference(offlineInputData.Submit, nameof(offlineInputData.Submit).AsField());
                    addActionReference(offlineInputData.Cancel, nameof(offlineInputData.Cancel).AsField());
                    addActionReference(offlineInputData.TrackedDevicePosition, nameof(offlineInputData.TrackedDevicePosition).AsField());
                    addActionReference(offlineInputData.TrackedDeviceOrientation, nameof(offlineInputData.TrackedDeviceOrientation).AsField());
                    addActionReference(offlineInputData.LeftClick, nameof(offlineInputData.LeftClick).AsField());
                    
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
