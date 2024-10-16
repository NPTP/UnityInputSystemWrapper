#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NPTP.InputSystemWrapper.Utilities
{
    /// <summary>
    /// Contains methods with editor-only functionality that are safe to call at runtime.
    /// </summary>
    internal static class RuntimeSafeEditorUtility
    {
        internal static bool IsDomainReloadDisabled()
        {
#if UNITY_EDITOR
            return EditorSettings.enterPlayModeOptionsEnabled &&
                   EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload);
#endif

#pragma warning disable CS0162
            // Domain is always reloaded in a build.
            return false;
#pragma warning restore CS0162
        }
    }
}