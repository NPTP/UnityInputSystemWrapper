using System;
using UnityEngine;
using UnityInputSystemWrapper.Attributes;

namespace UnityInputSystemWrapper.Data
{
    [Serializable]
    public class InputContextInfo
    {
        [SerializeField] private string name;
        public string Name => name;

        [InputMapSelector]
        [SerializeField] private string[] activeMaps;
        public string[] ActiveMaps => activeMaps;

        [SerializeField] private bool enableKeyboardTextInput;
        public bool EnableKeyboardTextInput => enableKeyboardTextInput;

        [SerializeField] private EventSystemActions eventSystemActions;
        public EventSystemActions EventSystemActions => eventSystemActions;
        
#if UNITY_EDITOR
        public void EDITOR_SetName(string n)
        {
            name = n;
        }
#endif
    }
}