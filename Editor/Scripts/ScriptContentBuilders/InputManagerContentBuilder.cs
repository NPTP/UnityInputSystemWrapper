using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityInputSystemWrapper.Data;

namespace UnityInputSystemWrapper.Editor.ScriptContentBuilders
{
    public static class InputManagerContentBuilder
    {
        public static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
        {
            switch (markerName)
            {
                case "RuntimeInputDataPath":
                    lines.Add($"        private const string RUNTIME_INPUT_DATA_PATH = \"{OfflineInputData.RUNTIME_INPUT_DATA_PATH}\";");
                    break;
                case "SingleOrMultiPlayerFieldsAndProperties":
                    OfflineInputData offlineInputData = Helper.OfflineInputData;
                    bool enableMp = offlineInputData.EnableMultiplayer;
                    int maxPlayers = offlineInputData.MaxPlayers;
                    
                    lines.Add($"        {(enableMp ? "public" : "private")} static {nameof(InputPlayer)} Player({nameof(PlayerID)} id) => playerCollection[id];");
                    
                    if (enableMp)
                    {
                        break;
                    }
                    
                    lines.Add(getSinglePlayerEventWrapperString(nameof(ControlScheme), "OnControlSchemeChanged"));
                    lines.Add(getSinglePlayerEventWrapperString("char", "OnKeyboardTextInput"));
                    foreach (string mapName in Helper.GetMapNames(asset))
                    {
                        lines.Add($"        public static {mapName.AsType()}Actions {mapName.AsType()} => GetPlayer({nameof(PlayerID)}.{PlayerID.Player1}).{mapName.AsType()};");
                    }
                    lines.Add($"        public static {nameof(InputContext)} CurrentContext => GetPlayer({nameof(PlayerID)}.{PlayerID.Player1}).CurrentContext;");
                    lines.Add($"        public static {nameof(ControlScheme)} CurrentControlScheme => GetPlayer({nameof(PlayerID)}.{PlayerID.Player1}).CurrentControlScheme;");
                    lines.Add($"        public static void EnableContext({nameof(InputContext)} context) => GetPlayer({nameof(PlayerID)}.{PlayerID.Player1}).CurrentContext = context;");
                    break;
                case "DefaultContextProperty":
                    lines.Add($"        private static {nameof(InputContext)} DefaultContext => {nameof(InputContext)}.{Helper.OfflineInputData.DefaultContext};");
                    break;
            }

            string getSinglePlayerEventWrapperString(string parameterName, string eventName)
            {
                return $"        public static event Action<{parameterName}> {eventName}\n" +
                       "        {\n" +
                       $"            add => GetPlayer({nameof(PlayerID)}.{PlayerID.Player1}).{eventName} += value;\n" +
                       $"            remove => GetPlayer({nameof(PlayerID)}.{PlayerID.Player1}).{eventName} -= value;\n" +
                       "        }";
            }
        }
    }
}
