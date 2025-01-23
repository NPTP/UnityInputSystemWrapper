using NPTP.InputSystemWrapper.Data;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal sealed class BindingChangerContentBuilder : ContentBuilder
    {
        internal override void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "BindingExcludedPaths":
                    addStringElements(Data.BindingExcludedPaths);
                    break;
                case "BindingCancelPaths":
                    addStringElements(Data.BindingCancelPaths);
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

        public BindingChangerContentBuilder(OfflineInputData offlineInputData) : base(offlineInputData)
        {
        }
    }
}