using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities.Editor;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class PlayerIDContentBuilder
    {
        internal static void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "Members":
                    OfflineInputData offlineInputData = EditorAssetGetter.GetFirst<OfflineInputData>();
                    int numPlayers = offlineInputData.EnableMultiplayer ? offlineInputData.MaxPlayers : 1;
                    for (int i = 0; i < numPlayers; i++)
                    {
                        info.NewLines.Add($"        Player{i + 1} = {i},");
                    }
                    break;
            }
        }
    }
}