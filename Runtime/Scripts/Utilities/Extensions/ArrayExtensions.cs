
namespace NPTP.InputSystemWrapper.Utilities.Extensions
{
    internal static class ArrayExtensions
    {
        internal static bool IndexIsValid<T>(this T[] array, int index)
        {
            return array != null && 0 <= index && index < array.Length;
        }
    }
}