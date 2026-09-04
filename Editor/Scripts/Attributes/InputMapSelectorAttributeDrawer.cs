using NPTP.InputSystemWrapper.Attributes;
using NPTP.InputSystemWrapper.Data;
using UnityEditor;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(InputMapSelectorAttribute))]
    internal class InputMapSelectorAttributeDrawer : InputNameStringSelectorAttributeDrawer
    {
        protected override string[] GetNames()
        {
            InputActionAsset asset = Generation.ProjectAssets.TryFindProjectAsset("InputData", out InputData inputData) ? inputData.InputActionAsset : null;
            if (asset == null)
            {
                return System.Array.Empty<string>();
            }

            InputActionMap[] maps = asset.actionMaps.ToArray();
            string[] names = new string[maps.Length];
            for (int i = 0; i < maps.Length; i++)
            {
                names[i] = maps[i].name;
            }

            return names;
        }
    }
}