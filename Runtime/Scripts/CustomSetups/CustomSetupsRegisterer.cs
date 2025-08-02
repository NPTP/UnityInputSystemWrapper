using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities.Extensions;

#if UNITY_EDITOR
using NPTP.InputSystemWrapper.Utilities;
using UnityEditor;
using UnityEngine;
#endif

namespace NPTP.InputSystemWrapper.CustomSetups
{
    /// <summary>
    /// Register additional interactions, bindings, and input device layouts with the ability for the developer
    /// to define more and hook them in via new CustomSetup assets added to RuntimeInputData.
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

            if (RuntimeSafeEditorUtility.TryLoadViaAssetDatabase(out RuntimeInputData runtimeInputData))
                PerformRegistrations(runtimeInputData);
        }
#endif

        internal static void PerformRegistrations(RuntimeInputData runtimeInputData)
        {
            runtimeInputData.CustomLayouts.ForEach(layout => layout.Register());
            runtimeInputData.CustomBindings.ForEach(binding => binding.Register());
            runtimeInputData.CustomInteractions.ForEach(interaction => interaction.Register());
        }
    }
}
