using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingGetter
    {
        /// <summary>
        /// What to display for a run of bindings. Each binding names its own device, so a scheme spanning
        /// a keyboard and a mouse reads each control from the data for the device it belongs to.
        /// <para>
        /// A device's binding data is loaded here and recorded in held, so whatever owns the result knows
        /// what to give back.
        /// </para>
        /// </summary>
        internal static IReadOnlyList<BindingInfo> GetBindingInfos(InputData inputData, ReadOnlyArray<InputBinding> bindings,
            InputBinding bindingMask, int startIndex, int count, List<AssetReference> held)
        {
            List<BindingInfo> bindingInfos = new();

            for (int i = startIndex; i < startIndex + count; i++)
            {
                InputBinding binding = bindings[i];
                if (!bindingMask.Matches(binding) || !ControlPath.TryParse(binding.effectivePath, out ControlPath controlPath))
                {
                    continue;
                }

                BindingData bindingData = AcquireBindingData(inputData, controlPath.DeviceLayoutName, held);
                if (bindingData == null)
                {
                    continue;
                }

                if (bindingData.TryGetBindingInfo(controlPath.PathOnDevice, out BindingInfo bindingInfo))
                {
                    bindingInfos.Add(bindingInfo);
                }
            }

            return bindingInfos;
        }

        /// <summary>
        /// A device's binding data, loaded and recorded so it can be given back later. Null when the
        /// device has no data to display its controls with.
        /// </summary>
        private static BindingData AcquireBindingData(InputData inputData, string deviceLayoutName, List<AssetReference> held)
        {
            AssetReference reference = inputData.GetBindingData(deviceLayoutName);
            if (reference == null || !reference.RuntimeKeyIsValid())
            {
                ISWDebug.LogWarning($"Device {deviceLayoutName} has no {nameof(BindingData)} and cannot produce display names/sprites!");
                return null;
            }

            BindingData bindingData = BindingDataCache.Acquire(reference);
            if (bindingData == null)
            {
                return null;
            }

            held.Add(reference);
            return bindingData;
        }

        /// <summary>
        /// A control path split at the device, e.g. "&lt;Keyboard&gt;/escape" into "Keyboard" and "escape".
        /// Binding data is keyed by the part after the device, since that part is all a device's own data
        /// needs to describe.
        /// </summary>
        private readonly struct ControlPath
        {
            internal string DeviceLayoutName { get; }
            internal string PathOnDevice { get; }

            private ControlPath(string deviceLayoutName, string pathOnDevice)
            {
                DeviceLayoutName = deviceLayoutName;
                PathOnDevice = pathOnDevice;
            }

            internal static bool TryParse(string effectivePath, out ControlPath controlPath)
            {
                string deviceLayoutName = InputControlPath.TryGetDeviceLayout(effectivePath);
                int deviceEndIndex = effectivePath.IndexOf('>');

                if (string.IsNullOrEmpty(deviceLayoutName) || deviceEndIndex < 0 || deviceEndIndex + 2 > effectivePath.Length)
                {
                    controlPath = default;
                    return false;
                }

                controlPath = new ControlPath(deviceLayoutName.Trim('<', '>'), effectivePath.Substring(deviceEndIndex + 2));
                return true;
            }
        }
    }
}
