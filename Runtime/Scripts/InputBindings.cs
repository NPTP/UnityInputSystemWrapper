using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace NPTP.InputSystemWrapper
{
    /// <summary>
    /// Static bindings-related logic is addressed here.
    /// </summary>
    internal static class InputBindings
    {
        internal static bool TryGetActionBindingInfo(RuntimeInputData runtimeInputData, InputAction action, InputDevice device, out IEnumerable<BindingInfo> bindingInfos)
        {
            bindingInfos = default;
            
            if (device == null)
            {
                return false;
            }
            
            // Get the asset on disk containing binding data.
            if (!TryGetBindingData(runtimeInputData, device, out BindingData bindingData))
            {
                return false;
            }

            // Get the string paths to the used input action.
            if (!TryGetControlPaths(action, device, out List<string> controlPaths))
            {
                return false;
            }

            // Get the binding info (name, sprite, etc) for each control path.
            if (!TryGetAllBindingInfo(controlPaths, bindingData, out List<BindingInfo> bindingInfoList))
            {
                return false;
            }

            bindingInfos = bindingInfoList;
            return true;
        }

        private static bool TryGetAllBindingInfo(List<string> controlPaths, BindingData bindingData, out List<BindingInfo> bindingInfoList)
        {
            bindingInfoList = new List<BindingInfo>();
            foreach (string controlPath in controlPaths)
            {
                if (bindingData.TryGetBindingInfo(controlPath, out BindingInfo bindingInfo))
                    bindingInfoList.Add(bindingInfo);
            }

            return bindingInfoList.Count > 0;
        }

        private static bool TryGetBindingData<TDevice>(RuntimeInputData runtimeInputData, TDevice device, out BindingData bindingData)
            where TDevice : InputDevice
        {
            bindingData = device switch
            {
                // Support additional device "classes" explicitly as needed (avoids the need to use control schemes)
                Mouse or Keyboard => runtimeInputData.MouseKeyboardBindingData,
                XInputController => runtimeInputData.XboxBindingData,
                DualShockGamepad => runtimeInputData.PlaystationBindingData,
                Gamepad => runtimeInputData.GamepadFallbackBindingData,
                _ => runtimeInputData.GamepadFallbackBindingData
            };

            bool bindingDataNull = bindingData == null;
            if (bindingDataNull)
                Debug.LogWarning($"Input device {typeof(TDevice).Name} is not supported by any {nameof(BindingData)} and cannot produce display names/sprites!");
            
            return !bindingDataNull;
        }
        
        private static bool TryGetControlPaths(InputAction action, InputDevice device, out List<string> controlPaths)
        {
            List<string> paths = new();

            if (device is Mouse or Keyboard)
            {
                addPathsForDevice(Mouse.current);
                addPathsForDevice(Keyboard.current);
            }
            else
            {
                addPathsForDevice(device);
            }

            controlPaths = paths;
            return controlPaths.Count > 0;
            
            void addPathsForDevice(InputDevice inputDevice)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    InputBinding binding = action.bindings[i];
                    InputControl control = InputControlPath.TryFindControl(inputDevice, binding.effectivePath);
                    if (control != null && control.device == inputDevice)
                    {
                        paths.Add(control.path[(2 + control.device.name.Length)..]);
                    }
                }
            }
        }
    }
}
