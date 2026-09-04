using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Components;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Editor.CustomEditors
{
    [CustomEditor(typeof(InputData))]
    internal class InputDataEditor : UnityEditor.Editor
    {
        private GUIStyle HeaderStyle => new(EditorStyles.label) { fontStyle = FontStyle.Bold, fontSize = 14 };
        private GUIStyle WarningStyle => new(EditorStyles.label) { fontStyle = FontStyle.Italic, fontSize = 12, normal = new GUIStyleState {textColor = Color.yellow}};
        private GUIStyle SpecialNoteStyle => new(EditorStyles.label) { fontStyle = FontStyle.Italic, fontSize = 10 };

        private SerializedProperty inputActionAsset;
        private SerializedProperty customLayouts;
        private SerializedProperty customBindings;
        private SerializedProperty customInteractions;

        private SerializedProperty initializationMode;

        private SerializedProperty allowEnablingVirtualMouse;
        private SerializedProperty virtualMouseActionMapName;
        private SerializedProperty virtualMouseCursorMode;
        private SerializedProperty virtualMouseCursorPrefab;
        private SerializedProperty virtualMouseCreatesOwnCanvas;
        private SerializedProperty defaultContextIndex;
        private SerializedProperty authoredContexts;


        private SerializedProperty loadAllBindingOverridesOnInitialize;
        private SerializedProperty bindingSerializationMode;
        private SerializedProperty bindingExcludedPaths;
        private SerializedProperty bindingCancelPaths;

        private SerializedProperty moveRepeatDelay;
        private SerializedProperty moveRepeatRate;
        private SerializedProperty deselectOnBackgroundClick;
        private SerializedProperty pointerBehavior;
        private SerializedProperty cursorLockBehavior;

        private SerializedProperty point;
        private SerializedProperty leftClick;
        private SerializedProperty middleClick;
        private SerializedProperty rightClick;
        private SerializedProperty scrollWheel;
        private SerializedProperty move;
        private SerializedProperty submit;
        private SerializedProperty cancel;
        private SerializedProperty trackedDevicePosition;
        private SerializedProperty trackedDeviceOrientation;

        private void OnEnable()
        {
            inputActionAsset = serializedObject.FindProperty(nameof(inputActionAsset));
            customLayouts = serializedObject.FindProperty(nameof(customLayouts));
            customBindings = serializedObject.FindProperty(nameof(customBindings));
            customInteractions = serializedObject.FindProperty(nameof(customInteractions));

            initializationMode = serializedObject.FindProperty(nameof(initializationMode));
            allowEnablingVirtualMouse = serializedObject.FindProperty(InputData.EDITOR_AllowEnablingVirtualMouseField);
            virtualMouseActionMapName = serializedObject.FindProperty(InputData.EDITOR_VirtualMouseActionMapNameField);
            virtualMouseCursorMode = serializedObject.FindProperty(InputData.EDITOR_VirtualMouseCursorModeField);
            virtualMouseCursorPrefab = serializedObject.FindProperty(InputData.EDITOR_VirtualMouseCursorPrefabField);
            virtualMouseCreatesOwnCanvas = serializedObject.FindProperty(InputData.EDITOR_VirtualMouseCreatesOwnCanvasField);
            defaultContextIndex = serializedObject.FindProperty(InputData.EDITOR_DefaultContextIndexField);
            authoredContexts = serializedObject.FindProperty(nameof(authoredContexts));


            loadAllBindingOverridesOnInitialize = serializedObject.FindProperty(InputData.EDITOR_LoadAllBindingOverridesOnInitializeField);
            bindingSerializationMode = serializedObject.FindProperty(InputData.EDITOR_BindingSerializationModeField);
            bindingExcludedPaths = serializedObject.FindProperty(InputData.EDITOR_BindingExcludedPathsField);
            bindingCancelPaths = serializedObject.FindProperty(InputData.EDITOR_BindingCancelPathsField);

            moveRepeatDelay = serializedObject.FindProperty(nameof(moveRepeatDelay));
            moveRepeatRate = serializedObject.FindProperty(nameof(moveRepeatRate));
            deselectOnBackgroundClick = serializedObject.FindProperty(nameof(deselectOnBackgroundClick));
            pointerBehavior = serializedObject.FindProperty(nameof(pointerBehavior));
            cursorLockBehavior = serializedObject.FindProperty(nameof(cursorLockBehavior));

            point = serializedObject.FindProperty(nameof(point));
            leftClick = serializedObject.FindProperty(nameof(leftClick));
            middleClick = serializedObject.FindProperty(nameof(middleClick));
            rightClick = serializedObject.FindProperty(nameof(rightClick));
            scrollWheel = serializedObject.FindProperty(nameof(scrollWheel));
            move = serializedObject.FindProperty(nameof(move));
            submit = serializedObject.FindProperty(nameof(submit));
            cancel = serializedObject.FindProperty(nameof(cancel));
            trackedDevicePosition = serializedObject.FindProperty(nameof(trackedDevicePosition));
            trackedDeviceOrientation = serializedObject.FindProperty(nameof(trackedDeviceOrientation));
        }

        private void DrawDefaultContextPopup()
        {
            InputContextInfo[] contexts = ((InputData)target).AuthoredContexts;
            if (contexts == null || contexts.Length == 0)
            {
                EditorGUILayout.LabelField("Default Context", "No input contexts defined");
                return;
            }

            string[] names = contexts.Select(context => context.Name).ToArray();
            int index = Mathf.Clamp(defaultContextIndex.intValue, 0, names.Length - 1);
            defaultContextIndex.intValue = EditorGUILayout.Popup("Default Context", index, names);
        }

        private void DrawHeader(string text)
        {
            EditorGUILayout.LabelField(text, HeaderStyle);
            EditorGUILayout.Space(4);
        }

        private void DrawWarning(string text)
        {
            EditorGUILayout.LabelField(text, WarningStyle);
        }

        private void DrawSpecialNote(string text)
        {
            EditorGUILayout.LabelField(text, SpecialNoteStyle);
        }

        /// <summary>
        /// The actions the event system drives, each picked from the assigned input action asset. With
        /// no asset assigned there is nothing to pick from, so the fields are left out entirely.
        /// </summary>
        private void DrawDefaultEventSystemActions()
        {
            EditorGUILayout.Space();
            DrawHeader("Default Event System Actions");

            InputActionAsset asset = ((InputData)target).InputActionAsset;
            if (asset == null)
            {
                DrawSpecialNote("No input action asset is assigned.");
                return;
            }

            InputActionReferenceDropdown.Draw(point, asset);
            InputActionReferenceDropdown.Draw(leftClick, asset);
            InputActionReferenceDropdown.Draw(middleClick, asset);
            InputActionReferenceDropdown.Draw(rightClick, asset);
            InputActionReferenceDropdown.Draw(scrollWheel, asset);
            InputActionReferenceDropdown.Draw(move, asset);
            InputActionReferenceDropdown.Draw(submit, asset);
            InputActionReferenceDropdown.Draw(cancel, asset);
            InputActionReferenceDropdown.Draw(trackedDevicePosition, asset);
            InputActionReferenceDropdown.Draw(trackedDeviceOrientation, asset);
        }

        /// <summary>
        /// The map a player's virtual mouse is driven by, what is wrong with it, and a button to write the
        /// actions it is missing.
        /// </summary>
        private void DrawVirtualMouse()
        {
            EditorGUILayout.Space();
            ISWEditorHelper.DrawHorizontalLine();

            DrawHeader("Virtual Mouse");

            InputActionAsset asset = ((InputData)target).InputActionAsset;
            if (asset == null)
            {
                DrawSpecialNote("No input action asset is assigned.");
                return;
            }

            EditorGUILayout.PropertyField(allowEnablingVirtualMouse, new GUIContent("Allow Enabling Virtual Mouse"));

            EditorGUI.BeginDisabledGroup(!allowEnablingVirtualMouse.boolValue);
            {
                EditorGUILayout.PropertyField(virtualMouseActionMapName, new GUIContent("Action Map"));
                DrawVirtualMouseMapProblems(asset);

                EditorGUILayout.PropertyField(virtualMouseCursorMode, new GUIContent("Cursor Mode"));

                EditorGUILayout.PropertyField(virtualMouseCursorPrefab, new GUIContent("Cursor Prefab"));
                DrawVirtualMouseCursorProblems();

                EditorGUILayout.PropertyField(virtualMouseCreatesOwnCanvas, new GUIContent("Creates Own Canvas"));
            }
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>What stops the chosen prefab from drawing a cursor, or nothing when it can.</summary>
        private void DrawVirtualMouseCursorProblems()
        {
            if (virtualMouseCursorPrefab.objectReferenceValue is not GameObject prefab)
            {
                DrawSpecialNote("With no cursor prefab the mouse still moves and clicks, but nothing is drawn for it.");
                return;
            }

            ISWVirtualMouseUI cursorUI = prefab.GetComponent<ISWVirtualMouseUI>();
            if (cursorUI == null)
            {
                DrawWarning($"\"{prefab.name}\" has no {nameof(ISWVirtualMouseUI)} on its root.");
                return;
            }

            if (cursorUI.CursorTransform == null)
            {
                DrawWarning($"The {nameof(ISWVirtualMouseUI)} on \"{prefab.name}\" has no cursor transform assigned.");
            }

            if (cursorUI.CursorGraphic == null)
            {
                DrawWarning($"The {nameof(ISWVirtualMouseUI)} on \"{prefab.name}\" has no cursor graphic assigned.");
            }
        }

        /// <summary>
        /// What the chosen map is missing, and the button that writes it, or nothing at all when the map
        /// holds what it should.
        /// </summary>
        private void DrawVirtualMouseMapProblems(InputActionAsset asset)
        {
            string mapName = virtualMouseActionMapName.stringValue;
            if (string.IsNullOrEmpty(mapName))
            {
                DrawWarning("No action map is chosen, so no player can drive a virtual mouse.");
                return;
            }

            InputActionMap actionMap = asset.FindActionMap(mapName, throwIfNotFound: false);
            List<string> problems = VirtualMouseMapSpec.Problems(actionMap);
            if (problems.Count == 0)
            {
                return;
            }

            foreach (string problem in problems)
            {
                DrawWarning(problem);
            }

            // Always a new map, never a change to one already in the asset, so nothing authored is touched.
            if (GUILayout.Button("Create Virtual Mouse Map"))
            {
                string createdMapName = VirtualMouseMapWriter.Create(asset, mapName);
                if (!string.IsNullOrEmpty(createdMapName))
                {
                    virtualMouseActionMapName.stringValue = createdMapName;
                }
            }

            EditorGUILayout.Space(2);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader("Input Action Asset");
            EditorGUILayout.PropertyField(inputActionAsset);
            if (inputActionAsset.objectReferenceValue == null)
            {
                DrawWarning("An Input Action Asset is required. Nothing can be generated without one.");
            }

            ISWEditorHelper.DrawHorizontalLine();

            DrawHeader("Custom Setups");
            DrawSpecialNote("Layouts, bindings and interactions registered with the input system before any player is set up.");
            EditorGUILayout.PropertyField(customLayouts);
            EditorGUILayout.PropertyField(customBindings);
            EditorGUILayout.PropertyField(customInteractions);

            ISWEditorHelper.DrawHorizontalLine();

            DrawHeader("Initialization");
            EditorGUILayout.PropertyField(initializationMode);

            ISWEditorHelper.DrawHorizontalLine();

            DrawHeader("Input Contexts");
            DrawDefaultContextPopup();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(authoredContexts);

            ISWEditorHelper.DrawHorizontalLine();

            DrawHeader("Bindings");
            EditorGUILayout.PropertyField(loadAllBindingOverridesOnInitialize);
            EditorGUILayout.PropertyField(bindingSerializationMode);
            if (((InputData)target).BindingSerializationMode.UsesEvent())
            {
                DrawSpecialNote("Handle ISW.OnBindingsSaveRequested to save.");
                DrawSpecialNote("Handle ISW.OnBindingsLoadRequested and populate the request to load.");
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(bindingExcludedPaths);
            EditorGUILayout.PropertyField(bindingCancelPaths);

            ISWEditorHelper.DrawHorizontalLine();

            DrawHeader("Event System");
            EditorGUILayout.PropertyField(moveRepeatDelay);
            EditorGUILayout.PropertyField(moveRepeatRate);
            EditorGUILayout.PropertyField(deselectOnBackgroundClick);
            EditorGUILayout.PropertyField(pointerBehavior);
            EditorGUILayout.PropertyField(cursorLockBehavior);

            DrawDefaultEventSystemActions();

            DrawVirtualMouse();

            serializedObject.ApplyModifiedProperties();
        }
    }
}