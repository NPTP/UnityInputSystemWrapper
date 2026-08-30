using NPTP.InputSystemWrapper.Utilities.Collections;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// Contains the binding data for 1 particular device.
    /// The dictionary takes an input control path/binding and returns a display name/sprite for that binding.
    /// </summary>
    [CreateAssetMenu(menuName = "InputSystemWrapper/BindingData")]
    internal class BindingData : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<string, BindingInfo> bindingDataDictionary = new();

        public bool TryGetBindingInfo(string controlPath, out BindingInfo bindingInfo) => bindingDataDictionary.TryGetValue(controlPath, out bindingInfo);

#if UNITY_EDITOR
        internal const string EDITOR_DictionaryField = nameof(bindingDataDictionary);

        internal bool EDITOR_Contains(string controlPath) => bindingDataDictionary.EDITOR_ContainsKey(controlPath);

        /// <summary>
        /// Add a control path with a starting localization key. Existing entries are left alone, so
        /// anything already filled in - a sprite, an edited key - survives being repopulated.
        /// </summary>
        internal void EDITOR_AddBinding(string controlPath, string localizationKey)
        {
            if (string.IsNullOrEmpty(controlPath) || EDITOR_Contains(controlPath))
            {
                return;
            }

            bindingDataDictionary.EDITOR_Add(controlPath, new BindingInfo(localizationKey));
        }
#endif
    }
}
