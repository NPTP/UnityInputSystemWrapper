using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Utilities.Collections
{
    /// <summary>
    /// Based on one of Unity's serializable dictionaries for a customizable/maintainable version.
    /// </summary>
    [Serializable]
    public sealed class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<KeyValueCombo<TKey, TValue>> keyValueCombos = new();

        private Dictionary<TKey, TValue> internalDictionary = new();

        public ICollection<TKey> Keys => internalDictionary.Keys;
        public ICollection<TValue> Values => internalDictionary.Values;
        public int Count => internalDictionary.Count;
        
        public TValue this[TKey key]
        {
            get => internalDictionary[key];
            set => internalDictionary[key] = value;
        }

        public TKey this[TValue value]
        {
            get
            {
                List<TKey> keys = new(internalDictionary.Keys);
                List<TValue> values = new(internalDictionary.Values);
                int index = values.FindIndex(x => x.Equals(value));
                if (index < 0)
                {
                    throw new KeyNotFoundException();
                }
                return keys[index];
            }
        }
        
        public void AddRange(IDictionary<TKey, TValue> items)
        {
            foreach (TKey key in items.Keys)
            {
                internalDictionary.TryAdd(key, items[key]);
            }
        }

        public void Add(TKey key, TValue value) => internalDictionary.Add(key, value);
        public bool TryAdd(TKey key, TValue value) => internalDictionary.TryAdd(key, value);
        public bool ContainsKey(TKey key) => internalDictionary.ContainsKey(key);
        public bool Remove(TKey key) => internalDictionary.Remove(key);
        public bool TryGetValue(TKey key, out TValue value) => internalDictionary.TryGetValue(key, out value);
        public void Clear() => internalDictionary.Clear();
        public IEnumerator GetEnumerator() => internalDictionary.GetEnumerator();

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            Clear();
            foreach (KeyValueCombo<TKey, TValue> keyValuePair in keyValueCombos)
            {
                TryAdd(keyValuePair.Key, keyValuePair.Value);
            }
        }
        
#if UNITY_EDITOR
        public void EDITOR_SetKey(TValue value, TKey newKey)
        {
            for (int i = 0; i < keyValueCombos.Count; i++)
            {
                KeyValueCombo<TKey, TValue> keyValueCombo = keyValueCombos[i];
                if (keyValueCombo.Value.Equals(value))
                {
                    keyValueCombos[i] = new KeyValueCombo<TKey, TValue>(newKey, value);
                    break;
                }
            }
        }

        public void EDITOR_Clear()
        {
            keyValueCombos.Clear();
        }

        public void EDITOR_Add(TKey key, TValue value)
        {
            foreach (KeyValueCombo<TKey,TValue> keyValueCombo in keyValueCombos)
            {
                if (EqualityComparer<TKey>.Default.Equals(keyValueCombo.Key, key))
                {
                    NPTPDebug.LogError($"Couldn't add {key} because value already exists in dictionary");
                    return;
                }
            }
            
            keyValueCombos.Add(new KeyValueCombo<TKey, TValue>(key, value));
        }

        public void EDITOR_Remove(TKey key)
        {
            int remove = -1;
            for (int i = 0; i < keyValueCombos.Count; i++)
            {
                KeyValueCombo<TKey, TValue> combo = keyValueCombos[i];
                if (combo.Key.Equals(key))
                {
                    remove = i;
                    break;
                }
            }

            if (remove != -1)
            {
                keyValueCombos.RemoveAt(remove);
            }
        }

        public void EDITOR_Remove(int index)
        {
            keyValueCombos.RemoveAt(Mathf.Clamp(index, 0, keyValueCombos.Count - 1));
        }

        public List<KeyValueCombo<TKey, TValue>> EDITOR_GetKeyValueCombos()
        {
            return keyValueCombos;
        }
#endif

        #region Explicit IDictionary Implementations
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => (internalDictionary as ICollection<KeyValuePair<TKey, TValue>>).IsReadOnly;
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => (internalDictionary as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => (internalDictionary as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => (internalDictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => (internalDictionary as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => (internalDictionary as IEnumerable<KeyValuePair<TKey, TValue>>).GetEnumerator();
        #endregion
    }
}