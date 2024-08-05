using UnityEditor;
using UnityEngine;

namespace UnityInputSystemWrapper.Attributes.Editor
{
    public abstract class InputNameStringSelectorAttributeDrawer : PropertyDrawer
    {
        private bool hasInitialized;
        private string[] names;
        
        protected abstract string[] GetNames();

        protected virtual string GetStringValue(SerializedProperty property) => property.stringValue;
        protected virtual void SetStringValue(SerializedProperty property, string value) => property.stringValue = value;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!hasInitialized)
            {
                hasInitialized = true;
                names = GetNames();
            }
            
            // if (property.propertyType == SerializedPropertyType.String)
            // {
                int index = Mathf.Max(0, System.Array.IndexOf(names, GetStringValue(property)));
                EditorGUI.BeginProperty(position, label, property);
                index = EditorGUI.Popup(position, label.text, index, names);
                SetStringValue(property, names[index]);
                EditorGUI.EndProperty();
            // }
            // else
            // {
                // EditorGUI.PropertyField(position, property, label);
            // }
        }
    }
}