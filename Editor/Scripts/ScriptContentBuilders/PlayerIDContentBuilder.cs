using NPTP.InputSystemWrapper.Data;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal class PlayerIDContentBuilder : ContentBuilder
    {
        internal override void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "Members":
                    int numPlayers = Data.EnableMultiplayer ? Data.MaxPlayers : 1;
                    for (int i = 0; i < numPlayers; i++)
                    {
                        info.NewLines.Add($"        Player{i + 1} = {i},");
                    }
                    break;
            }
        }

        public PlayerIDContentBuilder(OfflineInputData offlineInputData) : base(offlineInputData)
        {
        }
    }
}