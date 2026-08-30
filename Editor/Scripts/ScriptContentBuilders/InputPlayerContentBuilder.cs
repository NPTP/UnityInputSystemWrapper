using NPTP.InputSystemWrapper.Data;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal class InputPlayerContentBuilder : ContentBuilder
    {
        internal override void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "ActionsInstantiation":
                    foreach (string map in Helper.GetMapNames(Asset))
                    {
                        info.NewLines.Add($"            actionMapWrappers.Add(\"{map}\", new {map.AsType()}Actions(ID, Asset, actionWrapperTable));");
                    }
                    break;
            }
        }
        
        internal InputPlayerContentBuilder(OfflineInputData offlineInputData) : base(offlineInputData)
        {
        }
    }
}
