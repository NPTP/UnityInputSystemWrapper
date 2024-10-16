using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Data;
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
                    addEmptyLine();
                    lines.Add($"        private static {nameof(InputPlayer)} Player1 => GetPlayer({nameof(PlayerID)}.{nameof(PlayerID.Player1)});");
                    lines.Add($"        private static bool AllowPlayerJoining => false;");
                    break;
                case "DefaultContextProperty":
                    lines.Add($"        private static {nameof(InputContext)} DefaultContext => {nameof(InputContext)}.{offlineInputData.DefaultContext};");
                    break;
                case "InteractiveRebind": // TODO (multiplayer)
                    string rebindMethodHeader = offlineInputData.EnableMultiplayer
                        ? $"        public static void StartInteractiveRebind(InputActionReferenceWrapper actionReference, PlayerID playerID, ControlScheme controlScheme, Action callback = null)"
                        : $"        public static void StartInteractiveRebind(InputActionReferenceWrapper actionReference, ControlScheme controlScheme, Action callback = null)";
                    string rebindPlayerGetter = $"            {nameof(InputPlayer)} player = {(offlineInputData.EnableMultiplayer ? "GetPlayer(playerID)" : "Player1")};";
                    lines.Add(rebindMethodHeader);
                    lines.Add("        {");
                    lines.Add(rebindPlayerGetter);
                    break;
                case "EnableContextForAllPlayersSignature":
                    string accessor = offlineInputData.EnableMultiplayer ? "public" : "private";
                    lines.Add($"        {accessor} static void EnableContextForAllPlayers({nameof(InputContext)} inputContext)");
                    break;
                case "TryGetCurrentBindingInfo":
                    bool isMultiplayer = offlineInputData.EnableMultiplayer;
                    string methodHeader;
                    string methodBody;
                    if (isMultiplayer)
                    {
                        methodHeader = $"        public static bool TryGetCurrentBindingInfo({nameof(InputActionReferenceWrapper)} actionReferenceWrapper, {nameof(PlayerID)} playerID, out IEnumerable<{nameof(BindingInfo)}> bindingInfos)";
                        methodBody = $"            return BindingGetter.TryGetCurrentBindingInfo(runtimeInputData, GetPlayer(playerID), actionReferenceWrapper, out bindingInfos);";
                    }
                    else
                    {
                        methodHeader = $"        public static bool TryGetCurrentBindingInfo({nameof(InputActionReferenceWrapper)} actionReferenceWrapper, out IEnumerable<{nameof(BindingInfo)}> bindingInfos)";
                        methodBody = $"            return BindingGetter.TryGetCurrentBindingInfo(runtimeInputData, Player1, actionReferenceWrapper, out bindingInfos);";
                    }

                    lines.Add(methodHeader);
                    lines.Add("        {");
                    lines.Add(methodBody);
                    lines.Add("        }");
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
