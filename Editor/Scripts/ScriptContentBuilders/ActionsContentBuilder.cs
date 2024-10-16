using System.Collections.Generic;
using System.Linq;
using NPTP.InputSystemWrapper.Utilities.Extensions;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class ActionsContentBuilder
    {
        public static void AddContent(string markerName, InputActionMap map, List<string> lines)
        {
            IEnumerable<string> getActionNames() => map.actions.Select(inputAction => inputAction.name);
            IEnumerable<string> getActionNamesAsTypes() => map.actions.Select(inputAction => inputAction.name.AllWhitespaceTrimmed().CapitalizeFirst());
            IEnumerable<string> getActionNamesAsProperties() => getActionNamesAsTypes();
            
            string mapName = map.name.AllWhitespaceTrimmed().CapitalizeFirst();
            string className = $"{mapName}Actions";
            string actionMapProperty = "ActionMap";

            switch (markerName)
            {
                case "Ignore":
                    break;
                case "GeneratorNotice":
                    lines.AddRange(Helper.GetGeneratorNoticeLines());
                    break;
                case "Namespace":
                    lines.Add($"namespace {Helper.InputNamespace}.{Helper.GENERATED}.{Helper.ACTIONS}");
                    break;
                case "ClassSignature":
                    lines.Add($"    public class {className}");
                    break;
                case "PublicEvents":
                    int i = 0;
                    IEnumerable<string> actions = getActionNamesAsTypes();
                    foreach (string action in actions)
                    {
                        string eventName = $"On{action}";
                        string privateEventName = "_" + eventName;
                        lines.Add($"        private event Action<InputAction.CallbackContext> @{privateEventName};");
                        lines.Add($"        public event Action<InputAction.CallbackContext> @{eventName}");
                        lines.Add("        {");
                        lines.Add($"            add {'{'} {privateEventName} -= value; {privateEventName} += value; {'}'}");
                        lines.Add($"            remove => {privateEventName} -= value;");
                        lines.Add("        }");
                        if (i < actions.Count() - 1) lines.Add(string.Empty);
                        i++;
                    }
                    break;
                case "ActionWrapperPublicProperties":
                    foreach (string action in getActionNamesAsProperties())
                        lines.Add($"        public {nameof(ActionWrapper)} {action} " + "{ get; }");
                    break;
                case "ConstructorSignature":
                    lines.Add($"        internal {className}({nameof(InputActionAsset)} asset)");
                    break;
                case "ActionMapAssignment":
                    lines.Add($"            {actionMapProperty} = asset.FindActionMap(\"{map.name}\", throwIfNotFound: true);");
                    break;
                case "ActionWrapperAssignments":
                    foreach (string action in getActionNames())
                        lines.Add($"            {action.AsProperty()} = new {nameof(ActionWrapper)}({actionMapProperty}.FindAction(\"{action}\", throwIfNotFound: true));");
                    break;
                case "ActionsSubscribe":
                    foreach (string action in getActionNames())
                    {
                        lines.Add($"            {action.AsProperty()}.InputAction.started += Handle{action.AsProperty()};");
                        lines.Add($"            {action.AsProperty()}.InputAction.performed += Handle{action.AsProperty()};");
                        lines.Add($"            {action.AsProperty()}.InputAction.canceled += Handle{action.AsProperty()};");
                    }
                    break;
                case "ActionsUnsubscribe":
                    foreach (string action in getActionNames())
                    {
                        lines.Add($"            {action.AsProperty()}.InputAction.started -= Handle{action.AsProperty()};");
                        lines.Add($"            {action.AsProperty()}.InputAction.performed -= Handle{action.AsProperty()};");
                        lines.Add($"            {action.AsProperty()}.InputAction.canceled -= Handle{action.AsProperty()};");
                    }
                    break;
                case "PrivateEventHandlers":
                    foreach (string action in getActionNamesAsTypes())
                    {
                        string methodName = $"Handle{action}";
                        string privateEventName = $"_On{action}";
                        lines.Add($"        private void {methodName}(InputAction.CallbackContext context) => {privateEventName}?.Invoke(context);");
                    }
                    break;
            }
        }
    }
}