using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums.NPTP.InputSystemWrapper;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Emits the ISW facade: the public, enum-typed entry point users call, forwarding to the package's
    /// InputRuntime. It also wires the factory that lets InputPlayer build its generated actions objects.
    /// </summary>
    internal static class ISWEmitter
    {
        internal static List<string> BuildLines(InputActionAsset asset, OfflineInputData offlineInputData)
        {
            List<string> lines = new()
            {
                "using System;",
                "using System.Collections.Generic;",
                "using NPTP.InputSystemWrapper.Actions;",
                "using NPTP.InputSystemWrapper.AnyButtonPress;",
                "using NPTP.InputSystemWrapper.Bindings;",
                "using NPTP.InputSystemWrapper.Enums;",
                "using NPTP.InputSystemWrapper.Player;",
                "using NPTP.InputSystemWrapper.Utilities;",
                "using UnityEngine;",
                "using UnityEngine.InputSystem;",
                string.Empty
            };

            lines.AddRange(Helper.GetGeneratorNoticeLines());
            lines.Add($"namespace {GeneratedNamespaces.ROOT}");
            lines.Add("{");
            lines.Add("    /// <summary>");
            lines.Add("    /// Main point of usage for all input in the game. ISW stands for \"Input System Wrapper\".");
            lines.Add("    /// </summary>");
            lines.Add("    public static class ISW");
            lines.Add("    {");
            lines.Add("        private static InputRuntime Runtime => InputRuntime.Current;");
            lines.Add("        private static InputPlayer DefaultPlayer => Runtime.DefaultPlayer;");
            lines.Add(string.Empty);

            foreach (string mapName in Helper.GetMapNames(asset))
                lines.Add($"        public static {mapName.AsType()}Actions {mapName.AsType()} => DefaultPlayer.{mapName.AsType()}();");

            lines.Add("        public static ControlScheme CurrentControlScheme => DefaultPlayer.CurrentControlScheme();");
            lines.Add(string.Empty);
            lines.AddRange(InitializeLines(offlineInputData, asset));
            lines.AddRange(EventLines());
            lines.AddRange(InterfaceLines());
            lines.Add("    }");
            lines.Add("}");
            return lines;
        }

        private static IEnumerable<string> InitializeLines(OfflineInputData offlineInputData, InputActionAsset asset)
        {
            List<string> lines = new();
            if (offlineInputData.InitializationMode == InitializationMode.BeforeSceneLoad)
                lines.Add("        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]");

            string accessibility = offlineInputData.InitializationMode == InitializationMode.Manual ? "public" : "private";
            lines.Add($"        {accessibility} static void Initialize()");
            lines.Add("        {");
            lines.Add("            InputPlayer.ActionMapWrapperFactory = CreateActionMapWrappers;");
            lines.Add("            InputRuntime.Initialize();");
            lines.Add("        }");
            lines.Add(string.Empty);
            lines.Add("        private static void CreateActionMapWrappers(InputPlayer player, Dictionary<string, IActionMapWrapper> wrappers)");
            lines.Add("        {");

            foreach (string mapName in Helper.GetMapNames(asset))
                lines.Add($"            wrappers.Add(\"{mapName}\", new {mapName.AsType()}Actions(player.ID, player.Asset, player.ActionWrapperTable));");

            lines.Add("        }");
            lines.Add(string.Empty);
            return lines;
        }

        private static IEnumerable<string> EventLines()
        {
            return new List<string>
            {
                Event("Action<LocalizedStringRequest>", "OnLocalizedStringRequested"),
                Event("Action", "OnControlsUpdated"),
                Event("AnyButtonPressListener", "OnAnyButtonPress"),
                Event("Action", "OnBindingsChanged"),
                Event("Action<InputUserChangeInfo>", "OnAnyPlayerInputUserChange"),
                Event("Action<InputPlayer>", "OnAnyPlayerControlSchemeChanged"),
                Event("Action<char>", "OnAnyPlayerKeyboardTextInput"),
                string.Empty
            };
        }

        private static string Event(string handlerType, string name)
        {
            return $"        public static event {handlerType} {name} {{ add => Runtime.{name} += value; remove => Runtime.{name} -= value; }}";
        }

        private static IEnumerable<string> InterfaceLines()
        {
            return new List<string>
            {
                "        public static bool AllowPlayerJoining { get => Runtime.AllowPlayerJoining; set => Runtime.AllowPlayerJoining = value; }",
                "        public static Vector2 MousePosition => Mouse.current.position.ReadValue();",
                string.Empty,
                "        public static InputPlayer GetPlayer(int playerID) => Runtime.GetPlayer(playerID);",
                "        public static void AddPlayer(int playerID) => Runtime.AddPlayer(playerID);",
                "        public static void RemovePlayer(int playerID) => Runtime.RemovePlayer(playerID);",
                string.Empty,
                "        public static bool ControlSchemeHas<TDevice>(ControlScheme controlScheme, int playerID = 0) where TDevice : InputDevice =>",
                "            Runtime.ControlSchemeHas<TDevice>(controlScheme.ToId(), playerID);",
                string.Empty,
                "        public static void SetContextForAllPlayers(InputContext inputContext) => Runtime.SetContextForAllPlayers(inputContext.ToId());",
                string.Empty,
                "        /// <summary>",
                "        /// Try to get the ActionWrapper for the (deprecated) InputActionReference's action.",
                "        /// Useful as a transitional tool from normal Unity Input System usage to full ISW integration.",
                "        /// </summary>",
                "        public static bool TryConvert(InputActionReference inputActionReference, int playerID, out ActionWrapper actionWrapper) =>",
                "            Runtime.TryConvert(inputActionReference, playerID, out actionWrapper);",
                string.Empty,
                "        /// <summary>",
                "        /// Single-player overload",
                "        /// </summary>",
                "        public static bool TryConvert(InputActionReference inputActionReference, out ActionWrapper actionWrapper) =>",
                "            Runtime.TryConvert(inputActionReference, out actionWrapper);",
                string.Empty,
                "        public static void ResetBindingForAction(ActionReference actionReference, ControlScheme controlScheme) =>",
                "            Runtime.ResetBindingForAction(actionReference, controlScheme.ToId());",
                "        public static void ResetAllBindingsForControlScheme(ControlScheme controlScheme, int? playerID = null) =>",
                "            Runtime.ResetAllBindingsForControlScheme(controlScheme.ToId(), playerID);",
                "        public static void LoadAllBindings(int? playerID = null) => Runtime.LoadAllBindings(playerID);",
                "        public static void SaveAllBindings(int? playerID = null) => Runtime.SaveAllBindings(playerID);",
                "        public static void ResetAllBindings(int? playerID = 0) => Runtime.ResetAllBindings(playerID);"
            };
        }
    }
}
