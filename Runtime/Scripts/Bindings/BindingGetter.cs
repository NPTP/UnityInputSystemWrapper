using System.Collections.Generic;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingGetter
    {
        internal static bool TryGetBindingInfo(RuntimeInputData runtimeInputData, ActionBindingInfo actionBindingInfo, out IEnumerable<BindingInfo> bindingInfos)
        {
            bindingInfos = default;

            // Each binding names the device it is on, so binding data is looked up per binding rather than
            // once for the whole control scheme. A scheme spanning a keyboard and a mouse therefore reads
            // each control from the data for the device that control actually belongs to.
            if (!TryGetControlPaths(actionBindingInfo, actionBindingInfo.ControlSchemeId, out List<ControlPath> controlPaths))
            {
                return false;
            }

            List<BindingInfo> bindingInfoList = new();
            foreach (ControlPath controlPath in controlPaths)
            {
                BindingData bindingData = runtimeInputData.GetBindingData(controlPath.DeviceLayoutName);
                if (bindingData == null)
                {
                    ISWDebug.LogWarning($"Device {controlPath.DeviceLayoutName} has no {nameof(BindingData)} and cannot produce display names/sprites!");
                    continue;
                }

                if (bindingData.TryGetBindingInfo(controlPath.PathOnDevice, out BindingInfo bindingInfo))
                {
                    bindingInfoList.Add(bindingInfo);
                }
            }

            bindingInfos = bindingInfoList;
            return bindingInfoList.Count > 0;
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

        private static bool TryGetControlPaths(ActionBindingInfo actionBindingInfo, ControlSchemeId controlSchemeId, out List<ControlPath> controlPaths)
        {
            List<ControlPath> paths = new();
            InputBinding bindingMask = controlSchemeId.ToBindingMask();

            for (int i = 0; i < actionBindingInfo.ActionWrapper.InputAction.bindings.Count; i++)
            {
                InputBinding binding = actionBindingInfo.ActionWrapper.InputAction.bindings[i];
                if (bindingMask.Matches(binding) && (actionBindingInfo.DontUseCompositePart || actionBindingInfo.CompositePart.Matches(binding)) &&
                    ControlPath.TryParse(binding.effectivePath, out ControlPath controlPath))
                {
                    paths.Add(controlPath);
                }
            }

            controlPaths = paths;
            return controlPaths.Count > 0;
        }

        internal static bool TryGetFirstBindingIndex(ActionBindingInfo actionBindingInfo, out int firstBindingIndex)
        {
            firstBindingIndex = -1;

            InputBinding bindingMask = actionBindingInfo.ControlSchemeId.ToBindingMask();

            for (int i = 0; i < actionBindingInfo.ActionWrapper.InputAction.bindings.Count; i++)
            {
                InputBinding binding = actionBindingInfo.ActionWrapper.InputAction.bindings[i];
                if (bindingMask.Matches(binding) &&
                    ((actionBindingInfo.UseCompositePart && actionBindingInfo.CompositePart.Matches(binding)) ||
                    (actionBindingInfo.DontUseCompositePart && binding is { isComposite: false, isPartOfComposite: false })))
                {
                    firstBindingIndex = i;
                    break;
                }
            }

            return firstBindingIndex != -1;
        }
    }
}
