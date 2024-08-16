using System;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Tells us which strings and icons to display for a single binding.
    /// E.g. "dpad/up" which should show a D-Pad pointing up and display "D-Pad Up".
    /// </summary>
    [Serializable]
    public class BindingInfo
    {
        /// NOTE: This should be a localized string, using whatever localization system is in the project.
        [SerializeField] private string displayName;
        public string DisplayName => displayName;

        [SerializeField] private Sprite sprite;
        public Sprite Sprite => sprite;
    }
}