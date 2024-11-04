using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums.NPTP.InputSystemWrapper;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class InputManagerContentBuilder
    {
        internal static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
        {
            void addEmptyLine() => lines.Add(string.Empty);
            OfflineInputData offlineInputData = Helper.OfflineInputData;

            switch (markerName)
            {
                case "RuntimeInputDataPath":
                    lines.Add($"        private const string RUNTIME_INPUT_DATA_PATH = \"{OfflineInputData.RUNTIME_INPUT_DATA_PATH}\";");
                    break;
                case "SingleOrMultiPlayerFieldsAndProperties":
                    if (offlineInputData.EnableMultiplayer)
                    {
                        lines.Add("        private static bool allowPlayerJoining;\n" +
                                  "        public static bool AllowPlayerJoining\n" +
                                  "        {\n" +
                                  "            get => allowPlayerJoining;\n" +
                                  "            set\n" +
                                  "            {\n" +
                                  "                if (value == allowPlayerJoining) return;\n" +
                                  "                allowPlayerJoining = value;\n" +
                                  "                ListenForAnyButtonPress = value ? listenForAnyButtonPress + 1 : listenForAnyButtonPress - 1;\n" +
                                  "            }\n" +
                                  "        }");
                        addEmptyLine();
                        lines.Add($"        public static {nameof(InputPlayer)} Player({nameof(PlayerID)} id) => GetPlayer(id);");
                        addEmptyLine();
                        lines.Add($"        public static IEnumerable<{nameof(InputPlayer)}> Players => playerCollection.Players;");
                        addEmptyLine();
                        break;
                    }
                    lines.Add(getSinglePlayerEventWrapperString(nameof(InputUserChangeInfo), "OnInputUserChange"));
                    addEmptyLine();
                    lines.Add(getSinglePlayerEventWrapperString(nameof(ControlScheme), "OnControlSchemeChanged"));
                    addEmptyLine();
                    lines.Add(getSinglePlayerEventWrapperString("char", "OnKeyboardTextInput"));
                    addEmptyLine();
                    foreach (string mapName in Helper.GetMapNames(asset))
                        lines.Add($"        public static {mapName.AsType()}Actions {mapName.AsType()} => Player1.{mapName.AsType()};");
                    addEmptyLine();
                    lines.Add($"        public static {nameof(InputContext)} Context");
                    lines.Add("        {");
                    lines.Add($"            get => Player1.InputContext;");
                    lines.Add($"            set => Player1.InputContext = value;");
                    lines.Add("        }");
                    addEmptyLine();
                    lines.Add($"        public static {nameof(ControlScheme)} CurrentControlScheme => Player1.CurrentControlScheme;");
                    lines.Add($"        public static Vector2 MousePosition => Mouse.current.position.ReadValue();");
                    addEmptyLine();
                    lines.Add($"        private static {nameof(InputPlayer)} Player1 => GetPlayer({nameof(PlayerID)}.{nameof(PlayerID.Player1)});");
                    lines.Add($"        private static bool AllowPlayerJoining => false;");
                    break;
                case "DefaultContextProperty":
                    lines.Add($"        private static {nameof(InputContext)} DefaultContext => {nameof(InputContext)}.{offlineInputData.DefaultContext};");
                    break;
                case "InitializeBeforeSceneLoad":
                    if (offlineInputData.InitializationMode == InitializationMode.BeforeSceneLoad)
                    {
                        lines.Add("        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]");
                        lines.Add("        private static void InitializeBeforeSceneLoad() => Initialize();");
                    }
                    break;
                case "Initialize":
                    string initializeAccessor = offlineInputData.InitializationMode == InitializationMode.Manual
                        ? "public"
                        : "private";
                    lines.Add($"        {initializeAccessor} static void Initialize()");
                    break;
                case "StartInteractiveRebind": 
                    string rebindMethodHeader = offlineInputData.EnableMultiplayer
                        ? $"        public static void StartInteractiveRebind({nameof(ActionReference)} actionReference, {nameof(PlayerID)} playerID, {nameof(ControlScheme)} controlScheme, Action callback = null)"
                        : $"        public static void StartInteractiveRebind({nameof(ActionReference)} actionReference, {nameof(ControlScheme)} controlScheme, Action callback = null)";
                    lines.Add(rebindMethodHeader);
                    break;
                case "EnableContextForAllPlayersSignature":
                    string accessor = offlineInputData.EnableMultiplayer ? "public" : "private";
                    lines.Add($"        {accessor} static void EnableContextForAllPlayers({nameof(InputContext)} inputContext)");
                    break;
                case "TryGetCurrentBindingInfo":
                    string methodHeader = offlineInputData.EnableMultiplayer
                        ? $"        public static bool TryGetCurrentBindingInfo({nameof(ActionReference)} actionReference, {nameof(PlayerID)} playerID, out IEnumerable<{nameof(BindingInfo)}> bindingInfos)"
                        : $"        public static bool TryGetCurrentBindingInfo({nameof(ActionReference)} actionReference, out IEnumerable<{nameof(BindingInfo)}> bindingInfos)";
                    lines.Add(methodHeader);
                    break;
                case "PlayerGetter":
                    string playerGetter = offlineInputData.EnableMultiplayer
                        ? $"            {nameof(InputPlayer)} player = GetPlayer(playerID);"
                        : $"            {nameof(InputPlayer)} player = Player1;";
                    lines.Add(playerGetter);
                    break;
            }

            string getSinglePlayerEventWrapperString(string parameterName, string eventName)
            {
                return $"        public static event Action<{parameterName}> {eventName}\n" +
                       "        {\n" +
                       $"            add => Player1.{eventName} += value;\n" +
                       $"            remove => Player1.{eventName} -= value;\n" +
                       "        }";
            }
        }
    }
}
