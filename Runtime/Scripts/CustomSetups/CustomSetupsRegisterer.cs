using NPTP.InputSystemWrapper.CustomSetups.Bindings;
using NPTP.InputSystemWrapper.CustomSetups.Devices;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace NPTP.InputSystemWrapper.CustomSetups
{
    /// <summary>
    /// Register additional interactions, bindings, and input device layouts with the ability for the developer
    /// to define more and hook them in here via the partial methods.
    /// </summary>
    // TODO: Create a custom setups overrideable class that gets called from here externally to this assembly, since partial methods would have to be in the same assembly, and this limits their usefulness.
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static partial class CustomSetupsRegisterer
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
            RegisterOptionalCustomInteractions();
        }

        static partial void RegisterOptionalCustomInteractions();

        private static void RegisterCustomBindings()
        {
            TwoAxisComposite.Register();
            RegisterOptionalCustomBindings();
        }
        
        static partial void RegisterOptionalCustomBindings();

        private static void RegisterCustomDevices()
        {
            LogitechDualAction.Register();
            GenericUSBGamepad.Register();
            RegisterOptionalCustomDevices();
        }
        
        static partial void RegisterOptionalCustomDevices();
    }
}
