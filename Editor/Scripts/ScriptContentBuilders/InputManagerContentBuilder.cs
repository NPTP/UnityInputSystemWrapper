using System.Collections.Generic;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Data;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    public static class InputManagerContentBuilder
    {
        private static string MPPublicPlayerGetter => "Player"; 
        private static string SPPrivatePlayerGetter => "GetPlayer"; 
        
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
                    
                    if (enableMp)
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
                        lines.Add($"        public static {nameof(InputPlayer)} {MPPublicPlayerGetter}({nameof(PlayerID)} id) => playerCollection[id];");
                        lines.Add($"        public static IEnumerable<{nameof(InputPlayer)}> Players => playerCollection.Players;");
                        break;
                    }
                    
                    lines.Add($"        private static bool AllowPlayerJoining => false;");
                    lines.Add($"        private static {nameof(InputPlayer)} {SPPrivatePlayerGetter}({nameof(PlayerID)} id) => playerCollection[id];");
                    
                    lines.Add(getSinglePlayerEventWrapperString(nameof(DeviceControlInfo), "OnDeviceControlChanged"));
                    lines.Add(getSinglePlayerEventWrapperString("char", "OnKeyboardTextInput"));
                    foreach (string mapName in Helper.GetMapNames(asset))
                    {
                        lines.Add($"        public static {mapName.AsType()}Actions {mapName.AsType()} => {SPPrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).{mapName.AsType()};");
                    }
                    lines.Add($"        public static {nameof(InputContext)} Context");
                    lines.Add("        {");
                    lines.Add($"            get => {SPPrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).InputContext;");
                    lines.Add($"            set => {SPPrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).InputContext = value;");
                    lines.Add("        }");
                    lines.Add($"        public static {nameof(ControlScheme)} CurrentControlScheme => {SPPrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).CurrentControlScheme;");
                    lines.Add($"        public static {nameof(InputDevice)} LastUsedDevice => {SPPrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).LastUsedDevice;");
                    break;
                case "DefaultContextProperty":
                    lines.Add($"        private static {nameof(InputContext)} DefaultContext => {nameof(InputContext)}.{Helper.OfflineInputData.DefaultContext};");
                    break;
                case "EnableContextForAllPlayersSignature":
                    string accessor = Helper.OfflineInputData.EnableMultiplayer ? "public" : "private";
                    lines.Add($"        {accessor} static void EnableContextForAllPlayers({nameof(InputContext)} inputContext)");
                    break;
                case "TryGetActionBindingInfo":
                    bool isMultiplayer = Helper.OfflineInputData.EnableMultiplayer;
                    string methodHeader;
                    string methodBody;
                    if (isMultiplayer)
                    {
                        methodHeader = $"        public static bool TryGetActionBindingInfo({nameof(InputAction)} action, {nameof(PlayerID)} playerID, InputDevice device, out IEnumerable<{nameof(BindingInfo)}> bindingInfos)";
                        methodBody = $"            return InputBindings.TryGetActionBindingInfo(runtimeInputData, action, device, out bindingInfos);";
                    }
                    else
                    {
                        methodHeader = $"        public static bool TryGetActionBindingInfo({nameof(InputAction)} action, InputDevice device, out IEnumerable<{nameof(BindingInfo)}> bindingInfos)";
                        methodBody = $"            return InputBindings.TryGetActionBindingInfo(runtimeInputData, action, device, out bindingInfos);";
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
                       $"            add => {SPPrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).{eventName} += value;\n" +
                       $"            remove => {SPPrivatePlayerGetter}({nameof(PlayerID)}.{PlayerID.Player1}).{eventName} -= value;\n" +
                       "        }";
            }
        }
    }
}
