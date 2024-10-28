using NPTP.InputSystemWrapper.Actions;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Attributes.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ActionReference))]
    internal class ActionReferenceDrawer : PropertyDrawer
    {
        private const float INDENT = 15f;
        private const string REFERENCE = "reference";
        private const string USE_COMPOSITE_PART = "useCompositePart";
        private const string COMPOSITE_PART = "compositePart";
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // The number of lines is dependent on this bool value (showing composite part of action/binding).
            bool useCompositePart = property.FindPropertyRelative(USE_COMPOSITE_PART).boolValue;
            float multiplier = useCompositePart ? 4 : 3;
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
            EditorGUI.PropertyField(currentRect, reference);
            
            SerializedProperty useCompositePart = property.FindPropertyRelative(USE_COMPOSITE_PART);
            currentRect.y += lineHeight;
            EditorGUI.PropertyField(currentRect, useCompositePart);
            
            if (useCompositePart.boolValue)
            {
                SerializedProperty compositePart = property.FindPropertyRelative(COMPOSITE_PART);
                currentRect.y += lineHeight;
                EditorGUI.PropertyField(currentRect, compositePart);
            }

            EditorGUI.EndProperty();
        }
    }
}