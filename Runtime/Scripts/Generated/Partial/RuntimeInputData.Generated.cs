using System;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Data
{
    internal partial class RuntimeInputData
    {
        // MARKER.ControlSchemeBindingData.Start
        // MARKER.ControlSchemeBindingData.End

        internal BindingData GetControlSchemeBindingData(ControlScheme controlScheme)
        {
            return controlScheme switch
            {
                // MARKER.EnumToBindingDataSwitch.Start
                // MARKER.EnumToBindingDataSwitch.End
                _ => throw new ArgumentOutOfRangeException(nameof(controlScheme), controlScheme, null)
            };
        }
    }
}