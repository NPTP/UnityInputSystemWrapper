using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Editor.CustomEditors
{
    [CustomEditor(typeof(OfflineInputData))]
    internal class OfflineInputDataEditor : UnityEditor.Editor
    {
        private SerializedProperty assetsPathToPackage;
        private SerializedProperty runtimeInputData;
        private SerializedProperty actionsTemplateFile;
        private SerializedProperty initializationMode;
        private SerializedProperty enableMultiplayer;
        private SerializedProperty maxPlayers;
        private SerializedProperty defaultContext;
        private SerializedProperty inputContexts;
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
            actionsTemplateFile = serializedObject.FindProperty(nameof(actionsTemplateFile));
            initializationMode = serializedObject.FindProperty(nameof(initializationMode));
            defaultContext = serializedObject.FindProperty(nameof(defaultContext));
            inputContexts = serializedObject.FindProperty(nameof(inputContexts));
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
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(assetsPathToPackage);
            EditorGUILayout.PropertyField(runtimeInputData);
            EditorGUILayout.PropertyField(actionsTemplateFile);
            EditorGUILayout.PropertyField(initializationMode);
            
            EditorInspectorUtility.DrawHorizontalLine();

            // TODO (multiplayer): Remove these warning labels when MP support is ready.
            EditorGUILayout.LabelField("Warning! Multiplayer support is currently incomplete. Enable at your own risk.",
                new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.BoldAndItalic, fontSize = 12 });
            EditorGUILayout.PropertyField(enableMultiplayer);
            if (enableMultiplayer.boolValue)
            {
                EditorGUILayout.PropertyField(maxPlayers);
                maxPlayers.intValue = Mathf.Clamp(maxPlayers.intValue, 2, OfflineInputData.MAX_PLAYERS_LIMIT);
            }

            EditorInspectorUtility.DrawHorizontalLine();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(defaultContext);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(inputContexts);
            
            EditorInspectorUtility.DrawHorizontalLine();
            
            EditorGUILayout.LabelField("Excluded Path format: \"<Device>/path\"");
            EditorGUILayout.PropertyField(bindingExcludedPaths);
            EditorGUILayout.LabelField("Cancel Path format: \"/Device/path\"");
            EditorGUILayout.PropertyField(bindingCancelPaths);

            EditorInspectorUtility.DrawHorizontalLine();
            
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