using System.Collections.Generic;
using System.IO;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.UnitySourceGen.Editor;
using NPTP.UnitySourceGen.Editor.Enums;
using NPTP.UnitySourceGen.Editor.Generatable;
using NPTP.UnitySourceGen.Editor.Generatable.Attributes;
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
        private const string EDITOR_ONLY = "UNITY_EDITOR";

        internal static GeneratableFile BuildFile()
        {
            GeneratableTypeDefinition menuItems = SourceGen.NewStaticClass("BindingDataMenuItems", AccessModifier.Internal)
                .InNamespace(GeneratedNamespaces.ROOT)
                .WithDirective("UnityEditor");

            HashSet<string> usedMethodNames = new();
            foreach ((string name, string guid) in FindBindingDataAssets())
            {
                // Selection is by GUID rather than path, so moving or renaming the asset does not break
                // the shortcut until the next generation run.
                menuItems.WithMethod(SourceGen.NewMethod($"Select{UniqueMethodName(name, usedMethodNames)}")
                    .Private()
                    .Static()
                    .Returning<void>()
                    .WithAttribute("MenuItem",
                        AddableAttribute.StringArgument(MENU_PATH + name),
                        "isValidateFunction: false",
                        "100")
                    .Expression($"Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(\"{guid}\"))"));
            }

            // The whole file is editor-only, but lives in the generated runtime assembly.
            return SourceGen.NewFile()
                .OnlyIf(EDITOR_ONLY)
                .WithHeaderComment(Helper.GetGeneratorNoticeLines().ToArray())
                .Containing(menuItems);
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
