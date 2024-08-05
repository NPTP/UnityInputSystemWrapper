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
                case "RuntimeInputAddress":
                    lines.Add($"        private const string RUNTIME_INPUT_DATA_PATH = \"{OfflineInputData.RUNTIME_INPUT_DATA_PATH}\";");
                    break;
                case "SingleOrMultiPlayerFieldsAndProperties":
                    if (!Helper.OfflineInputData.SinglePlayerOnly)
                    {
                        lines.Add($"        public static bool TryGetPlayer(int id, out {nameof(InputPlayer)} inputPlayer) => playerCollection.TryGetPlayer(id, out inputPlayer);");
                        break;
                    }
                    lines.Add(getSinglePlayerEventWrapperString(nameof(ControlScheme), "OnControlSchemeChanged"));
                    lines.Add(getSinglePlayerEventWrapperString("char", "OnKeyboardTextInput"));
                    foreach (string mapName in Helper.GetMapNames(asset))
                        lines.Add($"        public static {mapName.AsType()}Actions {mapName.AsType()} => primaryPlayer.{mapName.AsType()};");
                    lines.Add($"        public static {nameof(InputContext)} CurrentContext => primaryPlayer.CurrentContext;");
                    lines.Add($"        public static {nameof(ControlScheme)} CurrentControlScheme => primaryPlayer.CurrentControlScheme;");
                    lines.Add($"        public static void EnableContext({nameof(InputContext)} context) => primaryPlayer.EnableContext(context);");
                    break;
                case "DefaultContextProperty":
                    lines.Add($"        private static {nameof(InputContext)} DefaultContext => {nameof(InputContext)}.{Helper.OfflineInputData.DefaultContext};");
                    break;
            }

            string getSinglePlayerEventWrapperString(string parameterName, string eventName)
            {
                return $"        public static event Action<{parameterName}> {eventName}\n" +
                       "        {\n" +
                       $"            add => primaryPlayer.{eventName} += value;\n" +
                       $"            remove => primaryPlayer.{eventName} -= value;\n" +
                       "        }";
            }
        }
    }
}
