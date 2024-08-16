using System.Collections.Generic;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    public static class DeviceControlInfoContentBuilder
    {
        public static void AddContent(InputActionAsset asset, string markerName, List<string> lines)
        {
            switch (markerName)
            {
                case "PlayerIDProperty":
                    if (Helper.OfflineInputData.EnableMultiplayer)
                        lines.Add($"        public {nameof(PlayerID)} {nameof(PlayerID)} " + @"{ get; }");
                    break;
                case "PlayerIDConstructor":
                    if (Helper.OfflineInputData.EnableMultiplayer)
                        lines.Add($"            {nameof(PlayerID)} = inputPlayer.ID;");
                    break;
            }
        }
    }
}