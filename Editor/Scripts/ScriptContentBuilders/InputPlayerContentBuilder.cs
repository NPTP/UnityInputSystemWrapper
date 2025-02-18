using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Data;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal class InputPlayerContentBuilder : ContentBuilder
    {
        internal override void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "ControlSchemeEventDefinition":
                    info.NewLines.Add(Data.EnableMultiplayer
                        ? $"        public event Action<{nameof(InputPlayer)}> OnControlSchemeChanged;"
                        : $"        public event Action<{nameof(ControlScheme)}> OnControlSchemeChanged;");
                    break;
                case "ControlSchemeEventInvocation":
                    info.NewLines.Add(Data.EnableMultiplayer
                    ? "                    OnControlSchemeChanged?.Invoke(this);"
                    : "                    OnControlSchemeChanged?.Invoke(controlScheme);");
                    break;
                case "ActionsProperties":
                    foreach (string mapName in Helper.GetMapNames(Asset))
                        info.NewLines.Add($"        public {mapName.AsProperty()}Actions {mapName.AsProperty()}" + " { get; }");
                    break;
                case "ActionsInstantiation":
                    foreach (string map in Helper.GetMapNames(Asset))
                        info.NewLines.Add($"            {map.AsProperty()} = new {map.AsType()}Actions(Asset, actionWrapperTable);");
                    break;
                case "EventSystemOptions":
                    info.NewLines.Add($"            uiInputModule.moveRepeatDelay = {Data.MoveRepeatDelay.ToString(CultureInfo.InvariantCulture)}f;");
                    info.NewLines.Add($"            uiInputModule.moveRepeatRate = {Data.MoveRepeatRate.ToString(CultureInfo.InvariantCulture)}f;");
                    info.NewLines.Add($"            uiInputModule.deselectOnBackgroundClick = {Data.DeselectOnBackgroundClick.ToString().ToLower()};");
                    info.NewLines.Add($"            uiInputModule.pointerBehavior = UIPointerBehavior.{Data.PointerBehavior};");
                    info.NewLines.Add($"            uiInputModule.cursorLockBehavior = InputSystemUIInputModule.CursorLockBehavior.{Data.CursorLockBehavior};");
                    break;
                case "PopulateEventSystemActionsPool":
                    HashSet<string> actionIDs = new();

                    const string defaultPrefix = "default";
                    AddEventSystemDefaultAction(Data.Point, defaultPrefix + nameof(Data.Point), 3, info.NewLines, actionIDs);
                    AddEventSystemDefaultAction(Data.LeftClick, defaultPrefix + nameof(Data.LeftClick), 3, info.NewLines, actionIDs);
                    AddEventSystemDefaultAction(Data.MiddleClick, defaultPrefix + nameof(Data.MiddleClick), 3, info.NewLines, actionIDs);
                    AddEventSystemDefaultAction(Data.RightClick, defaultPrefix + nameof(Data.RightClick), 3, info.NewLines, actionIDs);
                    AddEventSystemDefaultAction(Data.ScrollWheel, defaultPrefix + nameof(Data.ScrollWheel), 3, info.NewLines, actionIDs);
                    AddEventSystemDefaultAction(Data.Move, defaultPrefix + nameof(Data.Move), 3, info.NewLines, actionIDs);
                    AddEventSystemDefaultAction(Data.Submit, defaultPrefix + nameof(Data.Submit), 3, info.NewLines, actionIDs);
                    AddEventSystemDefaultAction(Data.Cancel, defaultPrefix + nameof(Data.Cancel), 3, info.NewLines, actionIDs);
                    AddEventSystemDefaultAction(Data.TrackedDevicePosition, defaultPrefix + nameof(Data.TrackedDevicePosition), 3, info.NewLines, actionIDs);
                    AddEventSystemDefaultAction(Data.TrackedDeviceOrientation, defaultPrefix + nameof(Data.TrackedDeviceOrientation), 3, info.NewLines, actionIDs);
                    
                    if (Data.GetEventSystemActionNonNullOverrideCount() == 0) break;
                    
                    info.NewLines.Add(string.Empty);
                    foreach (InputContextInfo inputContextInfo in Data.InputContexts)
                        foreach (EventSystemActionSpecification actionOverride in inputContextInfo.EventSystemActionOverrides)
                            AddEventSystemActionOverride(actionOverride.ActionReference, 3, info.NewLines, actionIDs);
                    
                    break;
                case "DisableAllMapsAndRemoveCallbacksBody":
                    foreach (string map in Helper.GetMapNames(Asset))
                        info.NewLines.Add($"            {map.AsProperty()}.DisableAndUnregisterCallbacks();");
                    break;
                case "EnableContextSwitchMembers":
                    foreach (InputContextInfo contextInfo in Data.InputContexts)
                    {
                        info.NewLines.Add($"                case {nameof(InputContext)}.{contextInfo.Name}:");
                        info.NewLines.Add($"                    {(contextInfo.EnableKeyboardTextInput ? "Enable" : "Disable")}KeyboardTextInput();");
                        foreach (string map in Helper.GetMapNames(Asset))
                        {
                            bool enable = contextInfo.ActiveMaps.Any(activeMapName => map == activeMapName);
                            info.NewLines.Add($"                    {map.AsProperty()}.{(enable ? "EnableAndRegisterCallbacks" : "DisableAndUnregisterCallbacks")}();");
                        }

                        foreach (EventSystemActionSpecification spec in contextInfo.EventSystemActionOverrides)
                        {
                            string inputModuleActionField = spec.ActionType.ToString().AsField();
                            string assignmentValue = spec.ActionReference == null || spec.ActionReference.action == null
                                    ? "null"
                                    : $"eventSystemActionsPool[\"{spec.ActionReference.action.id}\"]";
                            info.NewLines.Add($"                    uiInputModule.{inputModuleActionField} = {assignmentValue};");
                        }
                        
                        info.NewLines.Add($"                    break;");
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

        public InputPlayerContentBuilder(OfflineInputData offlineInputData) : base(offlineInputData)
        {
        }
    }
}
