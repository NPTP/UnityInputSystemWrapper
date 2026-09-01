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
        /// Add a control path with its starting localization key and display name. An existing entry keeps
        /// everything already authored on it, and only has blank fields filled in.
        /// </summary>
        internal void EDITOR_AddBinding(string controlPath, string localizationKey, string defaultDisplayName)
        {
            if (string.IsNullOrEmpty(controlPath))
            {
                return;
            }

            if (!EDITOR_Contains(controlPath))
            {
                bindingDataDictionary.EDITOR_Add(controlPath, new BindingInfo(localizationKey, defaultDisplayName));
                return;
            }

            // An entry from before these fields existed has them filled in, without touching an entry
            // someone has already authored.
            if (bindingDataDictionary.EDITOR_TryGetValue(controlPath, out BindingInfo existing) && existing.EDITOR_HasBlanks)
            {
                bindingDataDictionary.EDITOR_SetValue(controlPath, existing.EDITOR_WithBlanksFilled(localizationKey, defaultDisplayName));
            }
        }
#endif
    }
}
