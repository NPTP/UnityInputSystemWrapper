using System.Linq;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities.Editor;
using UnityEditor;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(BindingSelectorAttribute))]
    internal class BindingSelectorAttributeDrawer : InputNameStringSelectorAttributeDrawer
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
