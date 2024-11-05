using NPTP.InputSystemWrapper.Bindings;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Editor.CustomEditors
{
    [CustomEditor(typeof(BindingData))]
    internal class BindingDataEditor : UnityEditor.Editor
    {
        private BindingData targetBindingData;

        private void OnEnable()
        {
            targetBindingData = (BindingData)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Add Mouse Bindings"))
                targetBindingData.EDITOR_AddMouseBindings();

            if (GUILayout.Button("Add Keyboard Bindings"))
                targetBindingData.EDITOR_AddKeyboardBindings();

            if (GUILayout.Button("Add Gamepad Bindings"))
                targetBindingData.EDITOR_AddGamepadBindings();

            if (GUILayout.Button("Add Joystick Bindings"))
                targetBindingData.EDITOR_AddJoystickBindings();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}