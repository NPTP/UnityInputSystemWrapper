using UnityEngine;
using UnityEngine.InputSystem;

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
        // Support additional device "classes" explicitly as needed (avoids the need to use control schemes)
        [Header("Input Device Binding Data")] [Space]
        [SerializeField] private BindingData mouseKeyboardBindingData;
        public BindingData MouseKeyboardBindingData => mouseKeyboardBindingData;
        [SerializeField] private BindingData xboxBindingData;
        public BindingData XboxBindingData => xboxBindingData;
        [SerializeField] private BindingData playstationBindingData;
        public BindingData PlaystationBindingData => playstationBindingData;
        [SerializeField] private BindingData gamepadFallbackBindingData;
        public BindingData GamepadFallbackBindingData => gamepadFallbackBindingData;
    }
}