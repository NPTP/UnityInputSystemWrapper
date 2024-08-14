using System.Collections.Generic;
using System.Linq;
using NPTP.InputSystemWrapper.Utilities.Extensions;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    public static class MapActionsContentBuilder
    {
        public static void AddContent(string markerName, InputActionMap map, List<string> lines)
        {
            string mapName = map.name.AllWhitespaceTrimmed().CapitalizeFirst();
            string className = $"{mapName}Actions";
            string interfaceName = $"I{mapName}Actions";
            
            switch (markerName)
            {
                case "Ignore":
                    break;
                case "GeneratorNotice":
                    lines.AddRange(Helper.GetGeneratorNoticeLines());
                    break;
                case "Namespace":
                    lines.Add($"namespace {Helper.InputNamespace}.{Helper.GENERATED}.{Helper.MAP_ACTIONS}");
                    break;
                case "InterfaceName":
                    lines.Add($"    public interface {interfaceName}");
                    break;
                case "InterfaceMembers":
                    foreach (string action in getActionNamesAsTypes())
                    {
                        lines.Add($"        void On{action}(InputAction.CallbackContext context);");
                    }
                    break;
                case "ClassSignature":
                    lines.Add($"    public class {className} : {interfaceName}");
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
                case "InterfaceMethodImplementations":
                    foreach (string action in getActionNamesAsTypes())
                    {
                        string methodName = $"On{action}";
                        string privateEventName = $"_On{action}";
                        lines.Add($"        void {interfaceName}.{methodName}(InputAction.CallbackContext context) => {privateEventName}?.Invoke(context);");
                    }
                    break;
            }

            IEnumerable<string> getActionNamesAsTypes() => map.actions.Select(inputAction => inputAction.name.AllWhitespaceTrimmed().CapitalizeFirst());
        }
    }
}