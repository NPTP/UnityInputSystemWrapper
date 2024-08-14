using System;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Utilities.Editor
{
    public static class EditorScriptGetter
    {
        private enum PathType
        {
            File = 0,
            Folder
        }

        public static string GetSystemFilePath<T>() => GetSystemFilePath(typeof(T));
        public static string GetSystemFilePath(Type type) => GetSystemPath(type, PathType.File);
        public static string GetSystemFolderPath<T>() => GetSystemFolderPath(typeof(T));
        public static string GetSystemFolderPath(Type type) => GetSystemPath(type, PathType.Folder);

        private static string GetSystemPath(Type type, PathType pathType)
        {
            bool typeIsEnum = type.IsEnum;
            
            string[] guids = AssetDatabase.FindAssets($"t:Script");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

                if (scriptAsset == null)
                {
                    continue;
                }

                if (typeIsEnum && scriptAsset.text.Contains($"enum {type.Name}") ||
                    scriptAsset.GetClass() == type ||
                    type.IsAssignableFrom(scriptAsset.GetClass()) ||
                    scriptAsset.text.Contains($"record {type.Name}"))
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
    }
}