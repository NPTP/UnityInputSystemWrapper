using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.CustomEditors
{
    /// <summary>
    /// Draws an InputActionReference field as a searchable dropdown grouped by action map, offering only
    /// the actions of a given input action asset. Every reference an asset's actions have is a sub-asset
    /// of it, so the choices are read from there rather than from the whole project.
    /// </summary>
    internal static class InputActionReferenceDropdown
    {
        private const string NONE_LABEL = "(None)";
        private const string NO_ASSET_LABEL = "(No Input Action Asset)";

        internal static void Draw(SerializedProperty property, InputActionAsset asset, GUIContent label = null)
        {
            Draw(EditorGUILayout.GetControlRect(), property, asset, label);
        }

        internal static void Draw(Rect position, SerializedProperty property, InputActionAsset asset, GUIContent label = null)
        {
            label ??= new GUIContent(ObjectNames.NicifyVariableName(property.name));
            Rect dropdownRect = EditorGUI.PrefixLabel(position, label);

            using (new EditorGUI.DisabledScope(asset == null))
            {
                if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(GetLabel(property, asset)), FocusType.Keyboard) && asset != null)
                {
                    ShowDropdown(dropdownRect, property, asset);
                }
            }
        }

        /// <summary>Draws with no prefix label, for a field already labelled by whatever contains it.</summary>
        internal static void DrawWithoutLabel(Rect position, SerializedProperty property, InputActionAsset asset)
        {
            using (new EditorGUI.DisabledScope(asset == null))
            {
                if (EditorGUI.DropdownButton(position, new GUIContent(GetLabel(property, asset)), FocusType.Keyboard) && asset != null)
                {
                    ShowDropdown(position, property, asset);
                }
            }
        }

        private static string GetLabel(SerializedProperty property, InputActionAsset asset)
        {
            if (asset == null)
            {
                return NO_ASSET_LABEL;
            }

            return property.objectReferenceValue is InputActionReference reference && reference.action != null
                ? Describe(reference)
                : NONE_LABEL;
        }

        private static string Describe(InputActionReference reference)
        {
            InputAction action = reference.action;
            return action.actionMap == null ? action.name : $"{action.actionMap.name}/{action.name}";
        }

        private static void ShowDropdown(Rect dropdownRect, SerializedProperty property, InputActionAsset asset)
        {
            SerializedProperty target = property.Copy();

            ActionReferenceDropdown dropdown = new(new AdvancedDropdownState(), GetSelectableReferences(asset), selected =>
            {
                target.objectReferenceValue = selected;
                target.serializedObject.ApplyModifiedProperties();
            });

            dropdown.Show(dropdownRect);
        }

        /// <summary>
        /// The asset's action references, in map order. A reference whose action has since been deleted
        /// stays out of the list, so nothing unselectable is offered.
        /// </summary>
        private static List<(string Map, InputActionReference Reference)> GetSelectableReferences(InputActionAsset asset)
        {
            List<(string, InputActionReference)> selectable = new();
            string assetPath = AssetDatabase.GetAssetPath(asset);

            if (string.IsNullOrEmpty(assetPath))
            {
                return selectable;
            }

            Dictionary<string, List<InputActionReference>> referencesByMap = new();
            foreach (UnityEngine.Object subAsset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (subAsset is not InputActionReference reference || reference.action?.actionMap == null)
                {
                    continue;
                }

                string mapName = reference.action.actionMap.name;
                if (!referencesByMap.TryGetValue(mapName, out List<InputActionReference> mapReferences))
                {
                    mapReferences = new List<InputActionReference>();
                    referencesByMap.Add(mapName, mapReferences);
                }

                mapReferences.Add(reference);
            }

            // Walk the asset's own maps and actions, so the dropdown is ordered the way the asset is.
            foreach (InputActionMap map in asset.actionMaps)
            {
                if (!referencesByMap.TryGetValue(map.name, out List<InputActionReference> mapReferences))
                {
                    continue;
                }

                foreach (InputAction action in map.actions)
                {
                    InputActionReference match = mapReferences.Find(reference => reference.action.id == action.id);
                    if (match != null)
                    {
                        selectable.Add((map.name, match));
                    }
                }
            }

            return selectable;
        }

        private class ActionReferenceDropdown : AdvancedDropdown
        {
            private const string ROOT_NAME = "Input Action";

            private readonly List<(string Map, InputActionReference Reference)> selectableReferences;
            private readonly Action<InputActionReference> onReferenceSelected;

            /// <summary>Maps a dropdown item's id back to the reference it stands for. Index 0 clears the field.</summary>
            private readonly List<InputActionReference> referencesByItemId = new() { null };

            internal ActionReferenceDropdown(AdvancedDropdownState state,
                List<(string, InputActionReference)> selectableReferences, Action<InputActionReference> onReferenceSelected)
                : base(state)
            {
                this.selectableReferences = selectableReferences;
                this.onReferenceSelected = onReferenceSelected;
                minimumSize = new Vector2(0, 320);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                AdvancedDropdownItem root = new(ROOT_NAME);
                root.AddChild(new AdvancedDropdownItem(NONE_LABEL) { id = 0 });
                root.AddSeparator();

                string currentMap = null;
                AdvancedDropdownItem mapItem = null;

                foreach ((string map, InputActionReference reference) in selectableReferences)
                {
                    if (map != currentMap)
                    {
                        currentMap = map;
                        mapItem = new AdvancedDropdownItem(map);
                        root.AddChild(mapItem);
                    }

                    referencesByItemId.Add(reference);
                    mapItem.AddChild(new AdvancedDropdownItem(reference.action.name) { id = referencesByItemId.Count - 1 });
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                onReferenceSelected?.Invoke(referencesByItemId[item.id]);
            }
        }
    }
}
