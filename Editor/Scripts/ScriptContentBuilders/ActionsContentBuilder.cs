using System.Collections.Generic;
using System.Linq;
using NPTP.InputSystemWrapper.Actions;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class ActionsContentBuilder
    {
        internal static void AddContent(string markerName, InputActionMap map, List<string> lines)
        {
            IEnumerable<string> getActionNames() => map.actions.Select(inputAction => inputAction.name);
            string className() => $"{map.name.AsType()}Actions";
            string actionMapProperty() => "ActionMap";

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
                    lines.Add($"    public class {className()}");
                    break;
                case "ActionWrapperPublicProperties":
                    foreach (InputAction action in map)
                    {
                        if (action.type is InputActionType.Button)
                        {
                            lines.Add($"        public {nameof(ButtonActionWrapper)} {action.name.AsProperty()} " + "{ get; }");
                        }
                        else if (action.type is InputActionType.Value or InputActionType.PassThrough)
                        {
                            string expectedControlType = action.expectedControlType;
                            // TODO: Converted expected control type to correct type string (e.g. "Integer" -> "int")
                            string type = string.IsNullOrEmpty(expectedControlType)
                                ? $"{nameof(AnyValueActionWrapper)}"
                                : $"{nameof(ValueActionWrapper)}<{expectedControlType}>";
                            lines.Add($"        public {type} {action.name.AsProperty()} " + "{ get; }");
                        }
                    }
                    break;
                case "ConstructorSignature":
                    lines.Add($"        internal {className()}({nameof(InputActionAsset)} asset)");
                    break;
                case "ActionMapAssignment":
                    lines.Add($"            {actionMapProperty()} = asset.FindActionMap(\"{map.name}\", throwIfNotFound: true);");
                    break;
                case "ActionWrapperAssignments":
                    foreach (string action in getActionNames())
                        lines.Add($"            {action.AsProperty()} = new ({actionMapProperty()}.FindAction(\"{action}\", throwIfNotFound: true));");
                    break;
                case "RegisterCallbacks":
                    foreach (string action in getActionNames())
                        lines.Add($"            {action.AsProperty()}.RegisterCallbacks();");
                    break;
                case "UnregisterCallbacks":
                    foreach (string action in getActionNames())
                        lines.Add($"            {action.AsProperty()}.UnregisterCallbacks();");
                    break;
            }
        }
    }
}