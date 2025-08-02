using NPTP.InputSystemWrapper.Data;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal class InputContextContentBuilder : ContentBuilder
    {
        internal override void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "Members":
                    foreach (InputContextInfo contextInfo in Data.InputContexts)
                        info.NewLines.Add($"        {contextInfo.Name},");
                    break;
            }
        }

        internal InputContextContentBuilder(OfflineInputData offlineInputData) : base(offlineInputData)
        {
        }
    }
}