using NPTP.InputSystemWrapper.Data;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Attributes.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(EventSystemActionSpecification))]
    public class EventSystemActionSpecificationDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float halfWidth = position.width / 2;
            Rect actionTypeRect = new Rect(position.x, position.y, halfWidth - 2, position.height);
            Rect actionReferenceRect = new Rect(position.x + halfWidth + 2, position.y, halfWidth - 2, position.height);

            SerializedProperty actionTypeProperty = property.FindPropertyRelative("actionType");
            SerializedProperty actionReferenceProperty = property.FindPropertyRelative("actionReference");

            EditorGUI.PropertyField(actionTypeRect, actionTypeProperty, GUIContent.none);
            EditorGUI.PropertyField(actionReferenceRect, actionReferenceProperty, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}