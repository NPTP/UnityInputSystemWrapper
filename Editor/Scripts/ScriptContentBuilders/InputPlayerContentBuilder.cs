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
            OfflineInputData offlineInputData = Helper.OfflineInputData;
            switch (markerName)
            {
                case "ControlSchemeEventDefinition":
                    lines.Add(offlineInputData.EnableMultiplayer
                        ? $"        public event Action<{nameof(InputPlayer)}> OnControlSchemeChanged;"
                        : $"        public event Action<{nameof(ControlScheme)}> OnControlSchemeChanged;");
                    break;
                case "ControlSchemeEventInvocation":
                    lines.Add(offlineInputData.EnableMultiplayer
                    ? "                    OnControlSchemeChanged?.Invoke(this);"
                    : "                    OnControlSchemeChanged?.Invoke(controlScheme);");
                    break;
                case "ActionsProperties":
                    foreach (string mapName in Helper.GetMapNames(asset))
                        lines.Add($"        public {mapName.AsProperty()}Actions {mapName.AsProperty()}" + " { get; }");
                    break;
                case "ActionsInstantiation":
                    foreach (string map in Helper.GetMapNames(asset))
                        lines.Add($"            {map.AsProperty()} = new {map.AsType()}Actions(Asset, actionWrapperTable);");
                    break;
                case "EventSystemOptions":
                    lines.Add($"            uiInputModule.moveRepeatDelay = {offlineInputData.MoveRepeatDelay}f;");
                    lines.Add($"            uiInputModule.moveRepeatRate = {offlineInputData.MoveRepeatRate}f;");
                    lines.Add($"            uiInputModule.deselectOnBackgroundClick = {offlineInputData.DeselectOnBackgroundClick.ToString().ToLower()};");
                    lines.Add($"            uiInputModule.pointerBehavior = UIPointerBehavior.{offlineInputData.PointerBehavior};");
                    lines.Add($"            uiInputModule.cursorLockBehavior = InputSystemUIInputModule.CursorLockBehavior.{offlineInputData.CursorLockBehavior};");
                    break;
                case "PopulateEventSystemActionsPool":
                    HashSet<string> actionIDs = new();

                    const string defaultPrefix = "default";
                    AddEventSystemDefaultAction(offlineInputData.Point, defaultPrefix + nameof(offlineInputData.Point), 3, lines, actionIDs);
                    AddEventSystemDefaultAction(offlineInputData.LeftClick, defaultPrefix + nameof(offlineInputData.LeftClick), 3, lines, actionIDs);
                    AddEventSystemDefaultAction(offlineInputData.MiddleClick, defaultPrefix + nameof(offlineInputData.MiddleClick), 3, lines, actionIDs);
                    AddEventSystemDefaultAction(offlineInputData.RightClick, defaultPrefix + nameof(offlineInputData.RightClick), 3, lines, actionIDs);
                    AddEventSystemDefaultAction(offlineInputData.ScrollWheel, defaultPrefix + nameof(offlineInputData.ScrollWheel), 3, lines, actionIDs);
                    AddEventSystemDefaultAction(offlineInputData.Move, defaultPrefix + nameof(offlineInputData.Move), 3, lines, actionIDs);
                    AddEventSystemDefaultAction(offlineInputData.Submit, defaultPrefix + nameof(offlineInputData.Submit), 3, lines, actionIDs);
                    AddEventSystemDefaultAction(offlineInputData.Cancel, defaultPrefix + nameof(offlineInputData.Cancel), 3, lines, actionIDs);
                    AddEventSystemDefaultAction(offlineInputData.TrackedDevicePosition, defaultPrefix + nameof(offlineInputData.TrackedDevicePosition), 3, lines, actionIDs);
                    AddEventSystemDefaultAction(offlineInputData.TrackedDeviceOrientation, defaultPrefix + nameof(offlineInputData.TrackedDeviceOrientation), 3, lines, actionIDs);
                    
                    if (offlineInputData.GetEventSystemActionNonNullOverrideCount() == 0) break;
                    
                    lines.Add(string.Empty);
                    foreach (InputContextInfo inputContextInfo in offlineInputData.InputContexts)
                        foreach (EventSystemActionSpecification actionOverride in inputContextInfo.EventSystemActionOverrides)
                            AddEventSystemActionOverride(actionOverride.ActionReference, 3, lines, actionIDs);
                    
                    break;
                case "DisableAllMapsAndRemoveCallbacksBody":
                    foreach (string map in Helper.GetMapNames(asset))
                        lines.Add($"            {map.AsProperty()}.DisableAndUnregisterCallbacks();");
                    break;
                case "EnableContextSwitchMembers":
                    foreach (InputContextInfo contextInfo in offlineInputData.InputContexts)
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
                            string assignmentValue = spec.ActionReference == null || spec.ActionReference.action == null
                                    ? "null"
                                    : $"eventSystemActionsPool[\"{spec.ActionReference.action.id}\"]";
                            lines.Add($"                    uiInputModule.{inputModuleActionField} = {assignmentValue};");
                        }
                        
                        lines.Add($"                    break;");
                    }
                    break;
            }
        }
        
        private static void AddEventSystemDefaultAction(InputActionReference inputActionReference, string defaultActionFieldName, int tabs, List<string> lines, HashSet<string> actionIDs)
        {
            if (inputActionReference == null)
            {
                return;
            }

            StringBuilder line = new StringBuilder();
            string actionId = inputActionReference.action.id.ToString();
            
            for (int i = 0; i < tabs; i++) line.Append("    ");
            if (actionIDs.Contains(actionId))
            {
                line.Append($"{defaultActionFieldName} = eventSystemActionsPool[\"{actionId}\"];");
            }
            else
            {
                line.Append($"{defaultActionFieldName} = CreateInputActionReferenceToPlayerAsset(\"{actionId}\");\n");
                for (int i = 0; i < tabs; i++) line.Append("    ");
                line.Append($"eventSystemActionsPool.Add(\"{actionId}\", {defaultActionFieldName});");
                actionIDs.Add(actionId);
            }
            
            lines.Add(line.ToString());
        }

        private static void AddEventSystemActionOverride(InputActionReference inputActionReference, int tabs, List<string> lines, HashSet<string> actionIDs)
        {
            if (inputActionReference == null)
            {
                return;
            }

            string actionId = inputActionReference.action.id.ToString();
            if (actionIDs.Contains(actionId))
            {
                return;
            }

            actionIDs.Add(actionId);

            StringBuilder line = new StringBuilder();
            for (int i = 0; i < tabs; i++) line.Append("    ");
            line.Append($"eventSystemActionsPool.Add(\"{actionId}\", CreateInputActionReferenceToPlayerAsset(\"{actionId}\"));");
            lines.Add(line.ToString());
        }
    }
}
