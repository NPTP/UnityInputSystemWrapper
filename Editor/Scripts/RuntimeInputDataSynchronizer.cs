using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
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
            SyncControlSchemes(serializedObject, offlineInputData, runtimeInputData.InputActionAsset);
            SyncEventSystemOptions(serializedObject, offlineInputData);
            SyncInputContexts(serializedObject, offlineInputData);
            serializedObject.FindProperty(RuntimeInputData.EDITOR_DefaultContextIndexField).intValue = (int)offlineInputData.DefaultContext;
            serializedObject.FindProperty(RuntimeInputData.EDITOR_LoadAllBindingOverridesOnInitializeField).boolValue = offlineInputData.LoadAllBindingOverridesOnInitialize;

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

        private static void SyncEventSystemOptions(SerializedObject serializedObject, OfflineInputData offlineInputData)
        {
            SerializedProperty options = serializedObject.FindProperty(RuntimeInputData.EDITOR_EventSystemOptionsField);
            options.FindPropertyRelative(EventSystemOptions.EDITOR_MoveRepeatDelayField).floatValue = offlineInputData.MoveRepeatDelay;
            options.FindPropertyRelative(EventSystemOptions.EDITOR_MoveRepeatRateField).floatValue = offlineInputData.MoveRepeatRate;
            options.FindPropertyRelative(EventSystemOptions.EDITOR_DeselectOnBackgroundClickField).boolValue = offlineInputData.DeselectOnBackgroundClick;
            options.FindPropertyRelative(EventSystemOptions.EDITOR_PointerBehaviorField).enumValueIndex = (int)offlineInputData.PointerBehavior;
            options.FindPropertyRelative(EventSystemOptions.EDITOR_CursorLockBehaviorField).enumValueIndex = (int)offlineInputData.CursorLockBehavior;

            List<(EventSystemActionType, InputActionReference)> defaults = new()
            {
                (EventSystemActionType.Point, offlineInputData.Point),
                (EventSystemActionType.LeftClick, offlineInputData.LeftClick),
                (EventSystemActionType.MiddleClick, offlineInputData.MiddleClick),
                (EventSystemActionType.RightClick, offlineInputData.RightClick),
                (EventSystemActionType.ScrollWheel, offlineInputData.ScrollWheel),
                (EventSystemActionType.Move, offlineInputData.Move),
                (EventSystemActionType.Submit, offlineInputData.Submit),
                (EventSystemActionType.Cancel, offlineInputData.Cancel),
                (EventSystemActionType.TrackedDevicePosition, offlineInputData.TrackedDevicePosition),
                (EventSystemActionType.TrackedDeviceOrientation, offlineInputData.TrackedDeviceOrientation)
            };

            WriteActionBindings(options.FindPropertyRelative(EventSystemOptions.EDITOR_DefaultActionsField), defaults);
        }

        private static void SyncInputContexts(SerializedObject serializedObject, OfflineInputData offlineInputData)
        {
            InputContextInfo[] contextInfos = offlineInputData.InputContexts ?? new InputContextInfo[0];
            SerializedProperty contexts = serializedObject.FindProperty(RuntimeInputData.EDITOR_InputContextsField);
            contexts.arraySize = contextInfos.Length;

            for (int i = 0; i < contextInfos.Length; i++)
            {
                InputContextInfo contextInfo = contextInfos[i];
                SerializedProperty context = contexts.GetArrayElementAtIndex(i);
                context.FindPropertyRelative(InputContextDefinition.EDITOR_NameField).stringValue = contextInfo.Name;
                context.FindPropertyRelative(InputContextDefinition.EDITOR_EnableKeyboardTextInputField).boolValue = contextInfo.EnableKeyboardTextInput;
                CopyStringArray(context.FindPropertyRelative(InputContextDefinition.EDITOR_ActiveMapNamesField), contextInfo.ActiveMaps);

                List<(EventSystemActionType, InputActionReference)> overrides = new();
                foreach (EventSystemActionSpecification spec in contextInfo.EventSystemActionOverrides)
                    overrides.Add((spec.ActionType, spec.ActionReference));

                WriteActionBindings(context.FindPropertyRelative(InputContextDefinition.EDITOR_EventSystemActionOverridesField), overrides);
            }
        }

        private static void WriteActionBindings(SerializedProperty bindings, List<(EventSystemActionType ActionType, InputActionReference Reference)> source)
        {
            bindings.arraySize = source.Count;
            for (int i = 0; i < source.Count; i++)
            {
                SerializedProperty binding = bindings.GetArrayElementAtIndex(i);
                binding.FindPropertyRelative(EventSystemActionBinding.EDITOR_ActionTypeField).enumValueIndex = (int)source[i].ActionType;
                binding.FindPropertyRelative(EventSystemActionBinding.EDITOR_ActionIDField).stringValue =
                    source[i].Reference == null || source[i].Reference.action == null ? string.Empty : source[i].Reference.action.id.ToString();
            }
        }

        /// <summary>
        /// Rebuild the control scheme list from the asset's control schemes, preserving any BindingData
        /// the user has already assigned to a control scheme of the same name, and baking in each scheme's
        /// device basis from the offline data.
        /// </summary>
        private static void SyncControlSchemes(SerializedObject serializedObject, OfflineInputData offlineInputData, InputActionAsset asset)
        {
            SerializedProperty entries = serializedObject.FindProperty(RuntimeInputData.EDITOR_ControlSchemesField);

            Dictionary<string, BindingData> existingByName = new();
            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                string name = entry.FindPropertyRelative(ControlSchemeDefinition.EDITOR_ControlSchemeNameField).stringValue;
                if (!string.IsNullOrEmpty(name))
                {
                    existingByName[name] = entry.FindPropertyRelative(ControlSchemeDefinition.EDITOR_BindingDataField).objectReferenceValue as BindingData;
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
                entry.FindPropertyRelative(ControlSchemeDefinition.EDITOR_ControlSchemeNameField).stringValue = controlSchemeName;
                entry.FindPropertyRelative(ControlSchemeDefinition.EDITOR_BindingDataField).objectReferenceValue =
                    existingByName.TryGetValue(controlSchemeName, out BindingData bindingData) ? bindingData : null;

                ControlSchemeBasis.BasisSpec basis = GetBasis(offlineInputData, controlSchemeName);
                entry.FindPropertyRelative(ControlSchemeDefinition.EDITOR_IsMouseBasedField).boolValue = basis is ControlSchemeBasis.BasisSpec.IsMouseBased;
                entry.FindPropertyRelative(ControlSchemeDefinition.EDITOR_IsGamepadBasedField).boolValue = basis is ControlSchemeBasis.BasisSpec.IsGamepadBased;
            }
        }

        private static ControlSchemeBasis.BasisSpec GetBasis(OfflineInputData offlineInputData, string controlSchemeName)
        {
            if (offlineInputData.ControlSchemeBases == null)
            {
                return ControlSchemeBasis.BasisSpec.Undefined;
            }

            foreach (ControlSchemeBasis controlSchemeBasis in offlineInputData.ControlSchemeBases)
            {
                if (controlSchemeBasis.ControlScheme.ToInputAssetName() == controlSchemeName)
                {
                    return controlSchemeBasis.Basis;
                }
            }

            return ControlSchemeBasis.BasisSpec.Undefined;
        }
    }
}
