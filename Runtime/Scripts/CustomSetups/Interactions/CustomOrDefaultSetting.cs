using System;
using UnityEditor;
using UnityEngine;

namespace NPTP.InputSystemWrapper.InputDevices.Interactions
{
    /// <summary>
    /// Based very closely on UnityEngine.InputSystem.Editor.CustomOrDefaultSetting, but that's an internal
    /// struct so we couldn't access it in our own scripts.
    /// </summary>
    internal struct CustomOrDefaultSetting
    {
        private Func<float> m_GetValue;
        private Action<float> m_SetValue;
        private Func<float> m_GetDefaultValue;
        private bool m_UseDefaultValue;
        private float m_DefaultInitializedValue;
        private GUIContent m_ToggleLabel;
        private GUIContent m_ValueLabel;
        
        public void Initialize(string label, string tooltip, string defaultName, Func<float> getValue,
            Action<float> setValue, Func<float> getDefaultValue, bool defaultComesFromInputSettings = true,
            float defaultInitializedValue = default)
        {
            m_GetValue = getValue;
            m_SetValue = setValue;
            m_GetDefaultValue = getDefaultValue;
            m_ToggleLabel = EditorGUIUtility.TrTextContent("Default",
                defaultComesFromInputSettings
                    ? $"If enabled, the default {label.ToLower()} configured globally in the input settings is used. See Edit >> Project Settings... >> Input (NEW)."
                    : "If enabled, the default value is used.");
            m_ValueLabel = EditorGUIUtility.TrTextContent(label, tooltip);
            m_DefaultInitializedValue = defaultInitializedValue;
            m_UseDefaultValue = Mathf.Approximately(getValue(), defaultInitializedValue);
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(m_UseDefaultValue);

            var value = m_GetValue();

            if (m_UseDefaultValue)
                value = m_GetDefaultValue();

            // If previous value was an epsilon away from default value, it most likely means that value was set by our own code down in this method.
            // Revert it back to default to show a nice readable value in UI.
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((value - float.Epsilon) == m_DefaultInitializedValue)
                value = m_DefaultInitializedValue;

            var newValue = EditorGUILayout.FloatField(m_ValueLabel, value, GUILayout.ExpandWidth(false));
            if (!m_UseDefaultValue)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (newValue == m_DefaultInitializedValue)
                    // If user sets a value that is equal to default initialized, change value slightly so it doesn't pass potential default checks.
                    m_SetValue(newValue + float.Epsilon);
                else
                    m_SetValue(newValue);
            }

            EditorGUI.EndDisabledGroup();

            var newUseDefault = GUILayout.Toggle(m_UseDefaultValue, m_ToggleLabel, GUILayout.ExpandWidth(false));
            if (newUseDefault != m_UseDefaultValue)
            {
                if (!newUseDefault)
                    m_SetValue(m_GetDefaultValue());
                else
                    m_SetValue(m_DefaultInitializedValue);
            }

            m_UseDefaultValue = newUseDefault;
            EditorGUILayout.EndHorizontal();
        }
    }
}