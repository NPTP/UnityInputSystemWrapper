﻿using NPTP.InputSystemWrapper.Data;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(EventSystemActionSpecification))]
    public class EventSystemActionSpecificationDrawer : PropertyDrawer
    {
        private const string ACTION_TYPE = "actionType";
        private const string ACTION_REFERENCE = "actionReference";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float halfWidth = position.width / 2;
            Rect actionTypeRect = new Rect(position.x, position.y, halfWidth - 2, position.height);
            Rect actionReferenceRect = new Rect(position.x + halfWidth + 2, position.y, halfWidth - 2, position.height);

            SerializedProperty actionTypeProperty = property.FindPropertyRelative(ACTION_TYPE);
            SerializedProperty actionReferenceProperty = property.FindPropertyRelative(ACTION_REFERENCE);

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