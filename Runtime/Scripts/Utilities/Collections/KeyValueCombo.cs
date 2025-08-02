using System;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Utilities.Collections
{
    /// <summary>
    /// Serializable version of a Key Value Pair struct.
    /// </summary>
    [Serializable]
    internal struct KeyValueCombo<TKey, TValue>
    {
        [SerializeField] private TKey key;
        internal TKey Key => key;
        
        [SerializeField] private TValue value;
        internal TValue Value => value;
        
        internal KeyValueCombo(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
}