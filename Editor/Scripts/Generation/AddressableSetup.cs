using NPTP.InputSystemWrapper.Editor.Utilities;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Marks the assets the generator creates as addressable, so the references to them resolve at
    /// runtime without the user having to do it themselves. An asset already marked is left where the
    /// user put it, group and address included.
    /// </summary>
    internal static class AddressableSetup
    {
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

            if (settings.FindAssetEntry(assetGuid) != null)
            {
                return;
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGuid, settings.DefaultGroup, readOnly: false, postEvent: false);
            if (entry == null)
            {
                ISWDebug.LogWarning($"{address} could not be marked addressable.");
                return;
            }

            entry.address = address;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entry, postEvent: true, settingsModified: true);
            GenerationReport.Record($"{address} (marked addressable)");
        }
    }
}
