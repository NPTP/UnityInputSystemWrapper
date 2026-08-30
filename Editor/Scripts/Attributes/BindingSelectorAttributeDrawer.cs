using System.Linq;
using NPTP.InputSystemWrapper.Attributes;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Editor.Utilities;
using UnityEditor;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(BindingSelectorAttribute))]
    internal class BindingSelectorAttributeDrawer : InputNameStringSelectorAttributeDrawer
    {
        protected override string[] GetNames()
        {
            InputActionAsset asset = Generation.ProjectAssets.TryFindProjectAsset("RuntimeInputData", out RuntimeInputData runtimeInputData) ? runtimeInputData.InputActionAsset : null;
            if (asset == null)
            {
                return System.Array.Empty<string>();
            }

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
