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
        internal static bool TryGetActionBindingInfo(RuntimeInputData runtimeInputData, InputAction action, InputDevice device, out BindingInfo bindingInfo)
        {
            bindingInfo = default;
            
            if (device == null)
            {
                return false;
            }
            
            if (!TryGetBindingData(runtimeInputData, device, out BindingData bindingData))
            {
                return false;
            }

            // TODO: Support returning multiple control paths, since an action may have multiple bindings on a single device
            if (!TryGetControlPath(action, device, out string controlPath))
            {
                return false;
            }
            
            return bindingData.TryGetBindingInfo(controlPath, out bindingInfo);
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
        
        private static bool TryGetControlPath(InputAction action, InputDevice device, out string controlPath)
        {
            controlPath = default;
            
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                InputControl control = InputControlPath.TryFindControl(device, binding.effectivePath);
                if (control != null && control.device == device)
                {
                    controlPath = control.path[(2 + control.device.name.Length)..];
                    return true;
                }
            }

            return false;
        }
    }
}
