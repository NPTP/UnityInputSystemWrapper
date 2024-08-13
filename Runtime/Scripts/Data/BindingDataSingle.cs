using InputSystemWrapper.Utilities.Collections;
using UnityEngine;

namespace UnityInputSystemWrapper.Data
{
    public abstract class BindingData : ScriptableObject
    {
        public abstract SerializableDictionary<string, BindingPathInfo> BindingDisplayInfo { get; }
    }
    
    /// <summary>
    /// Contains all of the binding data for a particular input method/device: e.g. Mouse+Kbd, Xbox gamepad, etc. 
    /// </summary>
    [CreateAssetMenu]
    public class BindingDataSingle : BindingData
    {
        [SerializeField] private SerializableDictionary<string, BindingPathInfo> bindingDisplayInfo = new();
        public override SerializableDictionary<string, BindingPathInfo> BindingDisplayInfo => bindingDisplayInfo;
    }
}