using NPTP.InputSystemWrapper.InputDevices.Bindings;
using NPTP.InputSystemWrapper.InputDevices.Devices;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace NPTP.InputSystemWrapper.InputDevices
{
    /// <summary>
    /// Register additional bindings and input device layouts that this package supports
    /// for better out-of-the-box UX with 3rd party HID-compliant input devices with arbitrary layouts.
    /// Explicit support is required to improve the UX for each device, as Unity has no existing solution.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    internal static class InputDeviceRegistration
    {
#if UNITY_EDITOR
        static InputDeviceRegistration()
        {
            // PerformRegistrations is called when playing at a specific time, so prevent doing it twice.
            if (Application.isPlaying)
            {
                return;
            }

            PerformRegistrations();
        }
#endif

        /// <summary>
        /// Perform registrations. Must be called after PlayerInputs are initialized.
        /// </summary>
        internal static void PerformRegistrations()
        {
            RegisterSupplementaryBindings();
            RegisterSupplementaryDevices();
        }

        private static void RegisterSupplementaryBindings()
        {
            TwoAxisComposite.Register();
        }
        
        private static void RegisterSupplementaryDevices()
        {
            LogitechDualAction.Register();
            GenericUSBGamepad.Register();
        }
    }
}
