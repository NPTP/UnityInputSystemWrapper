using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Input Data used at runtime, containing the input action asset template on which new assets are cloned,
    /// and the data that lets us resolve input bindings to display names & sprites on the UI.
    /// </summary>
    public class RuntimeInputData : ScriptableObject
    {
        [SerializeField] private InputActionAsset inputActionAsset;
        public InputActionAsset InputActionAsset => inputActionAsset;

        // TODO: Some kind of reference to load BindingData in/out, instead of just referencing them directly
        // TODO: Support additional device "classes" explicitly, as needed. Avoids the need to use control schemes for binding info
        [Header("Input Device Binding Data")] [Space]
        [SerializeField] private BindingData mouseKeyboardBindingData;
        [SerializeField] private BindingData xboxBindingData;
        [SerializeField] private BindingData playstationBindingData;
        [SerializeField] private BindingData gamepadFallbackBindingData;

        public bool TryGetBindingData<TDevice>(TDevice device, out BindingData bindingData) where TDevice : InputDevice
        {
            bindingData = device switch
            {
                Mouse or Keyboard => mouseKeyboardBindingData,
                XInputController => xboxBindingData,
                DualShockGamepad => playstationBindingData,
                Gamepad => gamepadFallbackBindingData,
                _ => null
            };

            bool bindingDataNull = bindingData == null;
            if (bindingDataNull)
                Debug.LogWarning($"Input device {typeof(TDevice).Name} is not supported by any {nameof(BindingData)} and cannot produce display names/sprites!");
            
            return !bindingDataNull;
        }
    }
}