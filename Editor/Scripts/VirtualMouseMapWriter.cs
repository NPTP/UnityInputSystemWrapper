using System.IO;
using NPTP.InputSystemWrapper.Player;
using UnityEditor;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor
{
    /// <summary>
    /// Writes a virtual mouse map into an input action asset, so one can be had without authoring five
    /// actions by hand. Actions already there are left alone, and only the missing ones are added.
    /// </summary>
    internal static class VirtualMouseMapWriter
    {
        internal static void CreateOrComplete(InputActionAsset asset, string mapName)
        {
            if (asset == null || string.IsNullOrEmpty(mapName))
            {
                return;
            }

            InputActionMap actionMap = asset.FindActionMap(mapName, throwIfNotFound: false);
            if (actionMap == null)
            {
                actionMap = asset.AddActionMap(mapName);
            }

            foreach (VirtualMouseMapSpec.ActionSpec actionSpec in VirtualMouseMapSpec.Actions)
            {
                if (actionMap.FindAction(actionSpec.Name, throwIfNotFound: false) != null)
                {
                    continue;
                }

                actionMap.AddAction(actionSpec.Name, actionSpec.ActionType, actionSpec.DefaultBinding,
                    expectedControlLayout: actionSpec.ExpectedControlType);
            }

            Save(asset);
        }

        /// <summary>
        /// An input action asset is a JSON file, so changes made to the object in memory reach the project
        /// only by writing that file out and importing it again.
        /// </summary>
        private static void Save(InputActionAsset asset)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            File.WriteAllText(assetPath, asset.ToJson());
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
