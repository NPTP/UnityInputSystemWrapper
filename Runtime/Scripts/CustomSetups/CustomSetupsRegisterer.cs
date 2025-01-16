using NPTP.InputSystemWrapper.CustomSetups.Bindings;
using NPTP.InputSystemWrapper.CustomSetups.Devices;
using NPTP.InputSystemWrapper.InputDevices.Interactions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace NPTP.InputSystemWrapper.CustomSetups
{
    /// <summary>
    /// Register additional bindings and input device layouts that this package supports
    /// for better out-of-the-box UX with 3rd party HID-compliant input devices with arbitrary layouts.
    /// Explicit support is required to improve the UX for each device, as Unity has no existing solution.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    internal static class CustomSetupsRegisterer
    {
#if UNITY_EDITOR
        static CustomSetupsRegisterer()
        {
            // PerformRegistrations is called when playing at a specific time, so prevent doing it twice.
            if (Application.isPlaying)
            {
                return;
            }

            PerformRegistrations();
        }
#endif
        
        internal static void PerformRegistrations()
        {
            RegisterCustomInteractions();
            RegisterCustomBindings();
            RegisterCustomDevices();
        }

        private static void RegisterCustomInteractions()
        {
            FixedHoldInteraction.Register();
        }

        private static void RegisterCustomBindings()
        {
            TwoAxisComposite.Register();
        }
        
        private static void RegisterCustomDevices()
        {
            LogitechDualAction.Register();
            GenericUSBGamepad.Register();
        }
    }
}
