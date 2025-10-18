
namespace NPTP.InputSystemWrapper.Utilities.Extensions
{
    internal static class ArrayExtensions
    {
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

        internal static bool IndexIsValid<T>(this T[] array, int index)
        {
            return array != null && 0 <= index && index < array.Length;
        }
    }
}