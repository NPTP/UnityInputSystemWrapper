using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Editor.Utilities;
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

        private SerializedProperty defaultContextIndex;
        private SerializedProperty authoredContexts;

        private SerializedProperty controlSchemeBases;

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
            defaultContextIndex = serializedObject.FindProperty(InputData.EDITOR_DefaultContextIndexField);
            authoredContexts = serializedObject.FindProperty(nameof(authoredContexts));

            controlSchemeBases = serializedObject.FindProperty(nameof(controlSchemeBases));

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

            PopulateControlSchemeBases();
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

        private void PopulateControlSchemeBases()
        {
            Dictionary<string, ControlSchemeBasisSpec> schemeToSpec = new();
            for (int i = 0; i < controlSchemeBases.arraySize; i++)
            {
                if (controlSchemeBases.GetArrayElementAtIndex(i).boxedValue is ControlSchemeBasis basis)
                    schemeToSpec[basis.ControlSchemeName] = basis.Basis;
            }

            controlSchemeBases.ClearArray();

            InputActionAsset asset = ((InputData)target).InputActionAsset;
            string[] enumValues = asset == null ? Array.Empty<string>() : asset.controlSchemes.Select(controlScheme => controlScheme.name).ToArray();
            int index = 0;
            foreach (string scheme in enumValues)
            {
                schemeToSpec.TryGetValue(scheme, out ControlSchemeBasisSpec basisSpec);
                controlSchemeBases.InsertArrayElementAtIndex(index);
                controlSchemeBases.GetArrayElementAtIndex(index).boxedValue = new ControlSchemeBasis(scheme, basisSpec);
                index++;
            }

            // Applied immediately, since the next Update() would discard it. Without undo, because this
            // list mirrors the action asset rather than being a user edit.
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Whether the mirrored list no longer matches the input action asset's control schemes.</summary>
        private bool ControlSchemeBasesAreStale()
        {
            InputActionAsset asset = ((InputData)target).InputActionAsset;
            int schemeCount = asset == null ? 0 : asset.controlSchemes.Count;

            if (controlSchemeBases.arraySize != schemeCount)
            {
                return true;
            }

            for (int i = 0; i < schemeCount; i++)
            {
                if (controlSchemeBases.GetArrayElementAtIndex(i).boxedValue is not ControlSchemeBasis basis ||
                    basis.ControlSchemeName != asset.controlSchemes[i].name)
                {
                    return true;
                }
            }

            return false;
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

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (ControlSchemeBasesAreStale())
            {
                PopulateControlSchemeBases();
            }

            DrawHeader("Input Action Asset");
            EditorGUILayout.PropertyField(inputActionAsset);
            if (inputActionAsset.objectReferenceValue == null)
            {
                DrawWarning("An Input Action Asset is required. Nothing can be generated without one.");
            }

            EditorInspectorUtility.DrawHorizontalLine();

            DrawHeader("Custom Setups");
            DrawSpecialNote("Layouts, bindings and interactions registered with the input system before any player is set up.");
            EditorGUILayout.PropertyField(customLayouts);
            EditorGUILayout.PropertyField(customBindings);
            EditorGUILayout.PropertyField(customInteractions);

            EditorInspectorUtility.DrawHorizontalLine();

            DrawHeader("Initialization");
            EditorGUILayout.PropertyField(initializationMode);

            EditorInspectorUtility.DrawHorizontalLine();

            DrawHeader("Input Contexts");
            DrawDefaultContextPopup();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(authoredContexts);

            EditorInspectorUtility.DrawHorizontalLine();

            DrawHeader("Control Scheme Device Families");
            int length = controlSchemeBases.arraySize;
            if (length == 0)
            {
                DrawSpecialNote("No Control Schemes are defined in your Input Action Asset.");
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    SerializedProperty basisProperty = controlSchemeBases.GetArrayElementAtIndex(i);
                    if (basisProperty.boxedValue is not ControlSchemeBasis basis)
                        continue;

                    if (string.IsNullOrEmpty(basis.ControlSchemeName))
                        continue;

                    SerializedProperty specProperty = basisProperty.FindPropertyRelative(nameof(basis.Basis).ToLower());
                    specProperty.enumValueIndex = (int)(ControlSchemeBasisSpec)EditorGUILayout.EnumPopup(basis.ControlSchemeName, (ControlSchemeBasisSpec)specProperty.enumValueIndex);
                }
            }

            EditorInspectorUtility.DrawHorizontalLine();

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

            EditorInspectorUtility.DrawHorizontalLine();

            DrawHeader("Event System");
            EditorGUILayout.PropertyField(moveRepeatDelay);
            EditorGUILayout.PropertyField(moveRepeatRate);
            EditorGUILayout.PropertyField(deselectOnBackgroundClick);
            EditorGUILayout.PropertyField(pointerBehavior);
            EditorGUILayout.PropertyField(cursorLockBehavior);

            EditorGUILayout.PropertyField(point);
            EditorGUILayout.PropertyField(leftClick);
            EditorGUILayout.PropertyField(middleClick);
            EditorGUILayout.PropertyField(rightClick);
            EditorGUILayout.PropertyField(scrollWheel);
            EditorGUILayout.PropertyField(move);
            EditorGUILayout.PropertyField(submit);
            EditorGUILayout.PropertyField(cancel);
            EditorGUILayout.PropertyField(trackedDevicePosition);
            EditorGUILayout.PropertyField(trackedDeviceOrientation);

            serializedObject.ApplyModifiedProperties();
        }
    }
}