using NPTP.InputSystemWrapper.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// Tells us which strings and icons to display for a single binding.
    /// E.g. Given the binding "dpad/up", this might show a sprite with a
    /// D-Pad pointing up and use the display name "D-Pad Up".
    /// <para>
    /// One asset per control path, addressable, so a device's entries are in memory only while something
    /// is showing them.
    /// </para>
    /// </summary>
    public class BindingInfo : ScriptableObject
    {
        [FormerlySerializedAs("displayName")]
        [SerializeField]
        private string localizationKey;

        /// <summary>
        /// What to show for this binding when no localization is hooked into
        /// Input.OnLocalizedStringRequested, or when the request comes back unfulfilled.
        /// </summary>
        [SerializeField] private string defaultDisplayName;
        /// <summary>
        /// The localized name for this binding, falling back to the default display name.
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
        /// Fill in anything blank, leaving whatever has already been authored alone. Says whether
        /// anything changed, so an asset is only written when it has to be.
        /// </summary>
        internal bool EDITOR_FillBlanks(string localizationKey, string defaultDisplayName)
        {
            bool changed = false;

            if (string.IsNullOrEmpty(this.localizationKey))
            {
                this.localizationKey = localizationKey;
                changed = true;
            }

            if (string.IsNullOrEmpty(this.defaultDisplayName))
            {
                this.defaultDisplayName = defaultDisplayName;
                changed = true;
            }

            return changed;
        }
#endif
    }
}