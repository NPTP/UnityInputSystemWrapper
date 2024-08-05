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
                case "MapActionsRemoveCallbacks":
                    foreach (string map in Helper.GetMapNames(asset))
                        lines.Add($"            {map.AsField()}Map.RemoveCallbacks({map.AsType()});");
                    break;
                case "EnableContextSwitchMembers":
                    OfflineInputData inputData = Helper.OfflineInputData;
                    foreach (InputContextInfo contextInfo in inputData.InputContextInfos)
                    {
                        lines.Add($"                case {nameof(InputContext)}.{contextInfo.Name}:");
                        lines.Add($"                    {(contextInfo.EnableKeyboardTextInput ? "Enable" : "Disable")}KeyboardTextInput();");
                        foreach (string map in Helper.GetMapNames(asset))
                        {
                            bool enable = contextInfo.ActiveMaps.Any(activeMapName => map == activeMapName);
                            lines.Add($"                    {map.AsField()}Map.{(enable ? "Enable" : "Disable")}();");
                            lines.Add($"                    {map.AsField()}Map.{(enable ? "Add" : "Remove")}Callbacks({map.AsType()});");
                        }
                        
                        // Event System actions setting
                        string actions = "                    SetUIEventSystemActions(";
                        InputActionReference[] inputActionReferences = contextInfo.EventSystemActions.AllInputActionReferences;
                        for (int j = 0; j < inputActionReferences.Length; j++)
                        {
                            InputActionReference inputActionReference = inputActionReferences[j];
                            if (inputActionReference == null) actions += "null";
                            else actions += $"{inputActionReference.action.actionMap.name.AsField()}Map.{inputActionReference.action.name.AsType()}";
                            if (j < inputActionReferences.Length - 1) actions += ", ";
                        }
                        actions += ");";
                        lines.Add(actions);
                        
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
