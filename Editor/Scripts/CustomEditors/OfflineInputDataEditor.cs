using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Editor.CustomEditors
{
    [CustomEditor(typeof(OfflineInputData))]
    internal class OfflineInputDataEditor : UnityEditor.Editor
    {
        private SerializedProperty enableMultiplayer;
        private SerializedProperty actionReferencesInActionWrappers;
        private SerializedProperty maxPlayers;
        private SerializedProperty runtimeInputData;
        private SerializedProperty actionsTemplateFile;
        private SerializedProperty defaultContext;
        private SerializedProperty inputContexts;
        private SerializedProperty bindingExcludedPaths;
        private SerializedProperty bindingCancelPaths;
        
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
            enableMultiplayer = serializedObject.FindProperty(nameof(enableMultiplayer));
            maxPlayers = serializedObject.FindProperty(nameof(maxPlayers));
            runtimeInputData = serializedObject.FindProperty(nameof(runtimeInputData));
            actionsTemplateFile = serializedObject.FindProperty(nameof(actionsTemplateFile));
            actionReferencesInActionWrappers = serializedObject.FindProperty(nameof(actionReferencesInActionWrappers));
            defaultContext = serializedObject.FindProperty(nameof(defaultContext));
            inputContexts = serializedObject.FindProperty(nameof(inputContexts));
            bindingExcludedPaths = serializedObject.FindProperty(nameof(bindingExcludedPaths));
            bindingCancelPaths = serializedObject.FindProperty(nameof(bindingCancelPaths));
            
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
            EditorGUILayout.LabelField("Warning! Multiplayer support is currently incomplete.");
            EditorGUILayout.PropertyField(enableMultiplayer);
            if (enableMultiplayer.boolValue)
            {
                EditorGUILayout.PropertyField(maxPlayers);
                maxPlayers.intValue = Mathf.Clamp(maxPlayers.intValue, 2, OfflineInputData.MAX_PLAYERS_LIMIT);
            }
            
            EditorInspectorUtility.DrawHorizontalLine();
            
            EditorGUILayout.PropertyField(runtimeInputData);
            EditorGUILayout.PropertyField(actionsTemplateFile);
            EditorGUILayout.PropertyField(actionReferencesInActionWrappers);
            
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