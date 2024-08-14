using NPTP.InputSystemWrapper.AutopopulatedEnums;
using NPTP.InputSystemWrapper.Utilities.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Input Data used at runtime.
    /// </summary>
    public class RuntimeInputData : ScriptableObject
    {
        [SerializeField] private InputActionAsset inputActionAsset;
        public InputActionAsset InputActionAsset => inputActionAsset;

        [SerializeField] private SerializableDictionary<ControlScheme, BindingDataReference> bindingDataReferences;
        public SerializableDictionary<ControlScheme, BindingDataReference> BindingDataReferences => bindingDataReferences;
    }
}