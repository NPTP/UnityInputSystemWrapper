using System;
using NPTP.InputSystemWrapper.Editor;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Utilities.Editor
{
    internal static class EditorScriptGetter
    {
        private enum PathType
        {
            File = 0,
            Folder
        }

        internal static string GetSystemFilePath<T>() => GetSystemFilePath(typeof(T));
        internal static string GetSystemFilePath(Type type) => GetSystemPath(type, PathType.File);
        internal static string GetSystemFolderPath<T>() => GetSystemFolderPath(typeof(T));
        internal static string GetSystemFolderPath(Type type) => GetSystemPath(type, PathType.Folder);

        private static string GetSystemPath(Type type, PathType pathType)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Script", new[] {Helper.OfflineInputData.AssetsPathToPackage});
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

                if (scriptAsset == null)
                {
                    continue;
                }

                if (IsEnum(type, scriptAsset) ||
                    scriptAsset.GetClass() == type ||
                    type.IsAssignableFrom(scriptAsset.GetClass()) ||
                    IsRecord(type, scriptAsset) ||
                    IsStruct(type, scriptAsset))
                {
                    string path = Application.dataPath + assetPath.Replace("Assets", string.Empty);
                    if (pathType is PathType.Folder)
                    {
                        // Asset paths in Unity always use forward slash, regardless of platform.
                        path = path.Remove(path.LastIndexOf('/') + 1);
                    }
                    return path;
                }
            }

            return string.Empty;
        }

        private static bool IsEnum(Type type, MonoScript scriptAsset)
        {
            return type.IsEnum && scriptAsset.text.Contains($"enum {type.Name}");
        }

        private static bool IsStruct(Type type, MonoScript scriptAsset)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum && scriptAsset.text.Contains($"struct {type.Name}");
        }

        private static bool IsRecord(Type type, MonoScript scriptAsset)
        {
            return type.IsClass && scriptAsset.text.Contains($"record {type.Name}");
        }
    }
}