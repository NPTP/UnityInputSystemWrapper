using System;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// The runtime description of one control scheme: its name in the input action asset and which device
    /// families it uses. Kept in sync with the input action asset by the input script generator, in the
    /// same order as the generated ControlScheme enum.
    /// <para>
    /// Binding data is not here: it belongs to a device, not a scheme. See <see cref="DeviceBindingData"/>.
    /// </para>
    /// </summary>
    [Serializable]
    internal struct ControlSchemeDefinition
    {
        [SerializeField] private string controlSchemeName;
        internal string ControlSchemeName => controlSchemeName;

        [SerializeField] private ControlSchemeDeviceFamilies deviceFamilies;
        internal ControlSchemeDeviceFamilies DeviceFamilies => deviceFamilies;

        internal ControlSchemeId ToId(int index) => new(index, controlSchemeName, deviceFamilies);

#if UNITY_EDITOR
        internal const string EDITOR_ControlSchemeNameField = nameof(controlSchemeName);
        internal const string EDITOR_DeviceFamiliesField = nameof(deviceFamilies);
#endif
    }
}
