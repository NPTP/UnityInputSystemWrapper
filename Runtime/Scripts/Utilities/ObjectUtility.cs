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
            Object.FindObjectsOfType<T1>().Destroy();
            Object.FindObjectsOfType<T2>().Destroy();
            Object.FindObjectsOfType<T3>().Destroy();
            Object.FindObjectsOfType<T4>().Destroy();
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