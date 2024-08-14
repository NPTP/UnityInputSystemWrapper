using System;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Tells us which strings and icons to display for a binding.
    /// </summary>
    [Serializable]
    public class BindingPathInfo
    {
        public string RuntimeDisplayName => overrideDisplayName ? overrideName : inputControlDisplayName;

        [SerializeField] private bool overrideDisplayName;
        
        /// NOTE: This should be a localized string, using whatever localization system is in the project.
        [SerializeField] private string overrideName;

        [SerializeField] private Sprite icon;
        public Sprite Icon => icon;

        private string inputControlDisplayName = string.Empty;
        
        public void SetInputControlDisplayName(string inputControlName)
        {
            inputControlDisplayName = inputControlName;
        }
    }
}