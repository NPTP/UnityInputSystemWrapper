using System;
using NPTP.InputSystemWrapper.Attributes;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Data
{
    [Serializable]
    internal class InputContextInfo
    {
        [SerializeField] private string name;
        internal string Name => name;
        
        [SerializeField] private bool enableKeyboardTextInput;
        internal bool EnableKeyboardTextInput => enableKeyboardTextInput;

        [InputMapSelector][SerializeField] private string[] activeMaps;
        internal string[] ActiveMaps => activeMaps;
        
        [SerializeField] private EventSystemActionSpecification[] eventSystemActionOverrides;
        internal EventSystemActionSpecification[] EventSystemActionOverrides => eventSystemActionOverrides;

#if UNITY_EDITOR
        internal void EDITOR_SetName(string n)
        {
            name = n;
        }
#endif
    }
}