using System;
using System.Collections.Generic;
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
            }
        }

        public PlayerID ID { get; }
        public ControlScheme CurrentControlScheme { get; private set; }
        
        // MARKER.ActionsProperties.Start
        public PlayerActions Player { get; }
        public UIActions UI { get; }
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
        private ControlScheme previousControlScheme;
        
        #endregion
        
        #region Setup & Teardown

        internal InputPlayer(InputActionAsset asset, PlayerID id, bool isMultiplayer, Transform parent)
        {
            Asset = InstantiateNewActions(asset);
            ID = id;
            
            // MARKER.ActionsInstantiation.Start
            Player = new PlayerActions(Asset);
            UI = new UIActions(Asset);
            // MARKER.ActionsInstantiation.End
            
            SetUpInputPlayerGameObject(isMultiplayer, parent);
            
            SetEventSystemActions();
            
            // TODO (optimization): We are keeping track of the last used device with other methods now, can we get rid of this? Would probably be a big performance benefit to do so.
            // playerInput.onActionTriggered += HandleAnyActionTriggered;
        }
        
        internal void Terminate()
        {
            // TODO (optimization): We are keeping track of the last used device with other methods now, can we get rid of this? Would probably be a big performance benefit to do so.
            // playerInput.onActionTriggered -= HandleAnyActionTriggered;
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
            
            playerInput.actions = Asset;
            playerInput.uiInputModule = uiInputModule;
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            
            InputContext = inputContext;
        }

        private void SetEventSystemActions()
        {
            // MARKER.EventSystemActions.Start
            uiInputModule.point = createLocalAssetReference("32b35790-4ed0-4e9a-aa41-69ac6d629449");
            uiInputModule.leftClick = createLocalAssetReference("3c7022bf-7922-4f7c-a998-c437916075ad");
            uiInputModule.middleClick = createLocalAssetReference("dad70c86-b58c-4b17-88ad-f5e53adf419e");
            uiInputModule.rightClick = createLocalAssetReference("44b200b1-1557-4083-816c-b22cbdf77ddf");
            uiInputModule.scrollWheel = createLocalAssetReference("0489e84a-4833-4c40-bfae-cea84b696689");
            uiInputModule.move = createLocalAssetReference("c95b2375-e6d9-4b88-9c4c-c5e76515df4b");
            uiInputModule.submit = createLocalAssetReference("7607c7b6-cd76-4816-beef-bd0341cfe950");
            uiInputModule.cancel = createLocalAssetReference("15cef263-9014-4fd5-94d9-4e4a6234a6ef");
            uiInputModule.trackedDevicePosition = createLocalAssetReference("24908448-c609-4bc3-a128-ea258674378a");
            uiInputModule.trackedDeviceOrientation = createLocalAssetReference("9caa3d8a-6b2f-4e8e-8bad-6ede561bd9be");
            // MARKER.EventSystemActions.End

#pragma warning disable CS8321
            InputActionReference createLocalAssetReference(string actionID)
#pragma warning restore CS8321
            {
                return string.IsNullOrEmpty(actionID)
                    ? null
                    : InputActionReference.Create(Asset.FindAction(actionID, throwIfNotFound: false));
            }
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
                case InputUserChange.ControlSchemeChanged:
                case InputUserChange.DevicePaired:
                case InputUserChange.DeviceUnpaired:
                case InputUserChange.DeviceLost:
                case InputUserChange.DeviceRegained:
                case InputUserChange.ControlsChanged:
                    CurrentControlScheme = playerInput.currentControlScheme.ToControlSchemeEnum();
                    UpdateLastUsedDevice(inputDevice);
                    // TODO (optimization): Gate this behind a check that control scheme/device/bindings has actually changed?
                    OnInputUserChange?.Invoke(new InputUserChangeInfo(this, inputUserChange));
                    break;
            }
        }

        internal bool TryGetMapAndActionInPlayerAsset(InputActionReference actionReference, out InputActionMap map, out InputAction action)
        {
            action = null;
            map = Asset.FindActionMap(actionReference.action.actionMap.name);
            if (map == null) return false;
            action = map.FindAction(actionReference.action.name);
            return action != null;
        }
        
        internal void FindActionEventAndSubscribe(InputActionReference actionReference, Action<InputAction.CallbackContext> callback, bool subscribe)
        {
            if (!TryGetMapAndActionInPlayerAsset(actionReference, out InputActionMap map, out InputAction action))
            {
                return;
            }
            
            // The auto-generated code below ensures that the action used is from the correct asset AND behaves
            // identically to all direct action subscriptions in this wrapper system (where double subs are
            // prevented, subs to actions can be made at any time regardless of map/action/player state, and the
            // event signatures look the same as the ones subscribed to manually).

            // MARKER.ChangeSubscriptionIfStatements.Start
            if (Player.ActionMap == map)
            {
                if (action == Player.Move.InputAction)
                {
                    Player.Move.OnEvent -= callback;
                    if (subscribe) Player.Move.OnEvent += callback;
                }
                else if (action == Player.Look.InputAction)
                {
                    Player.Look.OnEvent -= callback;
                    if (subscribe) Player.Look.OnEvent += callback;
                }
                else if (action == Player.Fire.InputAction)
                {
                    Player.Fire.OnEvent -= callback;
                    if (subscribe) Player.Fire.OnEvent += callback;
                }
            }
            else if (UI.ActionMap == map)
            {
                if (action == UI.Navigate.InputAction)
                {
                    UI.Navigate.OnEvent -= callback;
                    if (subscribe) UI.Navigate.OnEvent += callback;
                }
                else if (action == UI.Submit.InputAction)
                {
                    UI.Submit.OnEvent -= callback;
                    if (subscribe) UI.Submit.OnEvent += callback;
                }
                else if (action == UI.Cancel.InputAction)
                {
                    UI.Cancel.OnEvent -= callback;
                    if (subscribe) UI.Cancel.OnEvent += callback;
                }
                else if (action == UI.Point.InputAction)
                {
                    UI.Point.OnEvent -= callback;
                    if (subscribe) UI.Point.OnEvent += callback;
                }
                else if (action == UI.Click.InputAction)
                {
                    UI.Click.OnEvent -= callback;
                    if (subscribe) UI.Click.OnEvent += callback;
                }
                else if (action == UI.ScrollWheel.InputAction)
                {
                    UI.ScrollWheel.OnEvent -= callback;
                    if (subscribe) UI.ScrollWheel.OnEvent += callback;
                }
                else if (action == UI.MiddleClick.InputAction)
                {
                    UI.MiddleClick.OnEvent -= callback;
                    if (subscribe) UI.MiddleClick.OnEvent += callback;
                }
                else if (action == UI.RightClick.InputAction)
                {
                    UI.RightClick.OnEvent -= callback;
                    if (subscribe) UI.RightClick.OnEvent += callback;
                }
                else if (action == UI.TrackedDevicePosition.InputAction)
                {
                    UI.TrackedDevicePosition.OnEvent -= callback;
                    if (subscribe) UI.TrackedDevicePosition.OnEvent += callback;
                }
                else if (action == UI.TrackedDeviceOrientation.InputAction)
                {
                    UI.TrackedDeviceOrientation.OnEvent -= callback;
                    if (subscribe) UI.TrackedDeviceOrientation.OnEvent += callback;
                }
            }
            // MARKER.ChangeSubscriptionIfStatements.End
        }

        #endregion

        #region Private
        
        // TODO (optimization): We are keeping track of the last used device with other methods now, can we get rid of this? Would probably be a big performance benefit to do so.
        // private void HandleAnyActionTriggered(InputAction.CallbackContext callbackContext)
        // {
        //     lastUsedDevice = callbackContext.control.device;
        // }
        
        private void EnableKeyboardTextInput()
        {
            GetKeyboards().ForEach(keyboard =>
            {
                keyboard.onTextInput -= HandleTextInput;
                keyboard.onTextInput += HandleTextInput;
            });
        }

        private void DisableKeyboardTextInput()
        {
            GetKeyboards().ForEach(keyboard => keyboard.onTextInput -= HandleTextInput);
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
            // MARKER.DisableAllMapsAndRemoveCallbacksBody.End
        }
        
        private List<Keyboard> GetKeyboards()
        {
            List<Keyboard> keyboards = new();
            if (playerInput == null)
            {
                return keyboards;
            }
            
            foreach (InputDevice inputDevice in playerInput.devices)
            {
                if (inputDevice is Keyboard keyboard)
                {
                    keyboards.Add(keyboard);
                }
            }

            return keyboards;
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
            
            switch (context)
            {
                // MARKER.EnableContextSwitchMembers.Start
                case InputContext.AllInputDisabled:
                    DisableKeyboardTextInput();
                    Player.DisableAndUnregisterCallbacks();
                    UI.DisableAndUnregisterCallbacks();
                    break;
                case InputContext.Player:
                    DisableKeyboardTextInput();
                    Player.EnableAndRegisterCallbacks();
                    UI.DisableAndUnregisterCallbacks();
                    break;
                case InputContext.UI:
                    EnableKeyboardTextInput();
                    Player.DisableAndUnregisterCallbacks();
                    UI.EnableAndRegisterCallbacks();
                    break;
                // MARKER.EnableContextSwitchMembers.End
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            }
        }

        #endregion
    }
}
