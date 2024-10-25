using System;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// Tells us which strings and icons to display for a single binding.
    /// E.g. Given the binding "dpad/up", this might show a sprite with a
    /// D-Pad pointing up and use the display name "D-Pad Up".
    /// </summary>
    [Serializable]
    public class BindingInfo
    {
        // TODO (localization): This should be a localized string, using whatever localization system is in the project.
        [SerializeField] private string displayName;
        public string DisplayName => displayName;

        [SerializeField] private Sprite sprite;
        public Sprite Sprite => sprite;
    }
}