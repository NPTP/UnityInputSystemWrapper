using System;
using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("displayName")] [SerializeField]
        private string localizationKey;
        
        /// <summary>
        /// If no localization is hooked into Input.OnLocalizedStringRequested, this
        /// will simply return the localization key string itself.
        /// </summary>
        public string DisplayName
        {
            get
            {
                LocalizedStringRequest localizedStringRequest = new(localizationKey);
                Input.BroadcastLocalizedStringRequested(localizedStringRequest);
                return string.IsNullOrEmpty(localizedStringRequest.localizedString)
                    ? localizationKey
                    : localizedStringRequest.localizedString;
            }
        }

        [SerializeField] private Sprite sprite;
        public Sprite Sprite => sprite;
    }
}