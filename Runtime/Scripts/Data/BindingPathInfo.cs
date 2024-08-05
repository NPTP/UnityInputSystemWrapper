using System;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityInputSystemWrapper.Data
{
    /// <summary>
    /// Tells us which strings and icons to display for a binding.
    /// </summary>
    [Serializable]
    public class BindingPathInfo
    {
        public string RuntimeDisplayName => overrideDisplayName ? overrideName.ToString() : inputControlDisplayName;

        [SerializeField] private bool overrideDisplayName;
        [SerializeField] private LocalizedString overrideName;

        [SerializeField] private Sprite icon;
        public Sprite Icon => icon;

        private string inputControlDisplayName = string.Empty;
        
        public void SetInputControlDisplayName(string inputControlName)
        {
            inputControlDisplayName = inputControlName;
        }
    }
}