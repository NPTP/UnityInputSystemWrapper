using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Generated.Actions;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using Object = UnityEngine.Object;

namespace NPTP.InputSystemWrapper
{
    public sealed partial class InputPlayer
    {
        #region Field & Properties

        /// <summary>
        /// Corresponds to InputUser.onChange, for this player specifically.
        /// </summary>
        public event Action<InputUserChangeInfo> OnInputUserChange;
        
        public event Action<InputPlayer> OnControlSchemeChanged;

        /// <summary>
        /// The input player can be used when enabled, and is ignored when disabled.
        /// </summary>
        public event Action<InputPlayer> OnEnabledOrDisabled;
        
        /// <summary>
        /// Sends the keyboard text character that was just input by this player,
        /// but only if the current InputContext that allows keyboard text input is active.
        /// </summary>
        public event Action<char> OnKeyboardTextInput;

        /// <summary>
        /// Invoked when any device paired to this player has any button pressed,
        /// regardless of which assets/maps/actions are enabled or disabled.
        /// </summary>
        public event AnyButtonPressListener OnAnyButtonPress
        {
            add => AddAnyButtonPressListener(value);
            remove => RemoveAnyButtonPressListener(value);
        }

        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (playerInput == null || enabled == value)
                {
                    return;
                }
            
                enabled = value;
                playerInputGameObject.SetActive(value);
                if (value)
                    InputContext = inputContext;
                else
                    Asset.Disable();
                // UpdateLastUsedDevice();
                OnEnabledOrDisabled?.Invoke(this);
            }
        }
        
        private InputContext inputContext;
        public InputContext InputContext
        {
            get => inputContext;
            set
            {
                inputContext = value;
                EnableMapsForContext(value);
#if UNITY_EDITOR
                EDITOR_OnInputContextChanged?.Invoke(this);
#endif
            }
        }

        public int ID { get; }

        private ControlScheme currentControlScheme;
        public ControlScheme CurrentControlScheme
        {
            get => currentControlScheme;
            private set
            {
                if (CurrentControlScheme == value)
                    return;
                
                currentControlScheme = value;
                OnControlSchemeChanged?.Invoke(this);
            }
        }

        private InputDevice lastUsedDevice;
        internal InputDevice LastUsedDevice
        {
            get
            {
                UpdateLastUsedDevice();
                return lastUsedDevice;
            }
        }

        internal bool IsMultiplayer
        {
            set
            {
                if (playerInput != null)
                {
                    playerInput.neverAutoSwitchControlSchemes = value;
                }
            }
        }
        
        internal InputActionAsset Asset { get; }
        
        private ReadOnlyArray<InputDevice> PairedDevices => playerInput == null ? new ReadOnlyArray<InputDevice>() : playerInput.devices;
        
        private readonly List<Keyboard> lastPairedKeyboards = new();
        private readonly Dictionary<Guid, ActionWrapper> actionWrapperTable = new();
        
        private GameObject playerInputGameObject;
        private PlayerInput playerInput;
        private InputSystemUIInputModule uiInputModule;
        private bool keyboardTextInputEnabled;
        private SpecificPlayerAnyButtonPressListenerCollection anyButtonPressListenerCollection;

        // Event System actions
        private readonly Dictionary<string, InputActionReference> eventSystemActionsPool = new();
        private InputActionReference defaultPoint;
        private InputActionReference defaultLeftClick;
        private InputActionReference defaultMiddleClick;
        private InputActionReference defaultRightClick;
        private InputActionReference defaultScrollWheel;
        private InputActionReference defaultMove;
        private InputActionReference defaultSubmit;
        private InputActionReference defaultCancel;
        private InputActionReference defaultTrackedDevicePosition;
        private InputActionReference defaultTrackedDeviceOrientation;
        
        #endregion
        
        #region Setup & Teardown

        internal void Terminate()
        {
            Enabled = false;
            anyButtonPressListenerCollection?.Clear();
            DisableKeyboardTextInput();
            DisableAllMapsAndRemoveCallbacks();
            Object.Destroy(playerInputGameObject);
        }

        private InputActionAsset InstantiateNewActions(InputActionAsset actions)
        {
            InputActionAsset oldActions = actions;
            InputActionAsset newActions = Object.Instantiate(actions);
            for (int actionMap = 0; actionMap < oldActions.actionMaps.Count; actionMap++)
            {
                for (int binding = 0; binding < oldActions.actionMaps[actionMap].bindings.Count; binding++)
                {
                    newActions.actionMaps[actionMap].ApplyBindingOverride(binding, oldActions.actionMaps[actionMap].bindings[binding]);
                }
            }

            return newActions;
        }
        
        private void SetUpInputPlayerGameObject(bool isMultiplayer, Transform parent)
        {
            if (playerInputGameObject != null)
            {
                return;
            }
            
            playerInputGameObject = new GameObject
            {
                name = $"{ID}Input",
                transform = { position = Vector3.zero, parent = parent}
            };

            playerInput = playerInputGameObject.AddComponent<PlayerInput>();
            playerInput.neverAutoSwitchControlSchemes = isMultiplayer;
            
            playerInputGameObject.AddComponent<MultiplayerEventSystem>();
            uiInputModule = playerInputGameObject.AddComponent<InputSystemUIInputModule>();
            uiInputModule.actionsAsset = Asset;
            SetEventSystemOptions();
            
            playerInput.actions = Asset;
            playerInput.uiInputModule = uiInputModule;
            
            // TODO: Unity means to add a "None" behavior which we will use once it's available since any events here are unnecessary overhead that we don't use
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents; 
            
            // TODO: Fix for new players where the string reads "Null"
            // Set this manually because the initial control scheme gets set before we are able to respond to it with event handlers.
            // CurrentControlScheme = playerInput.currentControlScheme.ToControlSchemeEnum();
        }

        private void SetDefaultEventSystemActions()
        {
            uiInputModule.point = defaultPoint;
            uiInputModule.leftClick = defaultLeftClick;
            uiInputModule.middleClick = defaultMiddleClick;
            uiInputModule.rightClick = defaultRightClick;
            uiInputModule.scrollWheel = defaultScrollWheel;
            uiInputModule.move = defaultMove;
            uiInputModule.submit = defaultSubmit;
            uiInputModule.cancel = defaultCancel;
            uiInputModule.trackedDevicePosition = defaultTrackedDevicePosition;
            uiInputModule.trackedDeviceOrientation = defaultTrackedDeviceOrientation;
        }
        
        private InputActionReference CreateInputActionReferenceToPlayerAsset(string actionID)
        {
            return string.IsNullOrEmpty(actionID)
                ? null
                : InputActionReference.Create(Asset.FindAction(actionID, throwIfNotFound: false));
        }

        #endregion

        #region Internal
        
        internal bool ControlSchemeHas<TDevice>(ControlScheme controlScheme) where TDevice : InputDevice
        {
            for (int i = 0; i < Asset.controlSchemes.Count; i++)
            {
                InputControlScheme inputControlScheme = Asset.controlSchemes[i];
                if (inputControlScheme.name != controlScheme.ToInputAssetName())
                {
                    continue;
                }

                string deviceControlPath = BindingPathHelper.GetDeviceControlPath<TDevice>();
                
                for (int j = 0; j < inputControlScheme.deviceRequirements.Count; j++)
                {
                    InputControlScheme.DeviceRequirement deviceRequirement = inputControlScheme.deviceRequirements[j];
                    if (deviceRequirement.controlPath == deviceControlPath)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        internal bool IsDevicePaired(InputDevice device)
        {
            return PairedDevices.ContainsReference(device);
        }
        
        internal bool IsUser(InputUser user)
        {
            return playerInput != null && playerInput.user.id == user.id;
        }

        internal void PairDevice(InputDevice device)
        {
            if (playerInput == null || !playerInput.user.valid)
            {
                return;
            }
         
            InputUser.PerformPairingWithDevice(device, playerInput.user);
            // UpdateLastUsedDevice();
        }

        internal void UnpairDevice(InputDevice device)
        {
            if (playerInput == null || !playerInput.user.valid)
            {
                return;
            }
            
            playerInput.user.UnpairDevice(device);
            // UpdateLastUsedDevice();
        }

        internal void UnpairDevices()
        {
            if (playerInput == null || !playerInput.user.valid)
            {
                return;
            }
            
            playerInput.user.UnpairDevices();
            // UpdateLastUsedDevice();
        }

        /// <summary>
        /// Called by the InputPlayerCollection. If we got here, it means we have already checked that the input user
        /// experiencing a change refers to this player.
        /// </summary>
        internal void HandleInputUserChange(InputUserChange inputUserChange, InputDevice inputDevice)
        {
            if (playerInput == null)
            {
                return;
            }

            switch (inputUserChange)
            {
                case InputUserChange.DevicePaired:
                case InputUserChange.DeviceUnpaired:
                case InputUserChange.DeviceLost:
                case InputUserChange.DeviceRegained:
                    UpdateDevices(inputDevice);
                    break;
                case InputUserChange.ControlSchemeChanged:
                    CurrentControlScheme = playerInput.currentControlScheme.ToControlSchemeEnum();
                    break;
            }
            
            OnInputUserChange?.Invoke(new InputUserChangeInfo(this, inputUserChange));
        }

        /// <summary>
        /// Get the ActionWrapper whose instance of InputAction matches the GUID of another InputAction, which may or
        /// may not be a different instance than that in the ActionWrapper. Important in centralizing the single
        /// source of truth for all input in the system.
        /// </summary>
        internal bool TryGetMatchingActionWrapper(InputAction otherAction, out ActionWrapper actionWrapper)
        {
            return actionWrapperTable.TryGetValue(otherAction.id, out actionWrapper);
        }

        #endregion

        #region Private

        private void AddAnyButtonPressListener(AnyButtonPressListener listener)
        {
            anyButtonPressListenerCollection ??= new SpecificPlayerAnyButtonPressListenerCollection(this);
            anyButtonPressListenerCollection.Add(listener);
        }

        private void RemoveAnyButtonPressListener(AnyButtonPressListener listener)
        {
            anyButtonPressListenerCollection.Remove(listener);
            if (anyButtonPressListenerCollection.Count == 0)
            {
                anyButtonPressListenerCollection = null;
            }
        }

        private void UpdateDevices(InputDevice changedDevice)
        {
            if (changedDevice is Keyboard && keyboardTextInputEnabled)
                EnableKeyboardTextInput();
            
            UpdateLastUsedDevice(changedDevice);
        }
        
        private void EnableKeyboardTextInput()
        {
            keyboardTextInputEnabled = true;
            lastPairedKeyboards.ForEach(kb => kb.onTextInput -= HandleTextInput); 
            UpdateLastPairedKeyboards();
            lastPairedKeyboards.ForEach(kb => kb.onTextInput += HandleTextInput);
        }

        private void DisableKeyboardTextInput()
        {
            keyboardTextInputEnabled = false;
            lastPairedKeyboards.ForEach(kb => kb.onTextInput -= HandleTextInput);
            lastPairedKeyboards.Clear();
        }
        
        private void UpdateLastPairedKeyboards()
        {
            lastPairedKeyboards.Clear();
            if (playerInput == null)
            {
                return;
            }
            
            foreach (InputDevice inputDevice in playerInput.devices)
            {
                if (inputDevice is Keyboard keyboard)
                {
                    lastPairedKeyboards.Add(keyboard);
                }
            }
        }
        
        // TODO (optimization): Currently commented out in this class in a few places, since enabling/disabling PlayerInput,
        // pairing/unpairing devices, etc. should all call HandleInputUserChange. Uncomment those calls if HandleInputUserChange
        // isn't cutting it, and delete the commented calls outright if it is!
        private void UpdateLastUsedDevice(InputDevice fallbackDevice = null)
        {
            ReadOnlyArray<InputDevice> pairedDevices = PairedDevices;
            
            if (pairedDevices.Count == 0)
            {
                lastUsedDevice = null;
            }
            else if (pairedDevices.Count == 1 ||
                     (pairedDevices.Count > 1 && (lastUsedDevice == null || !pairedDevices.ContainsReference(lastUsedDevice))))
            {
                lastUsedDevice = pairedDevices[0];
            }
            else if (fallbackDevice != null)
            {
                lastUsedDevice = fallbackDevice;
            }
        }

        private void HandleTextInput(char c)
        {
            OnKeyboardTextInput?.Invoke(c);
        }

        #endregion
        
        #region Editor-Only Debug
#if UNITY_EDITOR
        // ReSharper disable once InconsistentNaming
        internal event Action<InputPlayer> EDITOR_OnInputContextChanged;
#endif
        #endregion
    }
}
