using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Editor.Utilities;
using NPTP.InputSystemWrapper.Enums;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Editor.CustomEditors
{
    [CustomEditor(typeof(OfflineInputData))]
    internal class OfflineInputDataEditor : UnityEditor.Editor
    {
        private GUIStyle HeaderStyle => new(EditorStyles.label) { fontStyle = FontStyle.Bold, fontSize = 14 };
        private GUIStyle WarningStyle => new(EditorStyles.label) { fontStyle = FontStyle.Italic, fontSize = 12, normal = new GUIStyleState {textColor = Color.yellow}};
        private GUIStyle SpecialNoteStyle => new(EditorStyles.label) { fontStyle = FontStyle.Italic, fontSize = 10 };
        
        private SerializedProperty assetsPathToPackage;
        private SerializedProperty runtimeInputData;
        private SerializedProperty mainInputScriptFile;
        private SerializedProperty actionsTemplateFile;
        private SerializedProperty initializationMode;
        private SerializedProperty enableMultiplayer;
        private SerializedProperty maxPlayers;
        private SerializedProperty defaultContext;
        private SerializedProperty inputContexts;
        
        private SerializedProperty controlSchemeBases;
        
        private SerializedProperty loadAllBindingOverridesOnInitialize;
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
            assetsPathToPackage = serializedObject.FindProperty(nameof(assetsPathToPackage));
            enableMultiplayer = serializedObject.FindProperty(nameof(enableMultiplayer));
            maxPlayers = serializedObject.FindProperty(nameof(maxPlayers));
            runtimeInputData = serializedObject.FindProperty(nameof(runtimeInputData));
            mainInputScriptFile = serializedObject.FindProperty(nameof(mainInputScriptFile));
            actionsTemplateFile = serializedObject.FindProperty(nameof(actionsTemplateFile));
            initializationMode = serializedObject.FindProperty(nameof(initializationMode));
            defaultContext = serializedObject.FindProperty(nameof(defaultContext));
            inputContexts = serializedObject.FindProperty(nameof(inputContexts));
            
            controlSchemeBases = serializedObject.FindProperty(nameof(controlSchemeBases));

            loadAllBindingOverridesOnInitialize = serializedObject.FindProperty(nameof(loadAllBindingOverridesOnInitialize));
            bindingExcludedPaths = serializedObject.FindProperty(nameof(bindingExcludedPaths));
            bindingCancelPaths = serializedObject.FindProperty(nameof(bindingCancelPaths));
            
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

        private void PopulateControlSchemeBases()
        {
            Dictionary<ControlScheme, ControlSchemeBasis.BasisSpec> schemeToSpec = new();
            for (int i = 0; i < controlSchemeBases.arraySize; i++)
            {
                if (controlSchemeBases.GetArrayElementAtIndex(i).boxedValue is ControlSchemeBasis basis)
                    schemeToSpec[basis.ControlScheme] = basis.Basis;
            }
            
            controlSchemeBases.ClearArray();

            Array enumValues = Enum.GetValues(typeof(ControlScheme));
            int index = 0;
            foreach (ControlScheme scheme in enumValues)
            {
                schemeToSpec.TryGetValue(scheme, out ControlSchemeBasis.BasisSpec basisSpec);
                controlSchemeBases.InsertArrayElementAtIndex(index);
                controlSchemeBases.GetArrayElementAtIndex(index).boxedValue = new ControlSchemeBasis(scheme, basisSpec);
                index++;
            }
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
            EditorGUILayout.PropertyField(assetsPathToPackage);
            EditorGUILayout.PropertyField(runtimeInputData);
            EditorGUILayout.PropertyField(mainInputScriptFile);
            EditorGUILayout.PropertyField(actionsTemplateFile);
            EditorGUILayout.PropertyField(initializationMode);
            
            EditorInspectorUtility.DrawHorizontalLine();

            DrawHeader("Multiplayer");

            if (enableMultiplayer.boolValue)
            {
                // TODO (multiplayer): Remove this warning when MP support is ready.
                DrawWarning("Multiplayer support is currently incomplete. Enable at your own risk.");
            } 
            EditorGUILayout.PropertyField(enableMultiplayer);
            if (enableMultiplayer.boolValue)
            {
                EditorGUILayout.PropertyField(maxPlayers);
                maxPlayers.intValue = Mathf.Clamp(maxPlayers.intValue, 2, OfflineInputData.MAX_PLAYERS_LIMIT);
            }

            EditorInspectorUtility.DrawHorizontalLine();

            DrawHeader("Input Contexts");
            EditorGUILayout.PropertyField(defaultContext);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(inputContexts);
            
            EditorInspectorUtility.DrawHorizontalLine();
            
            DrawHeader("Control Schemes");
            int length = controlSchemeBases.arraySize;
            for (int i = 0; i < length; i++)
            {
                SerializedProperty basisProperty = controlSchemeBases.GetArrayElementAtIndex(i);
                if (basisProperty.boxedValue is not ControlSchemeBasis basis)
                    continue;

                SerializedProperty specProperty = basisProperty.FindPropertyRelative(nameof(basis.Basis).ToLower());
                specProperty.enumValueIndex = (int)(ControlSchemeBasis.BasisSpec)EditorGUILayout.EnumPopup(basis.ControlScheme.ToInputAssetName(), (ControlSchemeBasis.BasisSpec)specProperty.enumValueIndex);
            }

            EditorInspectorUtility.DrawHorizontalLine();
            
            DrawHeader("Bindings");
            EditorGUILayout.PropertyField(loadAllBindingOverridesOnInitialize);
            EditorGUILayout.Space();
            DrawSpecialNote("Excluded Path format: \"<Device>/path\"");
            EditorGUILayout.PropertyField(bindingExcludedPaths);
            DrawSpecialNote("Cancel Path format: \"/Device/path\"");
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