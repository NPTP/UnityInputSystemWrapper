using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Generated.Actions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using Object = UnityEngine.Object;

namespace NPTP.InputSystemWrapper
{
    public sealed class InputPlayer
    {
        #region Field & Properties

        /// <summary>
        /// Corresponds to InputUser.onChange, for this player specifically.
        /// </summary>
        public event Action<InputUserChangeInfo> OnInputUserChange;
        
        public event Action<ControlScheme> OnControlSchemeChanged;

        /// <summary>
        /// The input player can be used when enabled, and is ignored when disabled.
        /// </summary>
        public event Action<InputPlayer> OnEnabledOrDisabled;
        
        /// <summary>
        /// Sends the keyboard text character that was just input by this player,
        /// but only if the current InputContext that allows keyboard text input is active.
        /// </summary>
        public event Action<char> OnKeyboardTextInput;

        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            internal set
            {
                if (playerInput == null)
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

        public PlayerID ID { get; }
        public ControlScheme CurrentControlScheme { get; private set; }
        
        // MARKER.ActionsProperties.Start
        // MARKER.ActionsProperties.End

        private InputDevice lastUsedDevice;
        internal InputDevice LastUsedDevice
        {
            get
            {
                UpdateLastUsedDevice();
                return lastUsedDevice;
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

        internal InputPlayer(InputActionAsset asset, PlayerID id, bool isMultiplayer, Transform parent)
        {
            Asset = InstantiateNewActions(asset);
            ID = id;
            
            // MARKER.ActionsInstantiation.Start
            // MARKER.ActionsInstantiation.End
            
            SetUpInputPlayerGameObject(isMultiplayer, parent);
            PopulateEventSystemActionsPool();
            
            // Input context gets set by top Input class after this instantiation, which sets up maps & event system actions/overrides, so we don't have to handle that here.
        }
        
        internal void Terminate()
        {
            Asset.Disable();
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
                
            if (isMultiplayer)
                playerInputGameObject.AddComponent<MultiplayerEventSystem>();
            else
                playerInputGameObject.AddComponent<EventSystem>();
                
            uiInputModule = playerInputGameObject.AddComponent<InputSystemUIInputModule>();
            uiInputModule.actionsAsset = Asset;
            SetEventSystemOptions();
            
            playerInput.actions = Asset;
            playerInput.uiInputModule = uiInputModule;
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents; 
            
            // Set this manually because the initial control scheme gets set before we are able to respond to it with event handlers.
            CurrentControlScheme = playerInput.currentControlScheme.ToControlSchemeEnum();
        }

        private void SetEventSystemOptions()
        {
            // MARKER.EventSystemOptions.Start
            uiInputModule.moveRepeatDelay = 0.5f;
            uiInputModule.moveRepeatRate = 0.1f;
            uiInputModule.deselectOnBackgroundClick = false;
            uiInputModule.pointerBehavior = UIPointerBehavior.SingleMouseOrPenButMultiTouchAndTrack;
            uiInputModule.cursorLockBehavior = InputSystemUIInputModule.CursorLockBehavior.OutsideScreen;
            // MARKER.EventSystemOptions.End
        }
        
        /// <summary>
        /// Adds all default and override event system InputActionReferences to a shared pool to
        /// reduce duplication and lookup time.
        /// </summary>
        private void PopulateEventSystemActionsPool()
        {
            // MARKER.PopulateEventSystemActionsPool.Start
            // MARKER.PopulateEventSystemActionsPool.End
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

        internal void EnableAutoSwitching(bool enable)
        {
            if (playerInput == null)
            {
                return;
            }

            playerInput.neverAutoSwitchControlSchemes = !enable;
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
                    ControlScheme previousControlScheme = CurrentControlScheme;
                    CurrentControlScheme = playerInput.currentControlScheme.ToControlSchemeEnum();
                    if (previousControlScheme != CurrentControlScheme)
                    {
                        OnControlSchemeChanged?.Invoke(CurrentControlScheme);
                    }
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
        
        private void DisableAllMapsAndRemoveCallbacks()
        {
            // MARKER.DisableAllMapsAndRemoveCallbacksBody.Start
            // MARKER.DisableAllMapsAndRemoveCallbacksBody.End
        }
        
        private void HandleTextInput(char c)
        {
            OnKeyboardTextInput?.Invoke(c);
        }
        
        private void EnableMapsForContext(InputContext context)
        {
            if (!Enabled)
            {
                return;
            }
            
            SetDefaultEventSystemActions();
            
            switch (context)
            {
                // MARKER.EnableContextSwitchMembers.Start
                // MARKER.EnableContextSwitchMembers.End
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            }
        }

        #endregion
        
        #region Editor-Only Debug
#if UNITY_EDITOR
        internal event Action<InputPlayer> EDITOR_OnInputContextChanged;
#endif
        #endregion
    }
}
