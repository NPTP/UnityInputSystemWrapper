using System;
using NPTP.InputSystemWrapper.Bindings;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Associates one control scheme (by its name in the input action asset) with the binding data
    /// used to display that scheme's bindings on the UI. The list of these on <see cref="RuntimeInputData"/>
    /// is kept in sync with the input action asset by the input script generator.
    /// </summary>
    [Serializable]
    internal struct ControlSchemeBindingDataEntry
    {
        [SerializeField] private string controlSchemeName;
        internal string ControlSchemeName => controlSchemeName;

        [SerializeField] private BindingData bindingData;
        internal BindingData BindingData => bindingData;

#if UNITY_EDITOR
        internal ControlSchemeBindingDataEntry(string controlSchemeName, BindingData bindingData)
        {
            this.controlSchemeName = controlSchemeName;
            this.bindingData = bindingData;
        }

        internal const string EDITOR_ControlSchemeNameField = nameof(controlSchemeName);
        internal const string EDITOR_BindingDataField = nameof(bindingData);
#endif
    }
}
