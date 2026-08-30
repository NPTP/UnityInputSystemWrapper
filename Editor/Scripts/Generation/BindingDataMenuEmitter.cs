using System.Collections.Generic;
using System.IO;
using NPTP.InputSystemWrapper.Bindings;
using UnityEditor;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Emits a menu item per BindingData asset in the project, under the Input toolbar's Binding Data
    /// submenu. MenuItem paths must be compile-time constants, so this list can only exist as generated code.
    /// </summary>
    internal static class BindingDataMenuEmitter
    {
        private const string MENU_PATH = "Input/Binding Data/";

        internal static List<string> BuildLines()
        {
            List<string> lines = new()
            {
                "#if UNITY_EDITOR",
                "using UnityEditor;",
                string.Empty
            };

            lines.AddRange(Helper.GetGeneratorNoticeLines());
            lines.Add($"namespace {GeneratedNamespaces.ROOT}");
            lines.Add("{");
            lines.Add("    internal static class BindingDataMenuItems");
            lines.Add("    {");

            List<(string Name, string Guid)> bindingDataAssets = FindBindingDataAssets();
            if (bindingDataAssets.Count == 0)
            {
                lines.Add("        // No BindingData assets found in the project when this was generated.");
            }

            HashSet<string> usedMethodNames = new();
            foreach ((string name, string guid) in bindingDataAssets)
            {
                string methodName = UniqueMethodName(name, usedMethodNames);
                lines.Add($"        [MenuItem(\"{MENU_PATH}{name}\", isValidateFunction: false, 100)]");
                lines.Add($"        private static void Select{methodName}()");
                lines.Add("        {");
                lines.Add($"            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(\"{guid}\"));");
                lines.Add("        }");
                lines.Add(string.Empty);
            }

            lines.Add("    }");
            lines.Add("}");
            lines.Add("#endif");
            return lines;
        }

        /// <summary>
        /// Every BindingData the user can actually edit, i.e. everything except the package's read-only defaults.
        /// </summary>
        private static List<(string Name, string Guid)> FindBindingDataAssets()
        {
            List<(string, string)> found = new();
            foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(BindingData)}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Assets/") || path.Contains("/DefaultAssets/"))
                {
                    continue;
                }

                found.Add((Path.GetFileNameWithoutExtension(path), guid));
            }

            found.Sort((a, b) => string.CompareOrdinal(a.Item1, b.Item1));
            return found;
        }

        private static string UniqueMethodName(string assetName, HashSet<string> used)
        {
            string baseName = assetName.AsEnumMember().AsType();
            if (string.IsNullOrEmpty(baseName))
            {
                baseName = "BindingData";
            }

            string methodName = baseName;
            int suffix = 1;
            while (!used.Add(methodName))
            {
                methodName = baseName + ++suffix;
            }

            return methodName;
        }
    }
}
