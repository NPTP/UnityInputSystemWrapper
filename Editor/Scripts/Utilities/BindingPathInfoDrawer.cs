using UnityInputSystemWrapper.Data;
using UnityEditor;
using UnityEngine;

namespace InputSystemWrapper.Utilities.Editor
{
    [CustomPropertyDrawer(typeof(BindingPathInfo))]
    public class BindingPathInfoDrawer : PropertyDrawer
    {
        private static float PropertyHeight => 70;
        private static float VerticalSpacing => EditorGUIUtility.standardVerticalSpacing;
        private static float LineHeight => EditorGUIUtility.singleLineHeight;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => PropertyHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            SerializedProperty iconProperty = property.FindPropertyRelative("icon");
            EditorGUI.ObjectField(new Rect(position.x, position.y, PropertyHeight, PropertyHeight), iconProperty, typeof(Sprite), GUIContent.none);

            float boolContentsWidth = position.width / 2 - PropertyHeight - VerticalSpacing;
            Rect boolLabelPosition = new Rect(position.x + PropertyHeight + VerticalSpacing * 2, position.y, boolContentsWidth, LineHeight);
            Rect boolPosition = new Rect(boolLabelPosition.x + boolLabelPosition.width + VerticalSpacing * 2, boolLabelPosition.y, boolContentsWidth, LineHeight);
            Rect displayNamePosition = new Rect(boolLabelPosition.x, boolLabelPosition.y + LineHeight, position.width - PropertyHeight - VerticalSpacing, LineHeight);

            SerializedProperty overrideDisplayNameProperty = property.FindPropertyRelative("overrideDisplayName");
            SerializedProperty overrideNameProperty = property.FindPropertyRelative("overrideName");
            EditorGUI.LabelField(boolLabelPosition, "Override Display Name?");
            EditorGUI.PropertyField(boolPosition, overrideDisplayNameProperty, GUIContent.none);
            if (overrideDisplayNameProperty.boolValue)
            {
                EditorGUI.PropertyField(displayNamePosition, overrideNameProperty, GUIContent.none);
            }

            EditorGUI.EndProperty();
        }
    }
}