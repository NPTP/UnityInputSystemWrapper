using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Emits the extension methods that give type-safe access to a player's generated actions and to the
    /// enum-typed views of its state. These cannot be members on InputPlayer itself, because InputPlayer
    /// lives in the package and the generated types do not.
    /// </summary>
    internal static class InputPlayerExtensionsEmitter
    {
        internal static List<string> BuildLines(InputActionAsset asset)
        {
            List<string> lines = new()
            {
                "using System;",
                "using System.Collections.Generic;",
                "using NPTP.InputSystemWrapper;",
                "using NPTP.InputSystemWrapper.Actions;",
                "using NPTP.InputSystemWrapper.Bindings;",
                "using NPTP.InputSystemWrapper.Enums;",
                "using UnityEngine.InputSystem;",
                string.Empty
            };
            lines.AddRange(Helper.GetGeneratorNoticeLines());
            lines.Add($"namespace {GeneratedNamespaces.PLAYER}");
            lines.Add("{");
            lines.Add("    public static class InputPlayerExtensions");
            lines.Add("    {");

            foreach (string mapName in Helper.GetMapNames(asset))
            {
                string type = $"{mapName.AsType()}Actions";
                lines.Add($"        public static {type} {mapName.AsType()}(this InputPlayer inputPlayer) => ({type})inputPlayer.GetActionMapWrapper(\"{mapName}\");");
            }

            lines.Add(string.Empty);
            lines.Add("        public static ControlScheme CurrentControlScheme(this InputPlayer inputPlayer) => (ControlScheme)inputPlayer.CurrentControlSchemeId.Index;");
            lines.Add("        public static ControlScheme ControlScheme(this InputUserChangeInfo info) => (ControlScheme)info.ControlSchemeId.Index;");
            lines.Add(string.Empty);
            lines.Add("        public static InputContext GetInputContext(this InputPlayer inputPlayer) => (InputContext)inputPlayer.InputContextId.Index;");
            lines.Add("        public static void SetInputContext(this InputPlayer inputPlayer, InputContext inputContext) => inputPlayer.InputContextId = inputContext.ToId();");
            lines.Add(string.Empty);
            lines.Add("        public static bool ControlSchemeHas<TDevice>(this InputPlayer inputPlayer, ControlScheme controlScheme) where TDevice : InputDevice =>");
            lines.Add("            inputPlayer.ControlSchemeHas<TDevice>(controlScheme.ToId());");
            lines.Add("    }");
            lines.Add("}");
            lines.Add(string.Empty);
            lines.Add($"namespace {GeneratedNamespaces.ACTIONS}");
            lines.Add("{");
            lines.Add("    public static class ActionWrapperExtensions");
            lines.Add("    {");
            lines.Add("        public static void StartInteractiveRebind(this ActionWrapper actionWrapper, ControlScheme controlScheme, Action<RebindInfo> callback = null) =>");
            lines.Add("            actionWrapper.StartInteractiveRebind(controlScheme.ToId(), callback);");
            lines.Add(string.Empty);
            lines.Add("        public static void StartInteractiveRebind(this ActionWrapper actionWrapper, ControlScheme controlScheme, CompositePart compositePart, Action<RebindInfo> callback = null) =>");
            lines.Add("            actionWrapper.StartInteractiveRebind(controlScheme.ToId(), compositePart, callback);");
            lines.Add(string.Empty);
            lines.Add("        public static bool TryGetBindingInfo(this ActionWrapper actionWrapper, ControlScheme controlScheme, out IEnumerable<BindingInfo> bindingInfos) =>");
            lines.Add("            actionWrapper.TryGetBindingInfo(controlScheme.ToId(), out bindingInfos);");
            lines.Add(string.Empty);
            lines.Add("        public static bool TryGetBindingInfo(this ActionWrapper actionWrapper, ControlScheme controlScheme, CompositePart compositePart, out IEnumerable<BindingInfo> bindingInfos) =>");
            lines.Add("            actionWrapper.TryGetBindingInfo(controlScheme.ToId(), compositePart, out bindingInfos);");
            lines.Add(string.Empty);
            lines.Add("        public static void StartInteractiveRebind(this ActionReference actionReference, ControlScheme controlScheme, Action<RebindInfo> callback = null) =>");
            lines.Add("            actionReference.StartInteractiveRebind(controlScheme.ToId(), callback);");
            lines.Add(string.Empty);
            lines.Add("        public static bool TryGetBindingInfo(this ActionReference actionReference, ControlScheme controlScheme, out IEnumerable<BindingInfo> bindingInfos) =>");
            lines.Add("            actionReference.TryGetBindingInfo(controlScheme.ToId(), out bindingInfos);");
            lines.Add("    }");
            lines.Add("}");
            return lines;
        }
    }
}
