using InputSystemWrapper.Utilities.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityInputSystemWrapper.Data
{
    /// <summary>
    /// Input Data used at runtime.
    /// </summary>
    public class RuntimeInputData : ScriptableObject
    {
        [SerializeField] private InputActionAsset inputActionAsset;
        public InputActionAsset InputActionAsset => inputActionAsset;

        [SerializeField] private bool useContextEventSystemActions;
        public bool UseContextEventSystemActions => useContextEventSystemActions;

        [SerializeField] private SerializableDictionary<ControlScheme, BindingDataAssetReference> bindingDataReferences;
        public SerializableDictionary<ControlScheme, BindingDataAssetReference> BindingDataReferences => bindingDataReferences;
    }
}