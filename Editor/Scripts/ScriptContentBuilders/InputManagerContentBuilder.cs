using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityInputSystemWrapper.Data;

namespace UnityInputSystemWrapper.Editor.ScriptContentBuilders
{
    public static class InputManagerContentBuilder
    {
        private static string PublicPlayerGetter => "Player"; 
        private static string PrivatePlayerGetter => "GetPlayer"; 
        
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
                    
                    if (enableMp)
                    {
                        lines.Add($"        public static {nameof(InputPlayer)} {PublicPlayerGetter}({nameof(PlayerID)} id) => playerCollection[id];");
                        break;
                    }
                    
                    lines.Add($"        private static {nameof(InputPlayer)} {PrivatePlayerGetter}({nameof(PlayerID)} id) => playerCollection[id];");
                    
                    lines.Add(getSinglePlayerEventWrapperString(nameof(ControlScheme), "OnControlSchemeChanged"));
                    lines.Add(getSinglePlayerEventWrapperString("char", "OnKeyboardTextInput"));
                    foreach (string mapName in Helper.GetMapNames(asset))
                    {
                        lines.Add($"        public static {mapName.AsType()}Actions {mapName.AsType()} => {PrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).{mapName.AsType()};");
                    }
                    lines.Add($"        public static {nameof(InputContext)} CurrentContext => {PrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).CurrentContext;");
                    lines.Add($"        public static {nameof(ControlScheme)} CurrentControlScheme => {PrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).CurrentControlScheme;");
                    lines.Add($"        public static void EnableContext({nameof(InputContext)} context) => {PrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).CurrentContext = context;");
                    break;
                case "DefaultContextProperty":
                    lines.Add($"        private static {nameof(InputContext)} DefaultContext => {nameof(InputContext)}.{Helper.OfflineInputData.DefaultContext};");
                    break;
            }

            string getSinglePlayerEventWrapperString(string parameterName, string eventName)
            {
                return $"        public static event Action<{parameterName}> {eventName}\n" +
                       "        {\n" +
                       $"            add => {PrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).{eventName} += value;\n" +
                       $"            remove => {PrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).{eventName} -= value;\n" +
                       "        }";
            }
        }
    }
}
