using NPTP.InputSystemWrapper.Utilities.Collections;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// Contains the binding data for 1 particular device.
    /// The dictionary takes an input control path/binding and returns a display name/sprite for that binding.
    /// </summary>
    [CreateAssetMenu(menuName = "InputSystemWrapper/BindingData")]
    public class BindingData : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<string, BindingInfo> bindingDataDictionary = new();

        public bool TryGetBindingInfo(string controlPath, out BindingInfo bindingInfo) => bindingDataDictionary.TryGetValue(controlPath, out bindingInfo);
        
#if UNITY_EDITOR
        public void EDITOR_AddMouseBindings() => EDITOR_AddBindings(BindingPathHelper.MouseControls);
        public void EDITOR_AddKeyboardBindings() => EDITOR_AddBindings(BindingPathHelper.KeyboardControls);
        public void EDITOR_AddGamepadBindings() => EDITOR_AddBindings(BindingPathHelper.GamepadControls);
        
        private void EDITOR_AddBindings(string[] bindingNames)
        {
            foreach (string controlPath in bindingNames)
            {
                bindingDataDictionary.EDITOR_Add(controlPath, new BindingInfo());
            }
        }
#endif
    }
}