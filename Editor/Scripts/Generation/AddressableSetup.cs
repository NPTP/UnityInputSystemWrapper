using NPTP.InputSystemWrapper.Editor.Utilities;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Marks the assets the generator creates as addressable, so the references to them resolve at
    /// runtime without the user having to do it themselves. They go in a group of their own, keeping them
    /// out of whatever the project's default group is used for.
    /// </summary>
    internal static class AddressableSetup
    {
        private const string GROUP_NAME = "ISW Data Group";

        internal static void MarkAddressable(string assetGuid, string address)
        {
            if (string.IsNullOrEmpty(assetGuid))
            {
                return;
            }

            // Creates the Addressables settings on first use, so a project that has never opened the
            // Addressables window still works.
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(create: true);
            if (settings == null)
            {
                ISWDebug.LogWarning($"Addressable settings could not be created, so {address} was not marked addressable.");
                return;
            }

            AddressableAssetGroup group = GetOrCreateGroup(settings);
            if (group == null)
            {
                return;
            }

            AddressableAssetEntry existing = settings.FindAssetEntry(assetGuid);
            if (existing != null && existing.parentGroup == group)
            {
                return;
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGuid, group, readOnly: false, postEvent: false);
            if (entry == null)
            {
                ISWDebug.LogWarning($"{address} could not be marked addressable.");
                return;
            }

            entry.address = address;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entry, postEvent: true, settingsModified: true);
            GenerationReport.Record($"{address} (marked addressable in {GROUP_NAME})");
        }

        /// <summary>
        /// Drop an asset's addressable entry, for an asset that is being deleted. An entry left behind
        /// points at nothing and shows as missing in the Addressables window.
        /// </summary>
        internal static void RemoveAddressable(string assetGuid)
        {
            if (string.IsNullOrEmpty(assetGuid))
            {
                return;
            }

            // Nothing is created here: with no settings there is no entry to remove either.
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null || settings.FindAssetEntry(assetGuid) == null)
            {
                return;
            }

            settings.RemoveAssetEntry(assetGuid);
        }

        /// <summary>
        /// The group the generator's assets live in, created with the schemas a group needs to build if it
        /// is not there yet. A group the user has since reconfigured is used as they left it.
        /// </summary>
        private static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings)
        {
            AddressableAssetGroup existing = settings.FindGroup(GROUP_NAME);
            if (existing != null)
            {
                return existing;
            }

            AddressableAssetGroup created = settings.CreateGroup(GROUP_NAME, setAsDefaultGroup: false, readOnly: false,
                postEvent: false, schemasToCopy: null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));

            if (created == null)
            {
                ISWDebug.LogWarning($"The {GROUP_NAME} addressable group could not be created.");
                return null;
            }

            GenerationReport.Record($"{GROUP_NAME} (addressable group created)");
            return created;
        }
    }
}
