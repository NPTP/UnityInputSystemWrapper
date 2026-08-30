using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Editor.Utilities;
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
            WarnAboutUnknownMapNames(offlineInputData, runtimeInputData.InputActionAsset);
            serializedObject.FindProperty(RuntimeInputData.EDITOR_DefaultContextIndexField).intValue = offlineInputData.DefaultContextIndex;
            serializedObject.FindProperty(RuntimeInputData.EDITOR_LoadAllBindingOverridesOnInitializeField).boolValue = offlineInputData.LoadAllBindingOverridesOnInitialize;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(runtimeInputData);
            AssetDatabase.SaveAssetIfDirty(runtimeInputData);
            Generation.GenerationReport.Record($"{AssetDatabase.GetAssetPath(runtimeInputData)} (synchronized)");
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

        /// <summary>
        /// A context that names an action map the asset does not have enables nothing at runtime, which
        /// looks exactly like input being broken. Usually it means a map was renamed in the asset without
        /// updating the contexts that referenced it.
        /// </summary>
        private static void WarnAboutUnknownMapNames(OfflineInputData offlineInputData, InputActionAsset asset)
        {
            if (offlineInputData.InputContexts == null || asset == null)
            {
                return;
            }

            List<string> mapNames = new();
            foreach (InputActionMap map in asset.actionMaps)
            {
                mapNames.Add(map.name);
            }

            foreach (InputContextInfo contextInfo in offlineInputData.InputContexts)
            {
                if (contextInfo.ActiveMaps == null)
                {
                    continue;
                }

                foreach (string activeMap in contextInfo.ActiveMaps)
                {
                    if (!mapNames.Contains(activeMap))
                    {
                        ISWDebug.LogWarning($"Input context '{contextInfo.Name}' lists active map '{activeMap}', which does not exist in " +
                                            $"{asset.name}. No maps will be enabled for it. Available maps: {string.Join(", ", mapNames)}.");
                    }
                }
            }

            foreach (InputContextInfo contextInfo in offlineInputData.InputContexts)
            {
                if (contextInfo.ActiveMaps == null || contextInfo.ActiveMaps.Length == 0)
                {
                    ISWDebug.LogWarning($"Input context '{contextInfo.Name}' has no active maps, so no input will be received while it is set.");
                }
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
                    existingByName.GetValueOrDefault(controlSchemeName);

                entry.FindPropertyRelative(ControlSchemeDefinition.EDITOR_BasisField).enumValueIndex = (int)GetBasis(offlineInputData, controlSchemeName);
            }
        }

        private static ControlSchemeBasisSpec GetBasis(OfflineInputData offlineInputData, string controlSchemeName)
        {
            if (offlineInputData.ControlSchemeBases == null)
            {
                return ControlSchemeBasisSpec.Undefined;
            }

            foreach (ControlSchemeBasis controlSchemeBasis in offlineInputData.ControlSchemeBases)
            {
                if (controlSchemeBasis.ControlSchemeName == controlSchemeName)
                {
                    return controlSchemeBasis.Basis;
                }
            }

            return ControlSchemeBasisSpec.Undefined;
        }
    }
}
