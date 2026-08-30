using System;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// The runtime description of one control scheme: its name in the input action asset, the binding data
    /// used to display its bindings on the UI, and which device family it is based on. Kept in sync with the
    /// input action asset by the input script generator, in the same order as the generated ControlScheme enum.
    /// </summary>
    [Serializable]
    internal struct ControlSchemeDefinition
    {
        [SerializeField] private string controlSchemeName;
        internal string ControlSchemeName => controlSchemeName;

        [SerializeField] private BindingData bindingData;
        internal BindingData BindingData => bindingData;
        
        [SerializeField] private ControlSchemeBasisSpec basis;
        internal ControlSchemeBasisSpec Basis => basis;

        internal ControlSchemeId ToId(int index) => new(index, controlSchemeName, basis);

#if UNITY_EDITOR
        internal const string EDITOR_ControlSchemeNameField = nameof(controlSchemeName);
        internal const string EDITOR_BindingDataField = nameof(bindingData);
        internal const string EDITOR_BasisField = nameof(basis);
#endif
    }
}
