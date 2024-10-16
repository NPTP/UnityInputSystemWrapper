using NPTP.InputSystemWrapper.Components;
using NPTP.InputSystemWrapper.Utilities.Editor;
using UnityEditor;

namespace NPTP.InputSystemWrapper.Editor.CustomEditors
{
    // [CustomEditor(typeof(InputActionUpdater))]
    internal class InputActionUpdaterEditor : UnityEditor.Editor
    {
        private SerializedProperty setSpriteEvent;
        private SerializedProperty respondToAllPlayers;
        private SerializedProperty associatedPlayer;

        private void OnEnable()
        {
            setSpriteEvent = serializedObject.FindProperty(nameof(setSpriteEvent));
            respondToAllPlayers = serializedObject.FindProperty(nameof(respondToAllPlayers));
            associatedPlayer = serializedObject.FindProperty(nameof(associatedPlayer));
        }

        public override void OnInspectorGUI()
        {
            EditorInspectorUtility.ShowScriptInspector((InputActionUpdater)target);
            
            EditorGUILayout.PropertyField(setSpriteEvent);
            EditorGUILayout.PropertyField(respondToAllPlayers);
            if (!respondToAllPlayers.boolValue)
            {
                EditorGUILayout.PropertyField(associatedPlayer);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}