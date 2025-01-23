using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities.Editor;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class InputContextContentBuilder
    {
        internal static void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "Members":
                    OfflineInputData offlineInputData = EditorAssetGetter.GetFirst<OfflineInputData>();
                    foreach (InputContextInfo contextInfo in offlineInputData.InputContexts)
                        info.NewLines.Add($"        {contextInfo.Name},");
                    break;
            }
        }
    }
}