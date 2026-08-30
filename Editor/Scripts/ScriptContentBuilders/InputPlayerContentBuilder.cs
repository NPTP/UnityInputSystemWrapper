using NPTP.InputSystemWrapper.Data;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal class InputPlayerContentBuilder : ContentBuilder
    {
        internal override void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "ActionsProperties":
                    foreach (string mapName in Helper.GetMapNames(Asset))
                        info.NewLines.Add($"        public {mapName.AsProperty()}Actions {mapName.AsProperty()}" + " { get; }");
                    break;
                case "ActionsInstantiation":
                    foreach (string map in Helper.GetMapNames(Asset))
                    {
                        info.NewLines.Add($"            {map.AsProperty()} = new {map.AsType()}Actions(ID, Asset, actionWrapperTable);");
                        info.NewLines.Add($"            actionMapWrappers.Add(\"{map}\", {map.AsProperty()});");
                    }
                    break;
            }
        }
        
        internal InputPlayerContentBuilder(OfflineInputData offlineInputData) : base(offlineInputData)
        {
        }
    }
}
