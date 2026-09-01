using System.Collections.Concurrent;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// Loads addressable binding assets on demand and keeps one handle per asset however many callers
    /// want it. An asset stays in memory until every caller that took it has given it back.
    /// </summary>
    internal static class BindingDataCache
    {
        private class Entry
        {
            internal AsyncOperationHandle Handle;
            internal Object Asset;
            internal int ReferenceCount;
        }

        private static readonly Dictionary<object, Entry> entriesByKey = new();

        /// <summary>
        /// Releases queued from a finalizer, which does not run on the main thread and so cannot touch
        /// Addressables. They are drained the next time the cache is used from the main thread.
        /// </summary>
        private static readonly ConcurrentQueue<object> pendingReleases = new();

        /// <summary>
        /// The asset for a reference, loaded synchronously if it is not already in memory. Null when the
        /// reference names nothing loadable, so a missing asset costs display names rather than throwing.
        /// </summary>
        internal static T Acquire<T>(AssetReference reference) where T : Object
        {
            DrainPendingReleases();

            if (reference == null || !reference.RuntimeKeyIsValid())
            {
                return null;
            }

            object key = reference.RuntimeKey;
            if (entriesByKey.TryGetValue(key, out Entry existing))
            {
                existing.ReferenceCount++;
                return existing.Asset as T;
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(reference.RuntimeKey);
            handle.WaitForCompletion();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                ISWDebug.LogWarning($"Binding asset {reference.RuntimeKey} could not be loaded. " +
                                    "Check that it is still marked addressable.");
                Addressables.Release(handle);
                return null;
            }

            entriesByKey.Add(key, new Entry { Handle = handle, Asset = handle.Result, ReferenceCount = 1 });
            return handle.Result;
        }

        /// <summary>Gives an asset back. The last caller to do so unloads it.</summary>
        internal static void Release(AssetReference reference)
        {
            DrainPendingReleases();

            if (reference != null && reference.RuntimeKeyIsValid())
            {
                ReleaseKey(reference.RuntimeKey);
            }
        }

        /// <summary>Queues a release for the main thread, for a caller collected without releasing.</summary>
        internal static void ReleaseLater(AssetReference reference)
        {
            if (reference != null && reference.RuntimeKeyIsValid())
            {
                pendingReleases.Enqueue(reference.RuntimeKey);
            }
        }

        private static void DrainPendingReleases()
        {
            while (pendingReleases.TryDequeue(out object key))
            {
                ReleaseKey(key);
            }
        }

        private static void ReleaseKey(object key)
        {
            if (!entriesByKey.TryGetValue(key, out Entry entry))
            {
                return;
            }

            entry.ReferenceCount--;
            if (entry.ReferenceCount > 0)
            {
                return;
            }

            entriesByKey.Remove(key);
            Addressables.Release(entry.Handle);
        }
    }
}
