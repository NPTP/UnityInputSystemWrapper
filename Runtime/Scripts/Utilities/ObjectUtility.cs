using System.Collections.Generic;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Utilities
{
    internal static class ObjectUtility
    {
        public static void DestroyAllObjectsOfType<T1>()
            where T1 : Object
        {
            Object.FindObjectsOfType<T1>().DestroyAll();
        }
        
        public static void DestroyAllObjectsOfType<T1, T2>()
            where T1 : Object
            where T2 : Object
        {
            Object.FindObjectsOfType<T1>().DestroyAll();
            Object.FindObjectsOfType<T2>().DestroyAll();
        }
        
        public static void DestroyAllObjectsOfType<T1, T2, T3>()
            where T1 : Object
            where T2 : Object
            where T3 : Object
        {
            Object.FindObjectsOfType<T1>().DestroyAll();
            Object.FindObjectsOfType<T2>().DestroyAll();
            Object.FindObjectsOfType<T3>().DestroyAll();
        }

        public static void DestroyAllObjectsOfType<T1, T2, T3, T4>()
            where T1 : Object
            where T2 : Object
            where T3 : Object
            where T4 : Object
        {
            Object.FindObjectsOfType<T1>().DestroyAll();
            Object.FindObjectsOfType<T2>().DestroyAll();
            Object.FindObjectsOfType<T3>().DestroyAll();
            Object.FindObjectsOfType<T4>().DestroyAll();
        }
        
        private static void DestroyAll<T>(this IEnumerable<T> objects) where T : Object
        {
            foreach (T unityObject in objects)
            {
                Object.Destroy(unityObject);
            }
        }
    }
}