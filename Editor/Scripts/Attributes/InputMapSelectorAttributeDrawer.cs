using NPTP.InputSystemWrapper.Attributes;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Editor.Utilities;
using UnityEditor;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(InputMapSelectorAttribute))]
    internal class InputMapSelectorAttributeDrawer : InputNameStringSelectorAttributeDrawer
    {
        protected override string[] GetNames()
        {
            InputActionAsset asset = EditorAssetGetter.GetFirst<RuntimeInputData>().InputActionAsset;
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