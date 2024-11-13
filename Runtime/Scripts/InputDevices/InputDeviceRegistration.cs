#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NPTP.InputSystemWrapper.InputDevices
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    internal static class InputDeviceRegistration
    {
        static InputDeviceRegistration()
        {
            RegisterSupplementaryDevices();
        }
        
        internal static void RegisterSupplementaryDevices()
        {
            LogitechDualAction.RegisterLayout();
            GenericUSBGamepad.RegisterLayout();
        }
    }
}
