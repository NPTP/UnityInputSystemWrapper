using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Actions;
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
        public PlayerActions Player { get; }
        public UIActions UI { get; }
        public XXXActions XXX { get; }
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
        
        private GameObject playerInputGameObject;
        private PlayerInput playerInput;
        private InputSystemUIInputModule uiInputModule;
        private bool keyboardTextInputEnabled;
        private readonly List<Keyboard> lastPairedKeyboards = new();

        #endregion
        
        #region Setup & Teardown

        internal InputPlayer(InputActionAsset asset, PlayerID id, bool isMultiplayer, Transform parent)
        {
            Asset = InstantiateNewActions(asset);
            ID = id;
            
            // MARKER.ActionsInstantiation.Start
            Player = new PlayerActions(Asset);
            UI = new UIActions(Asset);
            XXX = new XXXActions(Asset);
            // MARKER.ActionsInstantiation.End
            
            SetUpInputPlayerGameObject(isMultiplayer, parent);
            
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

        private void SetDefaultEventSystemActions()
        {
            // MARKER.EventSystemActions.Start
            uiInputModule.point = CreateInputActionReferenceToPlayerAsset("32b35790-4ed0-4e9a-aa41-69ac6d629449");
            uiInputModule.leftClick = CreateInputActionReferenceToPlayerAsset("3c7022bf-7922-4f7c-a998-c437916075ad");
            uiInputModule.middleClick = CreateInputActionReferenceToPlayerAsset("dad70c86-b58c-4b17-88ad-f5e53adf419e");
            uiInputModule.rightClick = CreateInputActionReferenceToPlayerAsset("44b200b1-1557-4083-816c-b22cbdf77ddf");
            uiInputModule.scrollWheel = CreateInputActionReferenceToPlayerAsset("0489e84a-4833-4c40-bfae-cea84b696689");
            uiInputModule.move = CreateInputActionReferenceToPlayerAsset("c95b2375-e6d9-4b88-9c4c-c5e76515df4b");
            uiInputModule.submit = CreateInputActionReferenceToPlayerAsset("7607c7b6-cd76-4816-beef-bd0341cfe950");
            uiInputModule.cancel = CreateInputActionReferenceToPlayerAsset("15cef263-9014-4fd5-94d9-4e4a6234a6ef");
            uiInputModule.trackedDevicePosition = CreateInputActionReferenceToPlayerAsset("24908448-c609-4bc3-a128-ea258674378a");
            uiInputModule.trackedDeviceOrientation = CreateInputActionReferenceToPlayerAsset("9caa3d8a-6b2f-4e8e-8bad-6ede561bd9be");
            // MARKER.EventSystemActions.End
        }
        
        private InputActionReference CreateInputActionReferenceToPlayerAsset(string actionID)
        {
            return string.IsNullOrEmpty(actionID)
                ? null
                : InputActionReference.Create(Asset.FindAction(actionID, throwIfNotFound: false));
        }

        #endregion

        #region Internal
        
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

        internal bool TryGetMapAndActionInPlayerAsset(InputAction actionFromReference, out InputActionMap map, out InputAction action)
        {
            action = null;
            map = null;

            if (actionFromReference == null)
                return false;

            map = Asset.FindActionMap(actionFromReference.actionMap.name);
            if (map == null)
                return false;
            
            action = map.FindAction(actionFromReference.name);
            return action != null;
        }
        
        internal ActionWrapper FindActionWrapper(ActionReference actionReference)
        {
            if (!TryGetMapAndActionInPlayerAsset(actionReference.InternalAction, out InputActionMap map, out InputAction action))
            {
                return null;
            }
            
            // The auto-generated code below ensures that the action used is from the correct asset AND behaves
            // identically to all direct action subscriptions in this wrapper system (where double subs are
            // prevented, subs to actions can be made at any time regardless of map/action/player state, and the
            // event signatures look the same as the ones subscribed to manually).

            // MARKER.FindActionWrapperIfElse.Start
            if (Player.ActionMap == map)
            {
                if (action == Player.Move.InputAction) return Player.Move;
                if (action == Player.Look.InputAction) return Player.Look;
                if (action == Player.Fire.InputAction) return Player.Fire;
            }
            else if (UI.ActionMap == map)
            {
                if (action == UI.Navigate.InputAction) return UI.Navigate;
                if (action == UI.Submit.InputAction) return UI.Submit;
                if (action == UI.Cancel.InputAction) return UI.Cancel;
                if (action == UI.Point.InputAction) return UI.Point;
                if (action == UI.Click.InputAction) return UI.Click;
                if (action == UI.ScrollWheel.InputAction) return UI.ScrollWheel;
                if (action == UI.MiddleClick.InputAction) return UI.MiddleClick;
                if (action == UI.RightClick.InputAction) return UI.RightClick;
                if (action == UI.TrackedDevicePosition.InputAction) return UI.TrackedDevicePosition;
                if (action == UI.TrackedDeviceOrientation.InputAction) return UI.TrackedDeviceOrientation;
            }
            else if (XXX.ActionMap == map)
            {
                if (action == XXX.Any.InputAction) return XXX.Any;
                if (action == XXX.Analog.InputAction) return XXX.Analog;
                if (action == XXX.Axis.InputAction) return XXX.Axis;
                if (action == XXX.Bone.InputAction) return XXX.Bone;
                if (action == XXX.Delta.InputAction) return XXX.Delta;
                if (action == XXX.Digital.InputAction) return XXX.Digital;
                if (action == XXX.Double.InputAction) return XXX.Double;
                if (action == XXX.Dpad.InputAction) return XXX.Dpad;
                if (action == XXX.Eyes.InputAction) return XXX.Eyes;
                if (action == XXX.Integer.InputAction) return XXX.Integer;
                if (action == XXX.Pose.InputAction) return XXX.Pose;
                if (action == XXX.Quaternion.InputAction) return XXX.Quaternion;
                if (action == XXX.Stick.InputAction) return XXX.Stick;
                if (action == XXX.Touch.InputAction) return XXX.Touch;
                if (action == XXX.Vector2.InputAction) return XXX.Vector2;
                if (action == XXX.Vector3.InputAction) return XXX.Vector3;
                if (action == XXX.Button.InputAction) return XXX.Button;
                if (action == XXX.DiscreteButton.InputAction) return XXX.DiscreteButton;
            }
            // MARKER.FindActionWrapperIfElse.End

            return null;
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
            Player.DisableAndUnregisterCallbacks();
            UI.DisableAndUnregisterCallbacks();
            XXX.DisableAndUnregisterCallbacks();
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
                case InputContext.AllInputDisabled:
                    DisableKeyboardTextInput();
                    Player.DisableAndUnregisterCallbacks();
                    UI.DisableAndUnregisterCallbacks();
                    XXX.DisableAndUnregisterCallbacks();
                    break;
                case InputContext.Player:
                    DisableKeyboardTextInput();
                    Player.DisableAndUnregisterCallbacks();
                    UI.DisableAndUnregisterCallbacks();
                    XXX.EnableAndRegisterCallbacks();
                    break;
                case InputContext.UI:
                    EnableKeyboardTextInput();
                    Player.DisableAndUnregisterCallbacks();
                    UI.EnableAndRegisterCallbacks();
                    XXX.DisableAndUnregisterCallbacks();
                    break;
                // MARKER.EnableContextSwitchMembers.End
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            }
            
            // TODO (optimization): possibility that the InputActionReference ScriptableObjects (for event system actions)
            // do not get garbage collected as one might expect, so we do it manually via Resources.UnloadUnusedAssets.
            // However, this can be overkill if there are lots of other assets to unload, and can cause a performance hitch.
            // So, we should have a much more controlled management of which InputActionReferences are destroyed, maintained etc. by
            // keeping track of which ones were overridden and which weren't, and call Destroy on those specific ones so the GC
            // takes care of them at a better time.
            
            Resources.UnloadUnusedAssets();
        }

        #endregion
        
        #region Editor-Only Debug
#if UNITY_EDITOR
        public event Action<InputPlayer> EDITOR_OnInputContextChanged;
#endif
        #endregion
    }
}
