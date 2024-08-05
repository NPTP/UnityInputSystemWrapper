using System;
using UnityEngine.AddressableAssets;

namespace UnityInputSystemWrapper.Data
{
    [Serializable]
    public class BindingDataAssetReference : AssetReferenceT<BindingData>
    {
        protected BindingDataAssetReference(string guid) : base(guid)
        {
        }
    }
}