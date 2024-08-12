using System.Collections.Generic;
using UnityEngine;

namespace InputSystemWrapper.Utilities.Extensions
{
    public static class ObjectExtensions
    {
        public static void DestroyAll<T>(this IEnumerable<T> objects) where T : Object
        {
            foreach (T unityObject in objects)
            {
                Object.Destroy(unityObject);
            }
        }
    }
}