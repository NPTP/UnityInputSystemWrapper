using System;

namespace NPTP.InputSystemWrapper.Utilities.Extensions
{
    internal static class ArrayExtensions
    {
        public static void ForEach<T>(this T[] array, Action<T> action)
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
    }
}