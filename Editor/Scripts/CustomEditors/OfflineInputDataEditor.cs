using InputSystemWrapper.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityInputSystemWrapper.Data;

namespace UnityInputSystemWrapper.Editor.CustomEditors
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

        private void OnEnable()
        {
            enableMultiplayer = serializedObject.FindProperty(nameof(enableMultiplayer));
            maxPlayers = serializedObject.FindProperty(nameof(maxPlayers));
            runtimeInputData = serializedObject.FindProperty(nameof(runtimeInputData));
            mapActionsTemplateFile = serializedObject.FindProperty(nameof(mapActionsTemplateFile));
            mapCacheTemplateFile = serializedObject.FindProperty(nameof(mapCacheTemplateFile));
            defaultContext = serializedObject.FindProperty(nameof(defaultContext));
            inputContexts = serializedObject.FindProperty(nameof(inputContexts));
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

            EditorGUILayout.PropertyField(defaultContext);
            EditorGUILayout.PropertyField(inputContexts);

            serializedObject.ApplyModifiedProperties();
        }
    }
}