using System.Collections.Generic;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Utilities
{
    internal static class ObjectUtility
    {
        internal static void DestroyObjectsOfType<T1, T2, T3, T4>()
            where T1 : Object
            where T2 : Object
            where T3 : Object
            where T4 : Object
        {
#if UNITY_6000_5_OR_NEWER
            Object.FindObjectsByType<T1>(FindObjectsInactive.Include).Destroy();
            Object.FindObjectsByType<T2>(FindObjectsInactive.Include).Destroy();
            Object.FindObjectsByType<T3>(FindObjectsInactive.Include).Destroy();
            Object.FindObjectsByType<T4>(FindObjectsInactive.Include).Destroy();
#elif UNITY_2022_2_OR_NEWER
            Object.FindObjectsByType<T1>(FindObjectsInactive.Include, FindObjectsSortMode.None).Destroy();
            Object.FindObjectsByType<T2>(FindObjectsInactive.Include, FindObjectsSortMode.None).Destroy();
            Object.FindObjectsByType<T3>(FindObjectsInactive.Include, FindObjectsSortMode.None).Destroy();
            Object.FindObjectsByType<T4>(FindObjectsInactive.Include, FindObjectsSortMode.None).Destroy();
#else
            Object.FindObjectsOfType<T1>().Destroy();
            Object.FindObjectsOfType<T2>().Destroy();
            Object.FindObjectsOfType<T3>().Destroy();
            Object.FindObjectsOfType<T4>().Destroy();
#endif
        }
        
        private static void Destroy<T>(this IEnumerable<T> objects) where T : Object
        {
            foreach (T unityObject in objects)
            {
                Object.Destroy(unityObject);
            }
        }
    }
}