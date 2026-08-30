using System;
using System.Collections;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Editor.CustomEditors;
using NPTP.InputSystemWrapper.Editor.Generation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.PropertyDrawers
{
    /// <summary>
    /// Draws an ActionReference, picking its action from the project input data's input action asset. On an
    /// ActionReference&lt;T&gt; only the actions read as T are offered, so an assigned reference always has
    /// a value to read. With no asset to pick from, the field is replaced by a note saying so.
    /// </summary>
    [CustomPropertyDrawer(typeof(ActionReference), useForChildren: true)]
    internal class ActionReferenceDrawer : PropertyDrawer
    {
        private const float INDENT = 15f;
        private const string REFERENCE = "reference";
        private const string USE_COMPOSITE_PART = "useCompositePart";
        private const string COMPOSITE_PART = "compositePart";
        private const string PLAYER_ID = "playerID";
        private const string NO_ASSET_NOTE = "No input action asset is assigned.";

        private static GUIStyle NoteStyle => new(EditorStyles.label) { fontStyle = FontStyle.Italic, fontSize = 10 };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // The number of lines is dependent on this bool value (showing composite part of action/binding).
            bool useCompositePart = property.FindPropertyRelative(USE_COMPOSITE_PART).boolValue;
            float multiplier = useCompositePart ? 5 : 4;
            return multiplier * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect currentRect = new Rect(position.x, position.y, position.width, lineHeight);

            EditorGUI.LabelField(currentRect, new GUIContent(property.displayName), EditorStyles.boldLabel);

            // Indent for property fields
            currentRect.x += INDENT;
            currentRect.width -= INDENT;

            SerializedProperty reference = property.FindPropertyRelative(REFERENCE);
            currentRect.y += lineHeight;
            DrawReference(currentRect, reference);

            SerializedProperty useCompositePart = property.FindPropertyRelative(USE_COMPOSITE_PART);
            currentRect.y += lineHeight;
            EditorGUI.PropertyField(currentRect, useCompositePart);

            SerializedProperty playerID = property.FindPropertyRelative(PLAYER_ID);
            currentRect.y += lineHeight;
            EditorGUI.PropertyField(currentRect, playerID);

            if (useCompositePart.boolValue)
            {
                SerializedProperty compositePart = property.FindPropertyRelative(COMPOSITE_PART);
                currentRect.y += lineHeight;
                EditorGUI.PropertyField(currentRect, compositePart);
            }

            EditorGUI.EndProperty();
        }

        private void DrawReference(Rect position, SerializedProperty reference)
        {
            Rect fieldRect = EditorGUI.PrefixLabel(position, new GUIContent(reference.displayName));
            InputData inputData = ProjectAssets.FindProjectInputData();

            if (inputData == null || inputData.InputActionAsset == null)
            {
                EditorGUI.LabelField(fieldRect, NO_ASSET_NOTE, NoteStyle);
                return;
            }

            InputActionReferenceDropdown.DrawWithoutLabel(fieldRect, reference, inputData.InputActionAsset, BuildValueTypeFilter());
        }

        /// <summary>
        /// On an ActionReference&lt;T&gt;, only actions whose values are read as T. On a plain
        /// ActionReference, no filter: any action can be referenced.
        /// </summary>
        private Func<InputAction, bool> BuildValueTypeFilter()
        {
            Type valueType = GetReferencedValueType(fieldInfo.FieldType);
            if (valueType == null)
            {
                return null;
            }

            string valueTypeName = ControlValueTypeNames.FromType(valueType);
            return action => ControlValueTypeNames.FromAction(action) == valueTypeName;
        }

        /// <summary>
        /// The T of the ActionReference the field holds, looking through arrays and lists, or null when the
        /// field is a plain ActionReference.
        /// </summary>
        private static Type GetReferencedValueType(Type fieldType)
        {
            if (fieldType.IsArray)
            {
                fieldType = fieldType.GetElementType();
            }
            else if (fieldType.IsGenericType && typeof(IList).IsAssignableFrom(fieldType))
            {
                fieldType = fieldType.GetGenericArguments()[0];
            }

            while (fieldType != null && fieldType != typeof(ActionReference))
            {
                if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(ActionReference<>))
                {
                    return fieldType.GetGenericArguments()[0];
                }

                fieldType = fieldType.BaseType;
            }

            return null;
        }
    }
}
