using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NPTP.InputSystemWrapper.Editor.Utilities
{
    internal static class EditorAssetGetter
    {
        private const char ASSET_DATABASE_SEPARATOR_CHAR = '/';
        
        internal static T GetFirst<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        
        internal static string GetSystemFilePath<T>(T asset) where T : Object
        {
            if (asset == null)
            {
                return string.Empty;
            }
            
            return Application.dataPath + AssetDatabase.GetAssetPath(asset).Replace("Assets", string.Empty);
        }
        
        internal static string GetSystemFolderPath<T>(T asset) where T : Object
        {
            string filePath = GetSystemFilePath(asset);
            return string.IsNullOrEmpty(filePath)
                ? string.Empty
                : filePath[..filePath.LastIndexOf(ASSET_DATABASE_SEPARATOR_CHAR)];
        }
    }
}