using System;
using System.Linq;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums.NPTP.InputSystemWrapper;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal class ISWContentBuilder : ContentBuilder
    {
        private const string DEFAULT_PLAYER_FIELD = "DefaultPlayer";
        
        internal override void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "SinglePlayerFieldsAndProperties":
                    string[] mapNames = Helper.GetMapNames(Asset).ToArray();
                    info.NewLines.AddRange(mapNames.Select(mapName => $"        public static {mapName.AsType()}Actions {mapName.AsType()} => {DEFAULT_PLAYER_FIELD}.{mapName.AsType()}();"));
                    info.NewLines.Add($"        public static {nameof(ControlScheme)} CurrentControlScheme => {DEFAULT_PLAYER_FIELD}.CurrentControlScheme;");
                    break;
                case "Initialize":
                    if (Data.InitializationMode == InitializationMode.BeforeSceneLoad)
                        info.NewLines.Add("        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]");
                    info.NewLines.Add($"        {(Data.InitializationMode == InitializationMode.Manual ? "public" : "private")} static void Initialize()");
                    break;
            }
        }

        internal ISWContentBuilder(OfflineInputData offlineInputData) : base(offlineInputData)
        {
        }
    }
}
