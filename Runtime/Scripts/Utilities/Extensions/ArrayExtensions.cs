using System;

namespace NPTP.InputSystemWrapper.Utilities.Extensions
{
    internal static class ArrayExtensions
    {
        internal static void ForEach<T>(this T[] array, Action<T> action)
        {
            if (array == null || action == null)
            {
                return;
            }

            for (int i = 0; i < array.Length; i++)
            {
                action.Invoke(array[i]);
            }
        }

        internal static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        internal static void DefaultAll<T>(this T[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = default;
            }
        }
    }
}