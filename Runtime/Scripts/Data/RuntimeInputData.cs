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

        [SerializeField] private SerializableDictionary<ControlScheme, BindingDataAssetReference> bindingDataReferences;
        public SerializableDictionary<ControlScheme, BindingDataAssetReference> BindingDataReferences => bindingDataReferences;
        
        [Header("Event System Actions")]
        [SerializeField] private InputActionReference point;
        [SerializeField] private InputActionReference middleClick;
        [SerializeField] private InputActionReference rightClick;
        [SerializeField] private InputActionReference scrollWheel;
        [SerializeField] private InputActionReference move;
        [SerializeField] private InputActionReference submit;
        [SerializeField] private InputActionReference cancel;
        [SerializeField] private InputActionReference trackedDevicePosition;
        [SerializeField] private InputActionReference trackedDeviceOrientation;
        [SerializeField] private InputActionReference leftClick;
        public InputActionReference Point => point;
        public InputActionReference LeftClick => leftClick;
        public InputActionReference MiddleClick => middleClick;
        public InputActionReference RightClick => rightClick;
        public InputActionReference ScrollWheel => scrollWheel;
        public InputActionReference Move => move;
        public InputActionReference Submit => submit;
        public InputActionReference Cancel => cancel;
        public InputActionReference TrackedDevicePosition => trackedDevicePosition;
        public InputActionReference TrackedDeviceOrientation => trackedDeviceOrientation;
    }
}