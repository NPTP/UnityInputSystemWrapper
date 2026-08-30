using System;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// The runtime description of one input context: which action maps it enables, whether it allows
    /// keyboard text input, and which event system actions it overrides. Baked from the offline input
    /// data by the input script generator, in the same order as the generated InputContext enum.
    /// </summary>
    [Serializable]
    internal class InputContextDefinition
    {
        [SerializeField] private string name;
        internal string Name => name;

        [SerializeField] private bool enableKeyboardTextInput;
        internal bool EnableKeyboardTextInput => enableKeyboardTextInput;

        [SerializeField] private string[] activeMapNames;
        internal string[] ActiveMapNames => activeMapNames;

        [SerializeField] private EventSystemActionBinding[] eventSystemActionOverrides;
        internal EventSystemActionBinding[] EventSystemActionOverrides => eventSystemActionOverrides;

#if UNITY_EDITOR
        internal const string EDITOR_NameField = nameof(name);
        internal const string EDITOR_EnableKeyboardTextInputField = nameof(enableKeyboardTextInput);
        internal const string EDITOR_ActiveMapNamesField = nameof(activeMapNames);
        internal const string EDITOR_EventSystemActionOverridesField = nameof(eventSystemActionOverrides);
#endif
    }
}
