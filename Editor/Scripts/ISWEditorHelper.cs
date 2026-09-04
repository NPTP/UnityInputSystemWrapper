using System.Collections.Generic;
using System.Linq;
using NPTP.InputSystemWrapper.Data;
using NPTP.UnitySourceGen.Editor.Syntax;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor
{
    internal static class ISWEditorHelper
    {
        internal static InputData InputData => Generation.ProjectAssets.TryFindProjectAsset(nameof(InputData), out InputData inputData) ? inputData : null;

        /// <summary>A PascalCase identifier, for a generated type or property.</summary>
        internal static string AsType(this string s) => GeneratedIdentifier.SanitizeAsPascalCase(s);

        /// <inheritdoc cref="AsType"/>
        internal static string AsProperty(this string s) => AsType(s);

        internal static List<string> GetGeneratorNoticeLines()
        {
            return new List<string>
            {
                "// --------------------------------------------------------------------------------",
                "// This file was automatically generated. Do not modify it manually.",
                "// --------------------------------------------------------------------------------"
            };
        }

        internal static IEnumerable<string> GetMapNames(InputActionAsset asset)
        {
            return asset.actionMaps.Select(map => map.name);
        }

        internal static void DrawHorizontalLine()
        {
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
        }

        /// <summary>The script field Unity puts at the top of an inspector, which a custom one loses.</summary>
        internal static void ShowScriptInspector<T>(T targetMonoBehaviour) where T : MonoBehaviour
        {
            EditorGUI.BeginDisabledGroup(disabled: true);
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(targetMonoBehaviour), typeof(T), false);
            EditorGUI.EndDisabledGroup();
        }
    }
}
