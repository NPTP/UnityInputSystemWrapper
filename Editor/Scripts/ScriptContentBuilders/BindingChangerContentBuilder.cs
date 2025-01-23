using NPTP.InputSystemWrapper.Data;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal static class BindingChangerContentBuilder
    {
        internal static void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            OfflineInputData offlineInputData = Helper.OfflineInputData;
            
            switch (info.MarkerName)
            {
                case "BindingExcludedPaths":
                    addStringElements(offlineInputData.BindingExcludedPaths);
                    break;
                case "BindingCancelPaths":
                    addStringElements(offlineInputData.BindingCancelPaths);
                    break;
                
                void addStringElements(string[] source)
                {
                    for (int i = 0; i < source.Length; i++)
                    {
                        string element = $"            \"{source[i]}\"";
                        if (i < source.Length - 1) element += ",";
                        info.NewLines.Add(element);
                    }
                }
            }
        }
    }
}