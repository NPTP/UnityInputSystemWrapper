using System;
using UnityEngine;

namespace UnityInputSystemWrapper.Data
{
    [Serializable]
    public class BindingDataReference
    {
        [SerializeField] private string key;
        public string Key => key;
        [SerializeField] private string name;
        public string Name => name;

        private BindingData loadedReference;
        
        public BindingData Load()
        {
            if (loadedReference != null)
            {
                return loadedReference;
            }

            loadedReference = Resources.Load<BindingData>(key);
            return loadedReference;
        }

        public void Unload()
        {
            if (loadedReference == null)
            {
                return;
            }
            
            BindingData referenceHandle = loadedReference;
            loadedReference = null;
            Resources.UnloadAsset(referenceHandle);
        }
    }
}