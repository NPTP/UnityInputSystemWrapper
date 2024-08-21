using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Generated.MapActions;
using NPTP.InputSystemWrapper.Generated.MapCaches;
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

        public event Action<DeviceControlInfo> OnDeviceControlChanged;
        public event Action<InputPlayer> OnEnabledOrDisabled;
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
                    CurrentContext = currentContext;
                else
                    asset.Disable();
                UpdateLastUsedDevice();
                OnEnabledOrDisabled?.Invoke(this);
            }
        }

        public PlayerID ID { get; }
        private InputContext currentContext;
        public InputContext CurrentContext
        {
            get => currentContext;
            set
            {
                currentContext = value;
                EnableMapsForContext(value);
            }
        }

        // TODO: Still relevant? Can we get rid of it?
        public ControlScheme CurrentControlScheme { get; private set; }

        public InputDevice LastUsedDevice { get; private set; }

        private ReadOnlyArray<InputDevice> PairedDevices => playerInput == null ? new ReadOnlyArray<InputDevice>() : playerInput.devices;
        public bool IsDevicePaired(InputDevice device) => PairedDevices.ContainsReference(device);

        // MARKER.MapActionsProperties.Start
        public PlayerActions Player { get; }
        public UIActions UI { get; }
        // MARKER.MapActionsProperties.End
        
        // MARKER.MapCacheFields.Start
        private readonly PlayerMapCache playerMap;
        private readonly UIMapCache uIMap;
        // MARKER.MapCacheFields.End

        private readonly InputActionAsset asset;
        private GameObject playerInputGameObject;
        private PlayerInput playerInput;
        private InputSystemUIInputModule uiInputModule;
        private ControlScheme previousControlScheme;

        #endregion
        
        #region Setup & Teardown

        internal InputPlayer(InputActionAsset inputActionAsset, PlayerID id, bool isMultiplayer, Transform parent)
        {
            asset = InstantiateNewActions(inputActionAsset);
            ID = id;

            // MARKER.MapAndActionsInstantiation.Start
            Player = new PlayerActions();
            playerMap = new PlayerMapCache(asset);
            UI = new UIActions();
            uIMap = new UIMapCache(asset);
            // MARKER.MapAndActionsInstantiation.End

            SetUpInputPlayerGameObject(isMultiplayer, parent);
            
            SetEventSystemActions();

            // TODO: We are keeping track of the last used device with other methods now, can we get rid of this?
            playerInput.onActionTriggered += HandleAnyActionTriggered;
        }
        
        internal void Terminate()
        {
            playerInput.onActionTriggered -= HandleAnyActionTriggered;
            asset.Disable();
            DisableKeyboardTextInput();
            RemoveAllMapActionCallbacks();
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
            uiInputModule.actionsAsset = asset;
            
            playerInput.actions = asset;
            playerInput.uiInputModule = uiInputModule;
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            
            CurrentContext = currentContext;
        }

        private void SetEventSystemActions()
        {
            // MARKER.EventSystemActions.Start
            uiInputModule.point = createLocalAssetReference("32b35790-4ed0-4e9a-aa41-69ac6d629449");
            uiInputModule.middleClick = createLocalAssetReference("dad70c86-b58c-4b17-88ad-f5e53adf419e");
            uiInputModule.rightClick = createLocalAssetReference("44b200b1-1557-4083-816c-b22cbdf77ddf");
            uiInputModule.scrollWheel = createLocalAssetReference("0489e84a-4833-4c40-bfae-cea84b696689");
            uiInputModule.move = createLocalAssetReference("c95b2375-e6d9-4b88-9c4c-c5e76515df4b");
            uiInputModule.submit = createLocalAssetReference("7607c7b6-cd76-4816-beef-bd0341cfe950");
            uiInputModule.cancel = createLocalAssetReference("15cef263-9014-4fd5-94d9-4e4a6234a6ef");
            uiInputModule.trackedDevicePosition = createLocalAssetReference("24908448-c609-4bc3-a128-ea258674378a");
            uiInputModule.trackedDeviceOrientation = createLocalAssetReference("9caa3d8a-6b2f-4e8e-8bad-6ede561bd9be");
            uiInputModule.leftClick = createLocalAssetReference("3c7022bf-7922-4f7c-a998-c437916075ad");
            // MARKER.EventSystemActions.End

#pragma warning disable CS8321
            InputActionReference createLocalAssetReference(string actionID)
#pragma warning restore CS8321
            {
                return string.IsNullOrEmpty(actionID)
                    ? null
                    : InputActionReference.Create(asset.FindAction(actionID, throwIfNotFound: false));
            }
        }

        #endregion
        
        #region Internal Interface
        
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
            UpdateLastUsedDevice();
        }

        internal void UnpairDevice(InputDevice device)
        {
            if (playerInput == null || !playerInput.user.valid)
            {
                return;
            }
            
            playerInput.user.UnpairDevice(device);
            UpdateLastUsedDevice();
        }

        internal void UnpairDevices()
        {
            if (playerInput == null || !playerInput.user.valid)
            {
                return;
            }
            
            playerInput.user.UnpairDevices();
            UpdateLastUsedDevice();
        }

        internal void EnableAutoSwitching(bool enable)
        {
            if (playerInput == null)
            {
                return;
            }

            playerInput.neverAutoSwitchControlSchemes = !enable;
        }
        
        internal void HandleInputUserChange(InputUserChange inputUserChange, InputDevice inputDevice)
        {
            if (playerInput == null)
            {
                return;
            }
            
            switch (inputUserChange)
            {
                case InputUserChange.ControlSchemeChanged:
                    // TODO: ControlScheme update is basically getting ignored in favour of checking paired devices below. Remove ControlScheme tracking?
                    CurrentControlScheme = ControlSchemeNameToEnum(playerInput.currentControlScheme);
                    goto deviceChange;
                case InputUserChange.DevicePaired:
                case InputUserChange.DeviceUnpaired:
                case InputUserChange.ControlsChanged: // When user bindings have changed (among other things)
                deviceChange:
                    InputDevice previousDevice = LastUsedDevice;
                    UpdateLastUsedDevice(inputDevice);
                    if (previousDevice == LastUsedDevice || (previousDevice is Mouse or Keyboard && LastUsedDevice is Mouse or Keyboard))
                        break;
                    OnDeviceControlChanged?.Invoke(new DeviceControlInfo(this, inputUserChange));
                    break;
            }
        }
        
        internal void FindActionEventAndSubscribe(InputActionReference actionReference, Action<InputAction.CallbackContext> callback, bool subscribe)
        {
            InputActionMap map = asset.FindActionMap(actionReference.action.actionMap.name);
            if (map == null) return;
            InputAction action = map.FindAction(actionReference.action.name);
            if (action == null) return;
            
            // The auto-generated code below ensures that the action used is from the correct asset AND behaves
            // identically to all direct action subscriptions in this wrapper system (where double subs are
            // prevented, subs to actions can be made at any time regardless of map/action/player state, and the
            // event signatures look the same as the ones subscribed to manually).

            // MARKER.ChangeSubscriptionIfStatements.Start
            if (playerMap.ActionMap == map)
            {
                if (action == playerMap.Move)
                {
                    Player.OnMove -= callback;
                    if (subscribe) Player.OnMove += callback;
                }
                else if (action == playerMap.Look)
                {
                    Player.OnLook -= callback;
                    if (subscribe) Player.OnLook += callback;
                }
                else if (action == playerMap.Fire)
                {
                    Player.OnFire -= callback;
                    if (subscribe) Player.OnFire += callback;
                }
            }
            else if (uIMap.ActionMap == map)
            {
                if (action == uIMap.Navigate)
                {
                    UI.OnNavigate -= callback;
                    if (subscribe) UI.OnNavigate += callback;
                }
                else if (action == uIMap.Submit)
                {
                    UI.OnSubmit -= callback;
                    if (subscribe) UI.OnSubmit += callback;
                }
                else if (action == uIMap.Cancel)
                {
                    UI.OnCancel -= callback;
                    if (subscribe) UI.OnCancel += callback;
                }
                else if (action == uIMap.Point)
                {
                    UI.OnPoint -= callback;
                    if (subscribe) UI.OnPoint += callback;
                }
                else if (action == uIMap.Click)
                {
                    UI.OnClick -= callback;
                    if (subscribe) UI.OnClick += callback;
                }
                else if (action == uIMap.ScrollWheel)
                {
                    UI.OnScrollWheel -= callback;
                    if (subscribe) UI.OnScrollWheel += callback;
                }
                else if (action == uIMap.MiddleClick)
                {
                    UI.OnMiddleClick -= callback;
                    if (subscribe) UI.OnMiddleClick += callback;
                }
                else if (action == uIMap.RightClick)
                {
                    UI.OnRightClick -= callback;
                    if (subscribe) UI.OnRightClick += callback;
                }
                else if (action == uIMap.TrackedDevicePosition)
                {
                    UI.OnTrackedDevicePosition -= callback;
                    if (subscribe) UI.OnTrackedDevicePosition += callback;
                }
                else if (action == uIMap.TrackedDeviceOrientation)
                {
                    UI.OnTrackedDeviceOrientation -= callback;
                    if (subscribe) UI.OnTrackedDeviceOrientation += callback;
                }
            }
            // MARKER.ChangeSubscriptionIfStatements.End
        }

        #endregion

        #region Private Functionality
        
        private void HandleAnyActionTriggered(InputAction.CallbackContext context)
        {
            LastUsedDevice = context.control.device;
        }
        
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
        
        // TODO: Check if this only needs to be used in one place (HandleInputUserChange) instead of 5 places, since enabling/disabling PlayerInput,
        // pairing/unpairing devices, these will all call HandleInputUserChange, right? Check, and avoid redundancy if so
        private void UpdateLastUsedDevice(InputDevice fallbackDevice = null)
        {
            ReadOnlyArray<InputDevice> pairedDevices = PairedDevices;
            if (pairedDevices.Count == 0)
            {
                LastUsedDevice = null;
            }
            else if (pairedDevices.Count == 1 ||
                     (pairedDevices.Count > 1 && (LastUsedDevice == null || !pairedDevices.ContainsReference(LastUsedDevice))))
            {
                LastUsedDevice = pairedDevices[0];
            }
            else if (fallbackDevice != null)
            {
                LastUsedDevice = fallbackDevice;
            }
        }
        
        private void RemoveAllMapActionCallbacks()
        {
            // MARKER.MapActionsRemoveCallbacks.Start
            playerMap.RemoveCallbacks(Player);
            uIMap.RemoveCallbacks(UI);
            // MARKER.MapActionsRemoveCallbacks.End
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

        private static ControlScheme ControlSchemeNameToEnum(string controlSchemeName)
        {
#pragma warning disable CS8509
            return controlSchemeName switch
#pragma warning restore CS8509
            {
                // MARKER.ControlSchemeSwitch.Start
                "Keyboard&Mouse" => ControlScheme.KeyboardMouse,
                "Gamepad" => ControlScheme.Gamepad,
                "Touch" => ControlScheme.Touch,
                "Joystick" => ControlScheme.Joystick,
                "XR" => ControlScheme.XR,
                // MARKER.ControlSchemeSwitch.End
            };
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
                    playerMap.Disable();
                    playerMap.RemoveCallbacks(Player);
                    uIMap.Disable();
                    uIMap.RemoveCallbacks(UI);
                    break;
                case InputContext.Player:
                    DisableKeyboardTextInput();
                    playerMap.Enable();
                    playerMap.AddCallbacks(Player);
                    uIMap.Disable();
                    uIMap.RemoveCallbacks(UI);
                    break;
                case InputContext.UI:
                    EnableKeyboardTextInput();
                    playerMap.Disable();
                    playerMap.RemoveCallbacks(Player);
                    uIMap.Enable();
                    uIMap.AddCallbacks(UI);
                    break;
                // MARKER.EnableContextSwitchMembers.End
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            }
        }

        #endregion

        // TODO: Remove section after debugging finished
        #region Editor Debugging

#if UNITY_EDITOR
        public bool EDITOR_Enabled
        {
            set => Enabled = value;
        }
#endif

        #endregion
    }
}
