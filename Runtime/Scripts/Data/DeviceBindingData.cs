using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Associates one device layout with the binding data used to display its controls.
    /// <para>
    /// Keyed by device rather than by control scheme because a device can appear in any number of control
    /// schemes, and its controls display the same way in all of them. Keying by scheme meant a keyboard
    /// used by three schemes had its entries duplicated three times, each editable independently.
    /// </para>
    /// </summary>
    [Serializable]
    internal struct DeviceBindingData
    {
        /// <summary>The device's layout name in the input system, e.g. "Keyboard" or "DualShockGamepad".</summary>
        [SerializeField] private string deviceLayoutName;
        internal string DeviceLayoutName => deviceLayoutName;

        /// <summary>
        /// Addressable, so a device's display data is in memory only while something is showing it.
        /// </summary>
        [SerializeField] private AssetReference bindingData;
        internal AssetReference BindingData => bindingData;

#if UNITY_EDITOR
        internal const string EDITOR_DeviceLayoutNameField = nameof(deviceLayoutName);
        internal const string EDITOR_BindingDataField = nameof(bindingData);
#endif
    }
}
