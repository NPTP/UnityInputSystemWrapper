using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                case "EventSystemOptions":
                    OfflineInputData offlineData = Helper.OfflineInputData;
                    lines.Add($"            uiInputModule.moveRepeatDelay = {offlineData.MoveRepeatDelay}f;");
                    lines.Add($"            uiInputModule.moveRepeatRate = {offlineData.MoveRepeatRate}f;");
                    lines.Add($"            uiInputModule.deselectOnBackgroundClick = {offlineData.DeselectOnBackgroundClick.ToString().ToLower()};");
                    lines.Add($"            uiInputModule.pointerBehavior = UIPointerBehavior.{offlineData.PointerBehavior};");
                    lines.Add($"            uiInputModule.cursorLockBehavior = InputSystemUIInputModule.CursorLockBehavior.{offlineData.CursorLockBehavior};");
                    break;
                case "EventSystemActions":
                    OfflineInputData oid = Helper.OfflineInputData;
                    AddEventSystemActionReference(oid.Point, nameof(oid.Point).AsField(), 3, lines);
                    AddEventSystemActionReference(oid.LeftClick, nameof(oid.LeftClick).AsField(), 3, lines);
                    AddEventSystemActionReference(oid.MiddleClick, nameof(oid.MiddleClick).AsField(), 3, lines);
                    AddEventSystemActionReference(oid.RightClick, nameof(oid.RightClick).AsField(), 3, lines);
                    AddEventSystemActionReference(oid.ScrollWheel, nameof(oid.ScrollWheel).AsField(), 3, lines);
                    AddEventSystemActionReference(oid.Move, nameof(oid.Move).AsField(), 3, lines);
                    AddEventSystemActionReference(oid.Submit, nameof(oid.Submit).AsField(), 3, lines);
                    AddEventSystemActionReference(oid.Cancel, nameof(oid.Cancel).AsField(), 3, lines);
                    AddEventSystemActionReference(oid.TrackedDevicePosition, nameof(oid.TrackedDevicePosition).AsField(), 3, lines);
                    AddEventSystemActionReference(oid.TrackedDeviceOrientation, nameof(oid.TrackedDeviceOrientation).AsField(), 3, lines);
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

                        foreach (EventSystemActionSpecification spec in contextInfo.EventSystemActionOverrides)
                        {
                            string inputModuleActionField = spec.ActionType.ToString().AsField();
                            AddEventSystemActionReference(spec.ActionReference, inputModuleActionField, 5, lines);
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

        private static void AddEventSystemActionReference(InputActionReference inputActionReference, string inputModulePropertyName, int tabs, List<string> lines)
        {
            if (inputActionReference == null)
                return;

            StringBuilder line = new StringBuilder();
            for (int i = 0; i < tabs; i++) line.Append("    ");
            line.Append($"uiInputModule.{inputModulePropertyName} = CreateInputActionReferenceToPlayerAsset(\"{inputActionReference.action.id}\");");
            lines.Add(line.ToString());
        }
    }
}
