using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities.Editor;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class PlayerIDContentBuilder
    {
        internal static void AddContent(InputActionAsset inputActionAsset, string markerName, List<string> lines)
        {
            switch (markerName)
            {
                case "Members":
                    OfflineInputData offlineInputData = EditorAssetGetter.GetFirst<OfflineInputData>();
                    int numPlayers = offlineInputData.EnableMultiplayer ? offlineInputData.MaxPlayers : 1;
                    for (int i = 0; i < numPlayers; i++)
                    {
                        lines.Add($"        Player{i + 1} = {i},");
                    }
                    break;
            }
        }
    }
}