using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal class InputUserChangeInfoContentBuilder : ContentBuilder
    {
        internal override void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "PlayerIDProperty":
                    if (Helper.OfflineInputData.EnableMultiplayer)
                        info.NewLines.Add($"        public {nameof(PlayerID)} {nameof(PlayerID)} " + @"{ get; }");
                    break;
                case "PlayerIDConstructor":
                    if (Helper.OfflineInputData.EnableMultiplayer)
                        info.NewLines.Add($"            {nameof(PlayerID)} = inputPlayer.ID;");
                    break;
            }
        }

        internal InputUserChangeInfoContentBuilder(OfflineInputData offlineInputData) : base(offlineInputData)
        {
        }
    }
}