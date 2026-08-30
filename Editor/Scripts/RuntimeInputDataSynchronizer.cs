using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Data;
using UnityEditor;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor
{
    /// <summary>
    /// Keeps the serialized data on <see cref="RuntimeInputData"/> in sync with the input action asset and the
    /// offline (editor-only) input data. This replaces what used to be generated as C# into the RuntimeInputData
    /// and BindingChanger partial classes - it is plain data, so it lives in the asset rather than in code.
    /// </summary>
    internal static class RuntimeInputDataSynchronizer
    {
        internal static void Synchronize(OfflineInputData offlineInputData)
        {
            RuntimeInputData runtimeInputData = offlineInputData.RuntimeInputData;
            if (runtimeInputData == null)
            {
                return;
            }

            SerializedObject serializedObject = new(runtimeInputData);

            CopyStringArray(serializedObject.FindProperty(RuntimeInputData.EDITOR_BindingExcludedPathsField), offlineInputData.BindingExcludedPaths);
            CopyStringArray(serializedObject.FindProperty(RuntimeInputData.EDITOR_BindingCancelPathsField), offlineInputData.BindingCancelPaths);
            SyncControlSchemeBindingData(serializedObject, runtimeInputData.InputActionAsset);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(runtimeInputData);
            AssetDatabase.SaveAssetIfDirty(runtimeInputData);
        }

        private static void CopyStringArray(SerializedProperty arrayProperty, string[] source)
        {
            source ??= new string[0];
            arrayProperty.arraySize = source.Length;
            for (int i = 0; i < source.Length; i++)
            {
                arrayProperty.GetArrayElementAtIndex(i).stringValue = source[i];
            }
        }

        /// <summary>
        /// Rebuild the control scheme entry list from the asset's control schemes, preserving any BindingData
        /// the user has already assigned to a control scheme of the same name.
        /// </summary>
        private static void SyncControlSchemeBindingData(SerializedObject serializedObject, InputActionAsset asset)
        {
            SerializedProperty entries = serializedObject.FindProperty(RuntimeInputData.EDITOR_ControlSchemeBindingDataField);

            Dictionary<string, BindingData> existingByName = new();
            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                string name = entry.FindPropertyRelative(ControlSchemeBindingDataEntry.EDITOR_ControlSchemeNameField).stringValue;
                if (!string.IsNullOrEmpty(name))
                {
                    existingByName[name] = entry.FindPropertyRelative(ControlSchemeBindingDataEntry.EDITOR_BindingDataField).objectReferenceValue as BindingData;
                }
            }

            entries.arraySize = asset == null ? 0 : asset.controlSchemes.Count;
            if (asset == null)
            {
                return;
            }

            for (int i = 0; i < asset.controlSchemes.Count; i++)
            {
                string controlSchemeName = asset.controlSchemes[i].name;
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative(ControlSchemeBindingDataEntry.EDITOR_ControlSchemeNameField).stringValue = controlSchemeName;
                entry.FindPropertyRelative(ControlSchemeBindingDataEntry.EDITOR_BindingDataField).objectReferenceValue =
                    existingByName.TryGetValue(controlSchemeName, out BindingData bindingData) ? bindingData : null;
            }
        }
    }
}
