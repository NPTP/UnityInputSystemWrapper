using System;
using NPTP.InputSystemWrapper.Attributes;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Data
{
    [Serializable]
    public class InputContextInfo
    {
        [SerializeField] private string name;
        public string Name => name;
        
        [SerializeField] private bool enableKeyboardTextInput;
        public bool EnableKeyboardTextInput => enableKeyboardTextInput;

        [InputMapSelector][SerializeField] private string[] activeMaps;
        public string[] ActiveMaps => activeMaps;
        
        [SerializeField] private EventSystemActionSpecification[] eventSystemActionOverrides;
        public EventSystemActionSpecification[] EventSystemActionOverrides => eventSystemActionOverrides;

#if UNITY_EDITOR
        public void EDITOR_SetName(string n)
        {
            name = n;
        }
#endif
    }
}