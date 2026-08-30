using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Editor.Utilities;
using NPTP.InputSystemWrapper.Enums;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor
{
    /// <summary>
    /// Bakes the generated fields on <see cref="InputData"/> from the input action asset and the authored
    /// editor-only fields. InputActionReferences become the action IDs a player's cloned asset can resolve.
    /// </summary>
    internal static class InputDataSynchronizer
    {
        /// <summary>
        /// Every device layout the input system registers directly under InputDevice, against the family
        /// it stands for. Anything else is derived from one of these, so matching by inheritance puts
        /// every device in a family.
        /// </summary>
        private static readonly (string Layout, ControlSchemeDeviceFamilies Family)[] deviceFamilies =
        {
            ("Pointer", ControlSchemeDeviceFamilies.UsesPointer),
            ("Gamepad", ControlSchemeDeviceFamilies.UsesGamepad),
            ("Keyboard", ControlSchemeDeviceFamilies.UsesKeyboard),
            ("Joystick", ControlSchemeDeviceFamilies.UsesJoystick),
            ("Sensor", ControlSchemeDeviceFamilies.UsesSensor),
            ("TrackedDevice", ControlSchemeDeviceFamilies.UsesTrackedDevice)
        };

        internal static void Synchronize(InputData inputData)
        {
            if (inputData == null)
            {
                return;
            }

            SerializedObject serializedObject = new(inputData);

            SyncControlSchemes(serializedObject, inputData.InputActionAsset);
            SyncDeviceBindingData(serializedObject, inputData.InputActionAsset);
            SyncEventSystemOptions(serializedObject, inputData);
            SyncInputContexts(serializedObject, inputData);
            WarnAboutUnknownMapNames(inputData, inputData.InputActionAsset);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(inputData);
            AssetDatabase.SaveAssetIfDirty(inputData);
            Generation.GenerationReport.Record($"{AssetDatabase.GetAssetPath(inputData)} (synchronized)");
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

        private static void SyncEventSystemOptions(SerializedObject serializedObject, InputData inputData)
        {
            SerializedProperty options = serializedObject.FindProperty(InputData.EDITOR_EventSystemOptionsField);
            options.FindPropertyRelative(EventSystemOptions.EDITOR_MoveRepeatDelayField).floatValue = inputData.MoveRepeatDelay;
            options.FindPropertyRelative(EventSystemOptions.EDITOR_MoveRepeatRateField).floatValue = inputData.MoveRepeatRate;
            options.FindPropertyRelative(EventSystemOptions.EDITOR_DeselectOnBackgroundClickField).boolValue = inputData.DeselectOnBackgroundClick;
            options.FindPropertyRelative(EventSystemOptions.EDITOR_PointerBehaviorField).enumValueIndex = (int)inputData.PointerBehavior;
            options.FindPropertyRelative(EventSystemOptions.EDITOR_CursorLockBehaviorField).enumValueIndex = (int)inputData.CursorLockBehavior;

            List<(EventSystemActionType, InputActionReference)> defaults = new()
            {
                (EventSystemActionType.Point, inputData.Point),
                (EventSystemActionType.LeftClick, inputData.LeftClick),
                (EventSystemActionType.MiddleClick, inputData.MiddleClick),
                (EventSystemActionType.RightClick, inputData.RightClick),
                (EventSystemActionType.ScrollWheel, inputData.ScrollWheel),
                (EventSystemActionType.Move, inputData.Move),
                (EventSystemActionType.Submit, inputData.Submit),
                (EventSystemActionType.Cancel, inputData.Cancel),
                (EventSystemActionType.TrackedDevicePosition, inputData.TrackedDevicePosition),
                (EventSystemActionType.TrackedDeviceOrientation, inputData.TrackedDeviceOrientation)
            };

            WriteActionBindings(options.FindPropertyRelative(EventSystemOptions.EDITOR_DefaultActionsField), defaults);
        }

        private static void SyncInputContexts(SerializedObject serializedObject, InputData inputData)
        {
            InputContextInfo[] contextInfos = inputData.AuthoredContexts ?? new InputContextInfo[0];
            SerializedProperty contexts = serializedObject.FindProperty(InputData.EDITOR_ContextDefinitionsField);
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
        private static void WarnAboutUnknownMapNames(InputData inputData, InputActionAsset asset)
        {
            if (inputData.AuthoredContexts == null || asset == null)
            {
                return;
            }

            List<string> mapNames = new();
            foreach (InputActionMap map in asset.actionMaps)
            {
                mapNames.Add(map.name);
            }

            foreach (InputContextInfo contextInfo in inputData.AuthoredContexts)
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

            foreach (InputContextInfo contextInfo in inputData.AuthoredContexts)
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
        /// Rebuild the control scheme list from the asset's control schemes, baking in the device families
        /// each one uses.
        /// </summary>
        private static void SyncControlSchemes(SerializedObject serializedObject, InputActionAsset asset)
        {
            SerializedProperty entries = serializedObject.FindProperty(InputData.EDITOR_ControlSchemesField);

            entries.arraySize = asset == null ? 0 : asset.controlSchemes.Count;
            if (asset == null)
            {
                return;
            }

            for (int i = 0; i < asset.controlSchemes.Count; i++)
            {
                InputControlScheme controlScheme = asset.controlSchemes[i];
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative(ControlSchemeDefinition.EDITOR_ControlSchemeNameField).stringValue = controlScheme.name;
                // intValue, not enumValueIndex: this is a flags enum, so the value is a bit mask rather
                // than a position in the enum's declaration.
                entry.FindPropertyRelative(ControlSchemeDefinition.EDITOR_DeviceFamiliesField).intValue = (int)GetDeviceFamilies(controlScheme);
            }
        }

        /// <summary>
        /// Rebuild the binding data list from every device used by any control scheme, deduplicated: a
        /// device shared by several schemes gets one set of binding data, not one per scheme. Assets the
        /// user has already assigned to a device are kept.
        /// </summary>
        private static void SyncDeviceBindingData(SerializedObject serializedObject, InputActionAsset asset)
        {
            SerializedProperty entries = serializedObject.FindProperty(InputData.EDITOR_DeviceBindingDataField);

            Dictionary<string, BindingData> existingByDevice = new();
            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                string deviceLayoutName = entry.FindPropertyRelative(DeviceBindingData.EDITOR_DeviceLayoutNameField).stringValue;
                if (!string.IsNullOrEmpty(deviceLayoutName))
                {
                    existingByDevice[deviceLayoutName] = entry.FindPropertyRelative(DeviceBindingData.EDITOR_BindingDataField).objectReferenceValue as BindingData;
                }
            }

            List<string> deviceLayouts = GetAllDeviceLayouts(asset);
            entries.arraySize = deviceLayouts.Count;

            for (int i = 0; i < deviceLayouts.Count; i++)
            {
                string deviceLayoutName = deviceLayouts[i];
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative(DeviceBindingData.EDITOR_DeviceLayoutNameField).stringValue = deviceLayoutName;
                entry.FindPropertyRelative(DeviceBindingData.EDITOR_BindingDataField).objectReferenceValue =
                    existingByDevice.GetValueOrDefault(deviceLayoutName) ?? GetOrCreateBindingData(deviceLayoutName);
            }
        }

        /// <summary>
        /// Every distinct device layout named by any control scheme, in the order first encountered so the
        /// list stays stable between runs.
        /// </summary>
        private static List<string> GetAllDeviceLayouts(InputActionAsset asset)
        {
            List<string> deviceLayouts = new();
            if (asset == null)
            {
                return deviceLayouts;
            }

            foreach (InputControlScheme controlScheme in asset.controlSchemes)
            {
                foreach (string deviceLayout in Generation.DeviceControlPathCatalog.GetRequiredDeviceLayouts(controlScheme))
                {
                    if (!deviceLayouts.Contains(deviceLayout)) deviceLayouts.Add(deviceLayout);
                }
            }

            return deviceLayouts;
        }

        /// <summary>
        /// The binding data asset for a device that has none assigned yet. An existing asset named after
        /// the device is reused; otherwise one is created, populated with every control that device can
        /// produce so it is useful before anyone edits it.
        /// </summary>
        private static BindingData GetOrCreateBindingData(string deviceLayoutName)
        {
            string assetName = deviceLayoutName.AsType();

            if (Generation.ProjectAssets.TryFindProjectAsset(assetName, out BindingData existing))
            {
                return existing;
            }

            BindingData created = ScriptableObject.CreateInstance<BindingData>();
            foreach (KeyValuePair<string, string> pathToDisplayName in Generation.DeviceControlPathCatalog.GetControlPaths(deviceLayoutName))
            {
                created.EDITOR_AddBinding(pathToDisplayName.Key, pathToDisplayName.Value);
            }

            string assetPath = $"{Generation.ProjectAssets.GetOrCreateBindingDataFolder()}/{assetName}.asset";
            AssetDatabase.CreateAsset(created, assetPath);
            Generation.GenerationReport.Record($"{assetPath} (created for device '{deviceLayoutName}')");

            return created;
        }

        /// <summary>
        /// Which device families a control scheme uses, taken from the devices it requires. A scheme can
        /// use several at once, e.g. one requiring a keyboard and a mouse.
        /// </summary>
        private static ControlSchemeDeviceFamilies GetDeviceFamilies(InputControlScheme controlScheme)
        {
            ControlSchemeDeviceFamilies used = ControlSchemeDeviceFamilies.Undefined;

            foreach (string layout in Generation.DeviceControlPathCatalog.GetRequiredDeviceLayouts(controlScheme))
            {
                foreach ((string familyLayout, ControlSchemeDeviceFamilies family) in deviceFamilies)
                {
                    if (InputSystem.IsFirstLayoutBasedOnSecond(layout, familyLayout)) used |= family;
                }
            }

            return used;
        }
    }
}
