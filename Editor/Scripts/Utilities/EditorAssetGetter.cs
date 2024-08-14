using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Utilities.Editor
{
    public static class EditorAssetGetter
    {
        public static T GetFirst<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        
        public static string GetSystemFilePath<T>(T asset) where T : Object
        {
            if (asset == null)
            {
                return string.Empty;
            }
            
            return Application.dataPath + AssetDatabase.GetAssetPath(asset).Replace("Assets", string.Empty);
        }
    }
}