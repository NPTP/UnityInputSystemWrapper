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
                            lines.Add($"        public {nameof(ActionWrapper)} {action.name.AsProperty()} " + "{ get; }");
                        }
                        else if (action.type is InputActionType.Value or InputActionType.PassThrough)
                        {
                            string expectedControlType = action.expectedControlType;
                            // Null/empty string means the "Any" control type.
                            string type = string.IsNullOrEmpty(expectedControlType)
                                ? nameof(AnyValueActionWrapper)
                                : $"{nameof(ValueActionWrapper)}<{controlTypeToTypeString(expectedControlType)}>";
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

            // Converts Unity's InputAction.expectedControlType string into another string that matches the Type keyword
            // for the expected type in code (e.g. "Integer" becomes "int")
            string controlTypeToTypeString(string controlType)
            {
                return controlType switch
                {
                    "Analog" => "float",
                    "Axis" => "float",
                    "Bone" => "Bone",
                    "Button" => "float",
                    "Delta" => "Vector2",
                    "Digital" => "int",
                    "DiscreteButton" => "int",
                    "Double" => "double",
                    "Dpad" => "Vector2",
                    "Eyes" => "Eyes",
                    "Integer" => "int",
                    "Pose" => "Pose",
                    "Quaternion" => "Quaternion",
                    "Stick" => "Vector2",
                    "Touch" => "float", // TODO (control types): Unknown
                    "Vector2" => "Vector2",
                    "Vector3" => "Vector3",
                    _ => controlType
                };
            }
        }
    }
}