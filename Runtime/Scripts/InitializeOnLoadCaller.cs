using NPTP.InputSystemWrapper.InputDevices;

#if UNITY_EDITOR
using UnityEditor;
#else
using UnityEngine;
#endif

namespace NPTP.InputSystemWrapper
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class InitializeOnLoadCaller
    {
        static InitializeOnLoadCaller()
        {
            GenericUSBGamepad.Init();
            LogitechDualAction.Init();
        }
        
        // Forces the static constructor to run in a build.
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RuntimeInitializeOnLoadCaller() { }
#endif
    }
}
