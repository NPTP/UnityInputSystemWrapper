using System;
using NPTP.InputSystemWrapper.Utilities;
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
    public struct BindingInfo
    {
        [FormerlySerializedAs("displayName")]
        [SerializeField]
        private string localizationKey;

        /// <summary>
        /// What to show for this binding when no localization is hooked into
        /// Input.OnLocalizedStringRequested, or when the request comes back unfulfilled.
        /// </summary>
        [SerializeField] private string defaultDisplayName;
        public string DefaultDisplayName => defaultDisplayName;

        /// <summary>
        /// The localized name for this binding, falling back to <see cref="DefaultDisplayName"/>.
        /// </summary>
        public string DisplayName
        {
            get
            {
                LocalizedStringRequest localizedStringRequest = new(localizationKey);
                InputRuntime.Current.BroadcastLocalizedStringRequested(localizedStringRequest);
                return string.IsNullOrEmpty(localizedStringRequest.localizedString)
                    ? defaultDisplayName
                    : localizedStringRequest.localizedString;
            }
        }

        [SerializeField] private Sprite sprite;
        public Sprite Sprite => sprite;

#if UNITY_EDITOR
        internal const string EDITOR_LocalizationKeyField = nameof(localizationKey);
        internal const string EDITOR_DefaultDisplayNameField = nameof(defaultDisplayName);
        internal const string EDITOR_SpriteField = nameof(sprite);

        /// <summary>
        /// Starts a binding off with a key and a readable name, so a generated asset works and reads
        /// properly before anyone edits it.
        /// </summary>
        internal BindingInfo(string localizationKey, string defaultDisplayName)
        {
            this.localizationKey = localizationKey;
            this.defaultDisplayName = defaultDisplayName;
            sprite = null;
        }

        /// <summary>
        /// A copy with anything blank filled in, leaving whatever has already been authored alone.
        /// </summary>
        internal BindingInfo EDITOR_WithBlanksFilled(string localizationKey, string defaultDisplayName)
        {
            BindingInfo copy = this;
            if (string.IsNullOrEmpty(copy.localizationKey)) copy.localizationKey = localizationKey;
            if (string.IsNullOrEmpty(copy.defaultDisplayName)) copy.defaultDisplayName = defaultDisplayName;
            return copy;
        }

        internal bool EDITOR_HasBlanks => string.IsNullOrEmpty(localizationKey) || string.IsNullOrEmpty(defaultDisplayName);
#endif
    }
}