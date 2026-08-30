using System.Collections.Generic;
using System.Linq;
using NPTP.InputSystemWrapper.Data;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Emits the ControlScheme and InputContext enums and the extension methods that convert them to the
    /// id types the package runtime works in. Enum values are emitted in asset order so that the cast
    /// between an enum value and an id index is exact.
    /// </summary>
    internal static class EnumEmitter
    {
        internal static List<string> BuildControlSchemeLines(InputActionAsset asset)
        {
            List<string> lines = Header();

            lines.Add("    public enum ControlScheme");
            lines.Add("    {");
            lines.Add("        /// <summary>");
            lines.Add("        /// Corresponds to \"Null\" string for newly created, unassigned players in Unity's PlayerInput.");
            lines.Add("        /// </summary>");
            lines.Add("        None = -1,");
            lines.AddRange(asset.controlSchemes.Select((controlScheme, i) => $"        {controlScheme.name.AsEnumMember()} = {i},"));
            lines.Add("    }");
            lines.Add(string.Empty);
            lines.Add("    public static class ControlSchemeExtensions");
            lines.Add("    {");
            lines.Add("        internal static ControlSchemeId ToId(this ControlScheme controlScheme) => InputRuntime.Current.GetControlSchemeId((int)controlScheme);");
            lines.Add(string.Empty);
            lines.Add("        public static bool IsMouseBased(this ControlScheme controlScheme) => controlScheme.ToId().IsMouseBased;");
            lines.Add("        public static bool IsGamepadBased(this ControlScheme controlScheme) => controlScheme.ToId().IsGamepadBased;");
            lines.Add("    }");
            lines.Add("}");
            return lines;
        }

        internal static List<string> BuildInputContextLines(InputContextInfo[] inputContexts)
        {
            List<string> lines = Header();

            lines.Add("    public enum InputContext");
            lines.Add("    {");
            if (inputContexts == null || inputContexts.Length == 0)
            {
                lines.Add("        // >>> WARNING: No InputContexts have been defined in your OfflineInputData asset.");
                lines.Add("        // >>> Add at least 1 InputContext, then re-run input code generation.");
            }
            else
            {
                lines.AddRange(inputContexts.Select((context, i) => $"        {context.Name.AsEnumMember()} = {i},"));
            }

            lines.Add("    }");
            lines.Add(string.Empty);
            lines.Add("    public static class InputContextExtensions");
            lines.Add("    {");
            lines.Add("        internal static InputContextId ToId(this InputContext inputContext) => new((int)inputContext);");
            lines.Add("    }");
            lines.Add("}");
            return lines;
        }

        private static List<string> Header()
        {
            List<string> lines = new() { "using NPTP.InputSystemWrapper;" , string.Empty };
            lines.AddRange(Helper.GetGeneratorNoticeLines());
            lines.Add($"namespace {GeneratedNamespaces.ENUMS}");
            lines.Add("{");
            return lines;
        }
    }
}
