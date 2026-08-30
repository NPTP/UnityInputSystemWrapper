using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Editor.Generation;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Editor.CustomEditors
{
    [CustomEditor(typeof(BindingData))]
    internal class BindingDataEditor : UnityEditor.Editor
    {
        /// <summary>
        /// The device layouts offered as one-click fills. Control schemes populate their own binding data
        /// from the devices they actually require, so these are only for filling one in by hand.
        /// </summary>
        private static readonly string[] deviceLayouts = { "Mouse", "Keyboard", "Gamepad", "Joystick" };

        private BindingData targetBindingData;

        private void OnEnable()
        {
            targetBindingData = (BindingData)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            foreach (string deviceLayout in deviceLayouts)
            {
                if (!GUILayout.Button($"Add {deviceLayout} Bindings"))
                {
                    continue;
                }

                AddBindings(deviceLayout);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void AddBindings(string deviceLayout)
        {
            foreach (KeyValuePair<string, string> pathToDisplayName in DeviceControlPathCatalog.GetControlPaths(deviceLayout))
            {
                targetBindingData.EDITOR_AddBinding(pathToDisplayName.Key, pathToDisplayName.Value);
            }

            EditorUtility.SetDirty(targetBindingData);
        }
    }
}
