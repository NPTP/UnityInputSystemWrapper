using System.Linq;
using InputSystemWrapper.Utilities.Editor;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityInputSystemWrapper.Data;

namespace UnityInputSystemWrapper.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(BindingSelectorAttribute))]
    public class BindingSelectorAttributeDrawer : InputNameStringSelectorAttributeDrawer
    {
        protected override string[] GetNames()
        {
            InputActionAsset asset = EditorAssetGetter.GetFirst<RuntimeInputData>().InputActionAsset;
            InputBinding[] bindings = asset.bindings.ToArray();
            string[] names = new string[bindings.Length];
            for (int i = 0; i < bindings.Length; i++)
            {
                names[i] = bindings[i].path;
            }

            return names;
        }
    }
}