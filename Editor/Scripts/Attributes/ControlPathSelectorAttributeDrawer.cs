using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Attributes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Editor.Attributes
{
    /// <summary>
    /// Draws a control path string as a searchable dropdown grouped by device, instead of a free text
    /// field. Paths already used elsewhere in the same list are omitted, so the list cannot contain
    /// duplicates and every entry in it is a path the input system actually recognizes.
    /// </summary>
    [CustomPropertyDrawer(typeof(ControlPathSelectorAttribute))]
    internal class ControlPathSelectorAttributeDrawer : PropertyDrawer
    {
        private const string NONE_LABEL = "(None)";
        private const string ARRAY_PATH_SUFFIX = ".Array.data[";

        /// <summary>
        /// The devices offered, in the order they appear in the dropdown. Their controls come from the
        /// input system's layout registry, so the list is whatever Unity actually supports. Paths are
        /// stored with a leading slash and the device name, e.g. "/Keyboard/escape", which is the form the
        /// input system's rebinding API expects.
        /// </summary>
        private static readonly string[] deviceLayouts = { "Keyboard", "Mouse", "Gamepad", "Joystick" };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect dropdownRect = EditorGUI.PrefixLabel(position, label);
            string current = string.IsNullOrEmpty(property.stringValue) ? NONE_LABEL : property.stringValue;

            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(current), FocusType.Keyboard))
            {
                ShowDropdown(dropdownRect, property);
            }

            EditorGUI.EndProperty();
        }

        private static void ShowDropdown(Rect dropdownRect, SerializedProperty property)
        {
            // The drawer's SerializedProperty is reused between elements, so the callback needs its own.
            SerializedProperty target = property.Copy();

            ControlPathDropdown dropdown = new(new AdvancedDropdownState(), GetSelectablePaths(property), selectedPath =>
            {
                target.stringValue = selectedPath;
                target.serializedObject.ApplyModifiedProperties();
            });

            dropdown.Show(dropdownRect);
        }

        /// <summary>
        /// Every path for every offered device, minus the ones already chosen by a sibling element. The
        /// element being edited keeps its own value, so reopening the dropdown on it is not confusing.
        /// </summary>
        private static List<(string Device, string Path)> GetSelectablePaths(SerializedProperty property)
        {
            HashSet<string> alreadyUsed = GetSiblingValues(property);
            List<(string, string)> selectable = new();

            foreach (string device in deviceLayouts)
            {
                foreach (string controlPath in Generation.DeviceControlPathCatalog.GetControlPaths(device).Keys)
                {
                    string fullPath = $"/{device}/{controlPath}";
                    if (!alreadyUsed.Contains(fullPath)) selectable.Add((device, fullPath));
                }
            }

            return selectable;
        }

        private static HashSet<string> GetSiblingValues(SerializedProperty property)
        {
            HashSet<string> used = new();

            int suffixIndex = property.propertyPath.IndexOf(ARRAY_PATH_SUFFIX, StringComparison.Ordinal);
            if (suffixIndex < 0)
            {
                return used;
            }

            string arrayPath = property.propertyPath.Substring(0, suffixIndex);
            SerializedProperty array = property.serializedObject.FindProperty(arrayPath);
            if (array == null || !array.isArray)
            {
                return used;
            }

            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty element = array.GetArrayElementAtIndex(i);
                if (element.propertyPath != property.propertyPath && !string.IsNullOrEmpty(element.stringValue))
                {
                    used.Add(element.stringValue);
                }
            }

            return used;
        }

        private class ControlPathDropdown : AdvancedDropdown
        {
            private const string ROOT_NAME = "Control Path";

            private readonly List<(string Device, string Path)> selectablePaths;
            private readonly Action<string> onPathSelected;

            /// <summary>Maps a dropdown item's id back to the path it stands for. Index 0 clears the field.</summary>
            private readonly List<string> pathsByItemId = new() { string.Empty };

            internal ControlPathDropdown(AdvancedDropdownState state, List<(string, string)> selectablePaths, Action<string> onPathSelected)
                : base(state)
            {
                this.selectablePaths = selectablePaths;
                this.onPathSelected = onPathSelected;
                minimumSize = new Vector2(0, 320);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                AdvancedDropdownItem root = new(ROOT_NAME);
                root.AddChild(new AdvancedDropdownItem(NONE_LABEL) { id = 0 });
                root.AddSeparator();

                string currentDevice = null;
                AdvancedDropdownItem deviceItem = null;

                foreach ((string device, string path) in selectablePaths)
                {
                    if (device != currentDevice)
                    {
                        currentDevice = device;
                        deviceItem = new AdvancedDropdownItem(device);
                        root.AddChild(deviceItem);
                    }

                    // The control's own name reads better under a device group than the full path does,
                    // and search still matches on it.
                    string controlName = path.Substring(path.IndexOf('/', 1) + 1);
                    pathsByItemId.Add(path);
                    deviceItem.AddChild(new AdvancedDropdownItem(controlName) { id = pathsByItemId.Count - 1 });
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (item.id >= 0 && item.id < pathsByItemId.Count)
                {
                    onPathSelected?.Invoke(pathsByItemId[item.id]);
                }
            }
        }
    }
}
