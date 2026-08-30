using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    /// <summary>
    /// Emits the extension methods that give type-safe per-player access to each generated actions class,
    /// e.g. player.UI(). These cannot be properties on InputPlayer itself, because InputPlayer lives in
    /// the package and the generated actions classes do not.
    /// </summary>
    internal static class InputPlayerActionsExtensionsBuilder
    {
        internal static IEnumerable<string> BuildLines(InputActionAsset asset, string generatedNamespace)
        {
            List<string> lines = new()
            {
                "using NPTP.InputSystemWrapper.Player;",
                string.Empty,
                "// ReSharper disable once CheckNamespace",
                $"namespace {generatedNamespace}",
                "{",
                "    /// <summary>",
                "    /// Type-safe access to each player's generated actions. Auto-generated - do not edit.",
                "    /// </summary>",
                "    public static class InputPlayerActionsExtensions",
                "    {"
            };

            bool first = true;
            foreach (string mapName in Helper.GetMapNames(asset))
            {
                if (!first) lines.Add(string.Empty);
                first = false;
                string type = $"{mapName.AsType()}Actions";
                lines.Add($"        public static {type} {mapName.AsType()}(this InputPlayer inputPlayer)");
                lines.Add("        {");
                lines.Add($"            return ({type})inputPlayer.GetActionMapWrapper(\"{mapName}\");");
                lines.Add("        }");
            }

            lines.Add("    }");
            lines.Add("}");
            return lines;
        }
    }
}
