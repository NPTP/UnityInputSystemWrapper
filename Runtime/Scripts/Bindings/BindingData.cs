using System.Collections.Generic;
using NPTP.InputSystemWrapper.Utilities.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// Contains the binding data for 1 particular device.
    /// The dictionary takes an input control path/binding and returns the entry describing that binding.
    /// <para>
    /// Entries are referenced rather than held, so this asset costs only its own keys until something
    /// asks for a particular binding.
    /// </para>
    /// </summary>
    [CreateAssetMenu(menuName = "InputSystemWrapper/BindingData")]
    internal class BindingData : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<string, AssetReference> bindingDataDictionary = new();

        /// <summary>The reference to one control's entry, which the caller loads and later releases.</summary>
        internal bool TryGetBindingReference(string controlPath, out AssetReference reference) =>
            bindingDataDictionary.TryGetValue(controlPath, out reference);

#if UNITY_EDITOR
        internal const string EDITOR_DictionaryField = nameof(bindingDataDictionary);

        internal bool EDITOR_Contains(string controlPath) => bindingDataDictionary.EDITOR_ContainsKey(controlPath);

        /// <summary>Point a control path at its entry asset, replacing whatever it pointed at before.</summary>
        internal void EDITOR_SetBinding(string controlPath, string assetGuid)
        {
            if (string.IsNullOrEmpty(controlPath))
            {
                return;
            }

            AssetReference reference = new(assetGuid);
            if (EDITOR_Contains(controlPath))
            {
                bindingDataDictionary.EDITOR_SetValue(controlPath, reference);
                return;
            }

            bindingDataDictionary.EDITOR_Add(controlPath, reference);
        }

        /// <summary>The control paths this asset knows about, so stale entries can be found.</summary>
        internal IEnumerable<string> EDITOR_ControlPaths
        {
            get
            {
                foreach (KeyValueCombo<string, AssetReference> combo in bindingDataDictionary.EDITOR_GetKeyValueCombos())
                {
                    yield return combo.Key;
                }
            }
        }

        internal void EDITOR_RemoveBinding(string controlPath) => bindingDataDictionary.EDITOR_Remove(controlPath);
#endif
    }
}
