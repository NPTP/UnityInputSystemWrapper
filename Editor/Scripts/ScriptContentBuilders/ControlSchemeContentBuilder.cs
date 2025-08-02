using System.Linq;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Utilities.Extensions;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.ScriptContentBuilders
{
    internal class ControlSchemeContentBuilder : ContentBuilder
    {
        internal override void AddContent(InputScriptGeneratorMarkerInfo info)
        {
            switch (info.MarkerName)
            {
                case "Members":
                    foreach (InputControlScheme controlScheme in Asset.controlSchemes)
                        info.NewLines.Add($"        {controlScheme.name.AsEnumMember()},");
                    break;
                case "IsMouseBased":
                    PopulateMouseAndGamepadBasedMethods(info, ControlSchemeBasis.BasisSpec.IsMouseBased);
                    break;
                case "IsGamepadBased":
                    PopulateMouseAndGamepadBasedMethods(info, ControlSchemeBasis.BasisSpec.IsGamepadBased);
                    break;
                case "EnumToStringSwitch":
                    foreach (InputControlScheme controlScheme in Asset.controlSchemes)
                        info.NewLines.Add($"                {nameof(ControlScheme)}.{controlScheme.name.AsEnumMember()} => \"{controlScheme.name}\",");
                    break;
                case "StringToEnumSwitch":
                    foreach (InputControlScheme controlScheme in Asset.controlSchemes)
                        info.NewLines.Add($"                \"{controlScheme.name}\" => {nameof(ControlScheme)}.{controlScheme.name.AsEnumMember()},");
                    break;
            }
        }

        private void PopulateMouseAndGamepadBasedMethods(InputScriptGeneratorMarkerInfo info, ControlSchemeBasis.BasisSpec basisSpec)
        {
            if (Data.ControlSchemeBases.IsNullOrEmpty())
                return;
            
            foreach (InputControlScheme controlScheme in Asset.controlSchemes)
            {
                string enumMemberName = controlScheme.name.AsEnumMember();
                bool match = Data.ControlSchemeBases.Any(basis => basis.ControlScheme.ToString() == enumMemberName && basis.Basis == basisSpec);
                info.NewLines.Add($"                {nameof(ControlScheme)}.{enumMemberName} => {match.ToString().ToLower()},");
            }
        }

        internal ControlSchemeContentBuilder(OfflineInputData offlineInputData) : base(offlineInputData)
        {
        }
    }
}