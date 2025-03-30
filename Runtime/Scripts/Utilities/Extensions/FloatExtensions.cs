namespace NPTP.InputSystemWrapper.Utilities.Extensions
{
    public static class FloatExtensions
    {
        public static bool BetweenInclusive(this float value, float min, float max)
        {
            return min <= value && value <= max;
        }
        
        public static bool BetweenLowerInclusive(this float value, float min, float max)
        {
            return min <= value && value < max;
        }
    }
}