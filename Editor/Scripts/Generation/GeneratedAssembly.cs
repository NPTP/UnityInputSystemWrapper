using System.Collections.Generic;
using System.IO;
using NPTP.UnitySourceGen.Editor;
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

        /// <summary>
        /// Only the default location. The folder is found by its assembly definition, so it can be moved
        /// or renamed freely, and existing projects keep whatever folder they already have.
        /// </summary>
        private const string DEFAULT_FOLDER_NAME = "ISW.Generated";

        private const string DEFAULT_ASSETS_FOLDER = "Assets/" + DEFAULT_FOLDER_NAME;
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
                AssetDatabase.CreateFolder("Assets", DEFAULT_FOLDER_NAME);
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

        /// <summary>
        /// Delete generated scripts this run did not produce, so renaming an action map does not leave its
        /// old actions class behind. Only .cs files are touched, never the assembly definition or assets.
        /// </summary>
        internal static void PruneStaleScripts(string folderAssetPath)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:MonoScript", new[] { folderAssetPath }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Substring(0, path.LastIndexOf(''/'')) != folderAssetPath || GenerationReport.WasWritten(path))
                {
                    continue;
                }

                AssetDatabase.DeleteAsset(path);
                GenerationReport.Record($"{path} (deleted, no longer generated)");
            }
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

            SourceGen.WriteToPath(folderAssetPath + "/" + ASSEMBLY_NAME + ".asmdef", string.Join(System.Environment.NewLine, lines) + System.Environment.NewLine);
        }
    }
}
