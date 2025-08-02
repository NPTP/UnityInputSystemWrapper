namespace NPTP.InputSystemWrapper.Utilities.Extensions
{
    internal static class FloatExtensions
    {
        internal static bool BetweenInclusive(this float value, float min, float max)
        {
            return min <= value && value <= max;
        }
        
        internal static bool BetweenLowerInclusive(this float value, float min, float max)
        {
            return min <= value && value < max;
        }
    }
}