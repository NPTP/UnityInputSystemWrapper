using System;
using System.Linq;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums.NPTP.InputSystemWrapper;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal class InputManagerContentBuilder : ContentBuilder
    {
        internal override void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            void addEmptyLine() => info.NewLines.Add(string.Empty);

            switch (info.MarkerName)
            {
                case "RuntimeInputDataPath":
                    info.NewLines.Add($"        private const string RUNTIME_INPUT_DATA_PATH = \"{OfflineInputData.RUNTIME_INPUT_DATA_PATH}\";");
                    break;
                case "SingleOrMultiPlayerFieldsAndProperties":
                    if (Data.EnableMultiplayer)
                    {
                        info.NewLines.Add("        private static bool allowPlayerJoining;\n" +
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
                        info.NewLines.Add($"        public static {nameof(InputPlayer)} Player({nameof(PlayerID)} id) => GetPlayer(id);");
                        addEmptyLine();
                        info.NewLines.Add($"        public static IEnumerable<{nameof(InputPlayer)}> Players => playerCollection.Players;");
                        addEmptyLine();
                        break;
                    }
                    info.NewLines.Add(getSinglePlayerEventWrapperString(nameof(InputUserChangeInfo), "OnInputUserChange"));
                    addEmptyLine();
                    info.NewLines.Add(getSinglePlayerEventWrapperString(nameof(ControlScheme), "OnControlSchemeChanged"));
                    addEmptyLine();
                    info.NewLines.Add(getSinglePlayerEventWrapperString("char", "OnKeyboardTextInput"));
                    addEmptyLine();
                    string[] mapNames = Helper.GetMapNames(Asset).ToArray();
                    info.NewLines.AddRange(mapNames.Select(mapName => $"        public static {mapName.AsType()}Actions {mapName.AsType()} => Player1.{mapName.AsType()};"));
                    if (mapNames.Length > 0) addEmptyLine();
                    info.NewLines.Add($"        public static {nameof(InputContext)} Context");
                    info.NewLines.Add("        {");
                    info.NewLines.Add($"            get => Player1.InputContext;");
                    info.NewLines.Add($"            set => Player1.InputContext = value;");
                    info.NewLines.Add("        }");
                    addEmptyLine();
                    info.NewLines.Add($"        public static {nameof(ControlScheme)} CurrentControlScheme => Player1.CurrentControlScheme;");
                    info.NewLines.Add($"        public static Vector2 MousePosition => Mouse.current.position.ReadValue();");
                    addEmptyLine();
                    info.NewLines.Add($"        private static {nameof(InputPlayer)} Player1 => GetPlayer({nameof(PlayerID)}.{nameof(PlayerID.Player1)});");
                    info.NewLines.Add($"        private static bool AllowPlayerJoining => false;");
                    break;
                case "DefaultContextProperty":
                    string defaultContextValue = $"{nameof(InputContext)}.{Data.DefaultContext}";
                    if (Data.InputContexts.Length == 0)
                    {
                        info.NewLines.Add(">>> WARNING: No InputContexts have been defined in your OfflineInputData asset. Comment out this line to allow recompilation, add at least 1 InputContext, then re-save the asset.");
                        defaultContextValue = "0";
                    }
                    else if (Enum.GetNames(typeof(InputContext)).Length == 0)
                    {
                        defaultContextValue = $"{nameof(InputContext)}.{Data.InputContexts[0].Name}";
                    }
                    info.NewLines.Add($"        private static {nameof(InputContext)} DefaultContext => {defaultContextValue};");
                    break;
                case "Initialize":
                    if (Data.InitializationMode == InitializationMode.BeforeSceneLoad)
                        info.NewLines.Add("        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]");
                    info.NewLines.Add($"        {(Data.InitializationMode == InitializationMode.Manual ? "public" : "private")} static void Initialize()");
                    break;
                case "LoadAllBindingsOnInitialization":
                    if (Data.LoadAllBindingOverridesOnInitialize)
                        info.NewLines.Add("            LoadBindingsForAllPlayers();");
                    break;
                case "EnableContextForAllPlayersSignature":
                    string accessor = Data.EnableMultiplayer ? "public" : "private";
                    info.NewLines.Add($"        {accessor} static void EnableContextForAllPlayers({nameof(InputContext)} inputContext)");
                    break;
                case "PlayerGetter":
                    string playerGetter = Data.EnableMultiplayer
                        ? $"            {nameof(InputPlayer)} player = GetPlayer(playerID);"
                        : $"            {nameof(InputPlayer)} player = Player1;";
                    info.NewLines.Add(playerGetter);
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

        public InputManagerContentBuilder(OfflineInputData offlineInputData) : base(offlineInputData)
        {
        }
    }
}
