using System.IO;
using NPTP.InputSystemWrapper.Data;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// The package may be installed read-only (from a git URL, resolved into Library/PackageCache), so any
    /// asset the user or the generator has to write to must live in the project instead. This copies the
    /// package's default assets into a Resources folder alongside the generated code on first use.
    /// Binding data lives outside Resources, since it is reached through Addressables.
    /// </summary>
    internal static class ProjectAssets
    {
        private const string RESOURCES_FOLDER_NAME = "Resources";
        private const string BINDING_DATA_FOLDER_NAME = "BindingData";
        private const string PACKAGE_DEFAULTS_FOLDER_NAME = "DefaultAssets";
        private const string INPUT_DATA_NAME = "InputData";

        /// <summary>
        /// The asset-database path of the project's own Resources folder for this package's assets.
        /// </summary>
        internal static string ResourcesFolderAssetPath => GeneratedAssembly.GetOrCreateFolderAssetPath() + "/" + RESOURCES_FOLDER_NAME;

        /// <summary>
        /// Where binding data assets and their entries live. Outside Resources, since they are reached
        /// through Addressables: a Resources folder ships everything in it, which would put a second copy
        /// of all of this in the build. Created if it is not there yet.
        /// </summary>
        internal static string GetOrCreateBindingDataFolder()
        {
            string generatedFolder = GeneratedAssembly.GetOrCreateFolderAssetPath();
            string folderAssetPath = generatedFolder + "/" + BINDING_DATA_FOLDER_NAME;
            if (!AssetDatabase.IsValidFolder(folderAssetPath))
            {
                AssetDatabase.CreateFolder(generatedFolder, BINDING_DATA_FOLDER_NAME);
            }

            return folderAssetPath;
        }

        /// <summary>
        /// The folder holding one binding data asset's entries, named for that asset so the entries sit
        /// beside the binding data they belong to.
        /// </summary>
        internal static string GetOrCreateBindingEntryFolder(string bindingDataAssetName)
        {
            string bindingDataFolder = GetOrCreateBindingDataFolder();
            string folderAssetPath = bindingDataFolder + "/" + bindingDataAssetName;
            if (!AssetDatabase.IsValidFolder(folderAssetPath))
            {
                AssetDatabase.CreateFolder(bindingDataFolder, bindingDataAssetName);
            }

            return folderAssetPath;
        }

        /// <summary>
        /// Copy the package's default assets into the project if they are not there yet, and return the
        /// project's InputData. Existing project assets are never overwritten.
        /// </summary>
        /// <summary>The project's input data asset, or null if it has not been set up yet.</summary>
        internal static InputData FindProjectInputData()
        {
            return TryFindProjectAsset(INPUT_DATA_NAME, out InputData inputData) ? inputData : null;
        }

        internal static InputData EnsureProjectAssets()
        {
            if (TryFindProjectAsset(INPUT_DATA_NAME, out InputData existing))
            {
                return existing;
            }

            string defaultsFolder = FindPackageDefaultsFolder();
            if (string.IsNullOrEmpty(defaultsFolder))
            {
                ISWDebug.LogError($"Could not find the package's {PACKAGE_DEFAULTS_FOLDER_NAME} folder. Input assets cannot be set up in the project.");
                return null;
            }

            string resourcesFolder = ResourcesFolderAssetPath;
            if (!AssetDatabase.IsValidFolder(resourcesFolder))
            {
                AssetDatabase.CreateFolder(GeneratedAssembly.GetOrCreateFolderAssetPath(), RESOURCES_FOLDER_NAME);
            }

            // Copying the folder in one call keeps cross-references between the copied assets intact,
            // which per-asset copies would not.
            CopyContents(defaultsFolder, resourcesFolder);
            AssetDatabase.Refresh();

            if (!TryFindProjectAsset(INPUT_DATA_NAME, out InputData copied))
            {
                ISWDebug.LogError($"Failed to create the project's {INPUT_DATA_NAME} asset.");
                return null;
            }

            GenerationReport.Record($"{resourcesFolder} (project input assets created - edit these, the package's copies are defaults only)");
            return copied;
        }

        private static void CopyContents(string sourceFolder, string destinationFolder)
        {
            foreach (string subfolder in AssetDatabase.GetSubFolders(sourceFolder))
            {
                string name = subfolder.Substring(subfolder.LastIndexOf('/') + 1);
                string destination = destinationFolder + "/" + name;
                if (!AssetDatabase.IsValidFolder(destination))
                {
                    AssetDatabase.CopyAsset(subfolder, destination);
                }
            }

            foreach (string guid in AssetDatabase.FindAssets(string.Empty, new[] { sourceFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path) || path.Substring(0, path.LastIndexOf('/')) != sourceFolder)
                {
                    continue;
                }

                string destination = destinationFolder + "/" + Path.GetFileName(path);
                if (!File.Exists(destination))
                {
                    AssetDatabase.CopyAsset(path, destination);
                }
            }
        }

        /// <summary>
        /// Find an asset of this type that lives in the project rather than in the package, so that the
        /// package's read-only defaults are never picked up by mistake.
        /// </summary>
        internal static bool TryFindProjectAsset<T>(string assetName, out T asset) where T : Object
        {
            foreach (string guid in AssetDatabase.FindAssets($"{assetName} t:{typeof(T).Name}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Assets/") || IsPackageDefault(path))
                {
                    continue;
                }

                asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    return true;
                }
            }

            asset = null;
            return false;
        }

        private static bool IsPackageDefault(string assetPath)
        {
            return assetPath.Contains("/" + PACKAGE_DEFAULTS_FOLDER_NAME + "/");
        }

        private static string FindPackageDefaultsFolder()
        {
            // Located relative to a script that is definitely in the package, so this works whether the
            // package is embedded, in Assets, or resolved into the package cache.
            foreach (string guid in AssetDatabase.FindAssets($"t:Script {nameof(ProjectAssets)}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith($"/{nameof(ProjectAssets)}.cs"))
                {
                    continue;
                }

                // <package>/Editor/Scripts/Generation/ProjectAssets.cs -> <package>
                string packageRoot = path;
                for (int i = 0; i < 4; i++)
                {
                    packageRoot = packageRoot.Substring(0, packageRoot.LastIndexOf('/'));
                }

                string defaults = packageRoot + "/Runtime/" + PACKAGE_DEFAULTS_FOLDER_NAME;
                if (AssetDatabase.IsValidFolder(defaults))
                {
                    return defaults;
                }
            }

            return string.Empty;
        }
    }
}
