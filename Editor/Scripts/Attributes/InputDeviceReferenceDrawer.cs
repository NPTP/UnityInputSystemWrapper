using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(InputDeviceReference))]
    internal class InputDeviceReferenceDrawer : PropertyDrawer
    {
        private bool hasInitialized;
        private string[] names;
        
        private string[] GetNames()
        {
            Type baseType = typeof(InputDevice);
            Assembly assembly = Assembly.GetAssembly(typeof(InputDevice));
            IEnumerable<Type> deviceTypes = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(baseType));
            List<string> deviceNames = deviceTypes.Select(deviceType => deviceType.Name).ToList();
            return deviceNames.ToArray();
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!hasInitialized)
            {
                hasInitialized = true;
                names = GetNames();
            }
            
            SerializedProperty deviceTypeName = property.FindPropertyRelative("deviceTypeName");
            
            int index = Mathf.Max(0, System.Array.IndexOf(names, deviceTypeName.stringValue));
            EditorGUI.BeginProperty(position, label, property);
            index = EditorGUI.Popup(position, label.text, index, names);
            deviceTypeName.stringValue = names[index];
            EditorGUI.EndProperty();
        }
    }
}