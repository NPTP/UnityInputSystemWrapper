using System.IO;
using NPTP.InputSystemWrapper.Player;
using UnityEditor;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor
{
    /// <summary>
    /// Writes a virtual mouse map into an input action asset, so one can be had without authoring five
    /// actions by hand. Always a new map: a map already in the asset is left exactly as it is.
    /// </summary>
    internal static class VirtualMouseMapWriter
    {
        /// <summary>
        /// Write a map holding every action a virtual mouse needs, bound to a gamepad, and hand back the
        /// name it was given. Named after the one asked for, or after it with a number when that is taken.
        /// </summary>
        internal static string Create(InputActionAsset asset, string desiredMapName)
        {
            if (asset == null)
            {
                return null;
            }

            string mapName = UnusedMapName(asset, string.IsNullOrEmpty(desiredMapName)
                ? VirtualMouseMapSpec.DEFAULT_MAP_NAME
                : desiredMapName);

            InputActionMap actionMap = asset.AddActionMap(mapName);
            foreach (VirtualMouseMapSpec.ActionSpec actionSpec in VirtualMouseMapSpec.Actions)
            {
                actionMap.AddAction(actionSpec.Name, actionSpec.ActionType, actionSpec.DefaultBinding,
                    expectedControlLayout: actionSpec.ExpectedControlType);
            }

            Save(asset);
            return mapName;
        }

        private static string UnusedMapName(InputActionAsset asset, string desiredMapName)
        {
            if (asset.FindActionMap(desiredMapName, throwIfNotFound: false) == null)
            {
                return desiredMapName;
            }

            for (int i = 1; ; i++)
            {
                string candidate = $"{desiredMapName} {i.ToString()}";
                if (asset.FindActionMap(candidate, throwIfNotFound: false) == null)
                {
                    return candidate;
                }
            }
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
