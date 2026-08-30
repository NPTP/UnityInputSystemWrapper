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
        private const float PREVIEW_SIZE = 46f;
        private const float SPRITE_COLUMN_WIDTH = 190f;
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
            SerializedProperty sprite = value.FindPropertyRelative(BindingInfo.EDITOR_SpriteField);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.BeginVertical();
            {
                // The control path identifies the entry and is owned by generation, so it is shown rather
                // than edited.
                EditorGUILayout.LabelField(key.stringValue, ControlPathStyle);
                localizationKey.stringValue = EditorGUILayout.TextField("Localization Key", localizationKey.stringValue);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(SPRITE_COLUMN_WIDTH));
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(sprite, GUIContent.none, GUILayout.Width(SPRITE_COLUMN_WIDTH - PREVIEW_SIZE - ROW_SPACING * 2));
                    DrawSpritePreview(sprite.objectReferenceValue as Sprite);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(ROW_SPACING);
        }

        private static void DrawSpritePreview(Sprite sprite)
        {
            Rect previewRect = GUILayoutUtility.GetRect(PREVIEW_SIZE, PREVIEW_SIZE, GUILayout.Width(PREVIEW_SIZE), GUILayout.Height(PREVIEW_SIZE));

            if (sprite == null)
            {
                EditorGUI.DrawRect(previewRect, new Color(0f, 0f, 0f, 0.1f));
                return;
            }

            // Drawing the sprite's own rect out of its texture shows the right frame of an atlas, which a
            // whole-texture draw would not.
            Texture2D texture = sprite.texture;
            if (texture == null)
            {
                return;
            }

            Rect textureRect = sprite.textureRect;
            Rect normalized = new(textureRect.x / texture.width, textureRect.y / texture.height,
                textureRect.width / texture.width, textureRect.height / texture.height);

            GUI.DrawTextureWithTexCoords(FitToAspect(previewRect, textureRect.width / textureRect.height), texture, normalized, alphaBlend: true);
        }

        /// <summary>Centre the drawn sprite in the square preview without stretching it.</summary>
        private static Rect FitToAspect(Rect available, float aspect)
        {
            if (aspect <= 0f)
            {
                return available;
            }

            float width = available.width;
            float height = width / aspect;

            if (height > available.height)
            {
                height = available.height;
                width = height * aspect;
            }

            return new Rect(available.x + (available.width - width) * 0.5f,
                available.y + (available.height - height) * 0.5f, width, height);
        }
    }
}
