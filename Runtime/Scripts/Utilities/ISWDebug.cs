namespace NPTP.InputSystemWrapper.Utilities
{
    internal static class ISWDebug
    {
        private const string DEBUG_PREFIX = "[InputSystemWrapper]: ";
        internal static void Log(string log) => UnityEngine.Debug.Log(DEBUG_PREFIX + log);
        internal static void LogWarning(string log) => UnityEngine.Debug.LogWarning(DEBUG_PREFIX + log);
        internal static void LogError(string log) => UnityEngine.Debug.LogError(DEBUG_PREFIX + log);
    }
}
