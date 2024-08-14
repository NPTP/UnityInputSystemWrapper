using System;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Utilities.Collections
{
    /// <summary>
    /// Serializable version of a Key Value Pair struct.
    /// </summary>
    [Serializable]
    public struct KeyValueCombo<TKey, TValue>
    {
        [SerializeField] private TKey key;
        public TKey Key => key;
        
        [SerializeField] private TValue value;
        public TValue Value => value;
        
        public KeyValueCombo(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
}