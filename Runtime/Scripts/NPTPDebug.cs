namespace NPTP.InputSystemWrapper
{
    public static class NPTPDebug
    {
        private const string DEBUG_PREFIX = "[InputSystemWrapper]: ";
        public static void Log(string log) => UnityEngine.Debug.Log(DEBUG_PREFIX + log);
        public static void LogWarning(string log) => UnityEngine.Debug.LogWarning(DEBUG_PREFIX + log);
        public static void LogError(string log) => UnityEngine.Debug.LogError(DEBUG_PREFIX + log);
    }
}
