using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Resolves the folder in the consuming project that generated input code is written to, creating it and
    /// its assembly definition on first generation. Generated code lives outside the package so that the
    /// package can be installed read-only, e.g. by git URL.
    /// </summary>
    internal static class GeneratedAssembly
    {
        /// <summary>
        /// Fixed so that the package's AssemblyInfo can grant this assembly access to its internals
        /// without the package ever referencing it.
        /// </summary>
        internal const string ASSEMBLY_NAME = "InputSystemWrapper.Generated";

        internal const string NAMESPACE = "NPTP.InputSystemWrapper.Generated";

        private const string DEFAULT_ASSETS_FOLDER = "Assets/" + ASSEMBLY_NAME;
        private const string PACKAGE_RUNTIME_ASSEMBLY = "InputSystemWrapper";

        /// <summary>
        /// The asset-database folder path that generated code is written to. Found by locating the generated
        /// assembly definition so the user can move the folder; created at the default path if absent.
        /// </summary>
        internal static string GetOrCreateFolderAssetPath()
        {
            string existing = FindAsmdefFolder();
            if (!string.IsNullOrEmpty(existing))
            {
                return existing;
            }

            if (!AssetDatabase.IsValidFolder(DEFAULT_ASSETS_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets", ASSEMBLY_NAME);
            }

            WriteAsmdef(DEFAULT_ASSETS_FOLDER);
            AssetDatabase.ImportAsset(DEFAULT_ASSETS_FOLDER + "/" + ASSEMBLY_NAME + ".asmdef");
            return DEFAULT_ASSETS_FOLDER;
        }

        internal static string GetOrCreateFolderSystemPath()
        {
            string assetPath = GetOrCreateFolderAssetPath();
            return Application.dataPath + assetPath.Substring("Assets".Length) + Path.DirectorySeparatorChar;
        }

        private static string FindAsmdefFolder()
        {
            foreach (string guid in AssetDatabase.FindAssets($"{ASSEMBLY_NAME} t:AssemblyDefinitionAsset"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == ASSEMBLY_NAME)
                {
                    return path.Substring(0, path.LastIndexOf('/'));
                }
            }

            return string.Empty;
        }

        private static void WriteAsmdef(string folderAssetPath)
        {
            List<string> lines = new()
            {
                "{",
                $"    \"name\": \"{ASSEMBLY_NAME}\",",
                $"    \"rootNamespace\": \"{NAMESPACE}\",",
                "    \"references\": [",
                $"        \"{PACKAGE_RUNTIME_ASSEMBLY}\",",
                "        \"Unity.InputSystem\"",
                "    ],",
                "    \"includePlatforms\": [],",
                "    \"excludePlatforms\": [],",
                "    \"allowUnsafeCode\": false,",
                "    \"overrideReferences\": false,",
                "    \"precompiledReferences\": [],",
                "    \"autoReferenced\": true,",
                "    \"defineConstraints\": [],",
                "    \"versionDefines\": [],",
                "    \"noEngineReferences\": false",
                "}"
            };

            Helper.WriteLinesToFile(lines, Application.dataPath + folderAssetPath.Substring("Assets".Length) + "/" + ASSEMBLY_NAME + ".asmdef");
        }
    }
}
