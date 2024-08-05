using InputSystemWrapper.Utilities.Editor;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityInputSystemWrapper.Data;

namespace UnityInputSystemWrapper.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(InputMapSelectorAttribute))]
    public class InputMapSelectorAttributeDrawer : InputNameStringSelectorAttributeDrawer
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