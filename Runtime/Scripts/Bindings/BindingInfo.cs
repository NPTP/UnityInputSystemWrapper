using System;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

using NPTP.InputSystemWrapper;

namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// Tells us which strings and icons to display for a single binding.
    /// E.g. Given the binding "dpad/up", this might show a sprite with a
    /// D-Pad pointing up and use the display name "D-Pad Up".
    /// </summary>
    [Serializable]
    public struct BindingInfo
    {
        [FormerlySerializedAs("displayName")]
        [SerializeField]
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
                InputRuntime.Current.BroadcastLocalizedStringRequested(localizedStringRequest);
                return string.IsNullOrEmpty(localizedStringRequest.localizedString)
                    ? localizationKey
                    : localizedStringRequest.localizedString;
            }
        }

        [SerializeField] private Sprite sprite;
        public Sprite Sprite => sprite;

#if UNITY_EDITOR
        /// <summary>
        /// Starts a binding off with the display name the input system gives the control, so a generated
        /// asset is readable before anyone edits it.
        /// </summary>
        internal BindingInfo(string localizationKey)
        {
            this.localizationKey = localizationKey;
            sprite = null;
        }
#endif
    }
}