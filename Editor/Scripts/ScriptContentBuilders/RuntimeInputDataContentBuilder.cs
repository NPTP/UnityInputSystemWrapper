using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal class RuntimeInputDataContentBuilder : ContentBuilder
    {
        internal override void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "ControlSchemeBindingData":
                    if (Asset.controlSchemes.Count > 0)
                        info.NewLines.Add($"        [Header(\"Input Device Binding Data (Auto-Generated List)\")] [Space]");
                    foreach (InputControlScheme controlScheme in Asset.controlSchemes)
                        info.NewLines.Add($"        [SerializeField] private {nameof(BindingData)} {controlScheme.name.AsField()}BindingData;");
                    break;
                case "EnumToBindingDataSwitch":
                    foreach (InputControlScheme controlScheme in Asset.controlSchemes)
                        info.NewLines.Add($"                {nameof(ControlScheme)}.{controlScheme.name.AsEnumMember()} => {controlScheme.name.AsField()}BindingData,");
                    break;
            }
        }

        public RuntimeInputDataContentBuilder(OfflineInputData offlineInputData) : base(offlineInputData)
        {
        }
    }
}