using System.Collections.Generic;
using InputSystemWrapper.Utilities.Editor;
using UnityEngine.InputSystem;
using UnityInputSystemWrapper.Data;

namespace UnityInputSystemWrapper.Editor.ScriptContentBuilders
{
    public static class PlayerIDContentBuilder
    {
        public static void AddContent(InputActionAsset inputActionAsset, string markerName, List<string> lines)
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