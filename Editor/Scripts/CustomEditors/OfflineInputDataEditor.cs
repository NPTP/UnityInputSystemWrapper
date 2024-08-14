using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Editor.CustomEditors
{
    [CustomEditor(typeof(OfflineInputData))]
    public class OfflineInputDataEditor : UnityEditor.Editor
    {
        private SerializedProperty enableMultiplayer;
        private SerializedProperty maxPlayers;
        private SerializedProperty runtimeInputData;
        private SerializedProperty mapActionsTemplateFile;
        private SerializedProperty mapCacheTemplateFile;
        private SerializedProperty defaultContext;
        private SerializedProperty inputContexts;
        
        private SerializedProperty point;
        private SerializedProperty middleClick;
        private SerializedProperty rightClick;
        private SerializedProperty scrollWheel;
        private SerializedProperty move;
        private SerializedProperty submit;
        private SerializedProperty cancel;
        private SerializedProperty trackedDevicePosition;
        private SerializedProperty trackedDeviceOrientation;
        private SerializedProperty leftClick;

        private void OnEnable()
        {
            enableMultiplayer = serializedObject.FindProperty(nameof(enableMultiplayer));
            maxPlayers = serializedObject.FindProperty(nameof(maxPlayers));
            runtimeInputData = serializedObject.FindProperty(nameof(runtimeInputData));
            mapActionsTemplateFile = serializedObject.FindProperty(nameof(mapActionsTemplateFile));
            mapCacheTemplateFile = serializedObject.FindProperty(nameof(mapCacheTemplateFile));
            defaultContext = serializedObject.FindProperty(nameof(defaultContext));
            inputContexts = serializedObject.FindProperty(nameof(inputContexts));
            
            point = serializedObject.FindProperty(nameof(point));
            middleClick = serializedObject.FindProperty(nameof(middleClick));
            rightClick = serializedObject.FindProperty(nameof(rightClick));
            scrollWheel = serializedObject.FindProperty(nameof(scrollWheel));
            move = serializedObject.FindProperty(nameof(move));
            submit = serializedObject.FindProperty(nameof(submit));
            cancel = serializedObject.FindProperty(nameof(cancel));
            trackedDevicePosition = serializedObject.FindProperty(nameof(trackedDevicePosition));
            trackedDeviceOrientation = serializedObject.FindProperty(nameof(trackedDeviceOrientation));
            leftClick = serializedObject.FindProperty(nameof(leftClick));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(enableMultiplayer);
            if (enableMultiplayer.boolValue)
            {
                EditorGUILayout.PropertyField(maxPlayers);
                maxPlayers.intValue = Mathf.Clamp(maxPlayers.intValue, 2, OfflineInputData.MAX_PLAYERS_LIMIT);
            }

            EditorInspectorUtility.DrawHorizontalLine();
            
            EditorGUILayout.PropertyField(runtimeInputData);
            EditorGUILayout.PropertyField(mapActionsTemplateFile);
            EditorGUILayout.PropertyField(mapCacheTemplateFile);
            
            EditorInspectorUtility.DrawHorizontalLine();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(defaultContext);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(inputContexts);
            
            EditorInspectorUtility.DrawHorizontalLine();
            
            EditorGUILayout.PropertyField(point);
            EditorGUILayout.PropertyField(middleClick);
            EditorGUILayout.PropertyField(rightClick);
            EditorGUILayout.PropertyField(scrollWheel);
            EditorGUILayout.PropertyField(move);
            EditorGUILayout.PropertyField(submit);
            EditorGUILayout.PropertyField(cancel);
            EditorGUILayout.PropertyField(trackedDevicePosition);
            EditorGUILayout.PropertyField(trackedDeviceOrientation);
            EditorGUILayout.PropertyField(leftClick);

            serializedObject.ApplyModifiedProperties();
        }
    }
}