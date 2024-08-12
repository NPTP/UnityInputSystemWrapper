using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace InputSystemWrapper.Utilities.Extensions
{
    internal static class AssetReferenceExtensions
    {
        public static T LoadAssetSynchronous<T>(this AssetReference assetReference)
        {
            T asset;
            AsyncOperationHandle<T> handle = assetReference.LoadAssetAsync<T>();
#if UNITY_WEBGL
            // WebGL is single-threaded, and calling WaitForCompletion can lock it, so we use the normal async workflow for this case.
            // See https://docs.unity3d.com/Packages/com.unity.addressables@1.20/manual/SynchronousAddressables.html
            throw new System.Exception("WebGL not supported for synchronous Addressables loading!");
#else
            asset = handle.WaitForCompletion();
            Addressables.Release(handle);
#endif
            return asset;
        }
    }
}