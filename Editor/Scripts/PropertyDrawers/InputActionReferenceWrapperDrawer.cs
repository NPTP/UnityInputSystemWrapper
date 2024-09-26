using NPTP.InputSystemWrapper.Editor;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Attributes.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(InputActionReferenceWrapper))]
    public class InputActionReferenceWrapperDrawer : PropertyDrawer
    {
        private const string INTERNAL_REFERENCE = "internalReference";
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty referenceProperty = property.FindPropertyRelative(INTERNAL_REFERENCE);
            EditorGUI.PropertyField(position, referenceProperty, new GUIContent(property.name.AsInspectorLabel()));
            EditorGUI.EndProperty();
        }
    }
}