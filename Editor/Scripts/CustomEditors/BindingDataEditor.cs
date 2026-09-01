using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Utilities.Collections;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Editor.CustomEditors
{
    /// <summary>
    /// Shows every binding in the asset expanded, one row each: the control path and its localization key
    /// on the left, the sprite and a preview of it on the right.
    /// <para>
    /// There is nothing here for adding or removing entries. The set of controls belongs to the device, so
    /// it is filled in by input code generation from the input system's layout registry - what is editable
    /// is how each control is presented, not which controls exist.
    /// </para>
    /// </summary>
    [CustomEditor(typeof(BindingData))]
    internal class BindingDataEditor : UnityEditor.Editor
    {
        private const float SPRITE_SIZE = 64f;
        private const float ROW_SPACING = 2f;

        private SerializedProperty keyValueCombos;

        /// <summary>
        /// Unity's own search field, so this reads as the search boxes everywhere else in the editor do -
        /// magnifying glass, clear button and all - rather than a plain text field.
        /// </summary>
        private SearchField searchField;

        private string searchFilter = string.Empty;

        private GUIStyle ControlPathStyle => new(EditorStyles.boldLabel) { wordWrap = true };

        private void OnEnable()
        {
            searchField = new SearchField();

            SerializedProperty dictionary = serializedObject.FindProperty(BindingData.EDITOR_DictionaryField);
            keyValueCombos = dictionary?.FindPropertyRelative(SerializableDictionary<string, BindingInfo>.EDITOR_KeyValueCombosField);
        }

        public override void OnInspectorGUI()
        {
            if (keyValueCombos == null)
            {
                EditorGUILayout.HelpBox("Could not read the binding dictionary on this asset.", MessageType.Error);
                return;
            }

            serializedObject.Update();

            DrawHeader(out int shownCount);

            for (int i = 0; i < keyValueCombos.arraySize; i++)
            {
                SerializedProperty combo = keyValueCombos.GetArrayElementAtIndex(i);
                SerializedProperty key = combo.FindPropertyRelative(KeyValueCombo<string, BindingInfo>.EDITOR_KeyField);

                if (!MatchesFilter(key.stringValue))
                {
                    continue;
                }

                DrawEntry(key, combo.FindPropertyRelative(KeyValueCombo<string, BindingInfo>.EDITOR_ValueField));
            }

            if (shownCount == 0)
            {
                EditorGUILayout.HelpBox(keyValueCombos.arraySize == 0
                        ? "No bindings yet. They are filled in when input code is generated, from the devices your control schemes use."
                        : "No bindings match the search.",
                    MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader(out int shownCount)
        {
            shownCount = 0;
            for (int i = 0; i < keyValueCombos.arraySize; i++)
            {
                SerializedProperty key = keyValueCombos.GetArrayElementAtIndex(i)
                    .FindPropertyRelative(KeyValueCombo<string, BindingInfo>.EDITOR_KeyField);
                if (MatchesFilter(key.stringValue)) shownCount++;
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                searchFilter = searchField.OnToolbarGUI(searchFilter);
                GUILayout.Label($"{shownCount} / {keyValueCombos.arraySize}", EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private bool MatchesFilter(string controlPath)
        {
            return string.IsNullOrEmpty(searchFilter) ||
                   (controlPath != null && controlPath.IndexOf(searchFilter, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void DrawEntry(SerializedProperty key, SerializedProperty value)
        {
            SerializedProperty localizationKey = value.FindPropertyRelative(BindingInfo.EDITOR_LocalizationKeyField);
            SerializedProperty defaultDisplayName = value.FindPropertyRelative(BindingInfo.EDITOR_DefaultDisplayNameField);
            SerializedProperty sprite = value.FindPropertyRelative(BindingInfo.EDITOR_SpriteField);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.BeginVertical();
            {
                // The control path identifies the entry and is owned by generation, so it is shown rather
                // than edited.
                EditorGUILayout.LabelField(key.stringValue, ControlPathStyle);

                // Labels above the fields rather than beside them, so each gets the column's full width.
                EditorGUILayout.LabelField("Localization Key", EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(localizationKey, GUIContent.none);

                EditorGUILayout.LabelField("Default Display Name", EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(defaultDisplayName, GUIContent.none);

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndVertical();

            DrawSpriteField(sprite);

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(ROW_SPACING);
        }

        /// <summary>
        /// A large object field: a preview of the sprite inside a bordered box, keeping the same box when
        /// nothing is assigned, which is the same control Unity's own inspectors use for sprite slots.
        /// <para>
        /// It fills the row's height, so it stays level with however many fields the column beside it has.
        /// </para>
        /// </summary>
        private static void DrawSpriteField(SerializedProperty sprite)
        {
            Rect spriteRect = GUILayoutUtility.GetRect(SPRITE_SIZE, SPRITE_SIZE,
                GUILayout.Width(SPRITE_SIZE), GUILayout.MinHeight(SPRITE_SIZE), GUILayout.ExpandHeight(true));

            EditorGUI.ObjectField(spriteRect, sprite, typeof(Sprite), GUIContent.none);
        }
    }
}
