using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    public static class MapCachesContentBuilder
    {
        public static void AddContent(string markerName, InputActionMap map, List<string> lines)
        {
            string className = $"{map.name.AsType()}MapCache";
            string interfaceName = $"I{map.name.AsType()}Actions";
            string actionMapProperty = "ActionMap";

            switch (markerName)
            {
                case "Ignore":
                    break;
                case "GeneratorNotice":
                    lines.AddRange(Helper.GetGeneratorNoticeLines());
                    break;
                case "Namespace":
                    lines.Add($"namespace {Helper.InputNamespace}.{Helper.GENERATED}.{Helper.MAP_CACHES}");
                    break;
                case "ClassSignature":
                    lines.Add($"    public class {className}");
                    break;
                case "InterfacesList":
                    lines.Add($"        private readonly List<{interfaceName}> interfaces = new();");
                    break;
                case "InputActionFields":
                    foreach (string action in getActionNames())
                    {
                        lines.Add($"        public InputAction {action.AsType()} " + "{ get; }");
                    }
                    break;
                case "ConstructorSignature":
                    lines.Add($"        public {className}({nameof(InputActionAsset)} asset)");
                    break;
                case "ActionMapAssignment":
                    lines.Add($"            {actionMapProperty} = asset.FindActionMap(\"{map.name}\", throwIfNotFound: true);");
                    break;
                case "InputActionAssignments":
                    foreach (string action in getActionNames())
                    {
                        lines.Add($"            {action.AsType()} = {actionMapProperty}.FindAction(\"{action}\", throwIfNotFound: true);");
                    }
                    break;
                case "AddCallbacksSignature":
                    lines.Add($"        public void AddCallbacks({interfaceName} instance)");
                    break;
                case "ActionsSubscribe":
                    foreach (string action in getActionNames())
                    {
                        lines.Add($"            {action.AsType()}.started += instance.On{action.AsType()};");
                        lines.Add($"            {action.AsType()}.performed += instance.On{action.AsType()};");
                        lines.Add($"            {action.AsType()}.canceled += instance.On{action.AsType()};");
                    }
                    break;
                case "RemoveCallbacksSignature":
                    lines.Add($"        public void RemoveCallbacks({interfaceName} instance)");
                    break;
                case "ActionsUnsubscribe":
                    foreach (string action in getActionNames())
                    {
                        lines.Add($"            {action.AsType()}.started -= instance.On{action.AsType()};");
                        lines.Add($"            {action.AsType()}.performed -= instance.On{action.AsType()};");
                        lines.Add($"            {action.AsType()}.canceled -= instance.On{action.AsType()};");
                    }
                    break;
            }

            IEnumerable<string> getActionNames() => map.actions.Select(inputAction => inputAction.name);
        }
    }
}