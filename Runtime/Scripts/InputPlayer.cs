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
                    InputContext = inputContext;
                else
                    Asset.Disable();
                UpdateLastUsedDevice();
                OnEnabledOrDisabled?.Invoke(this);
            }
        }

        public PlayerID ID { get; }
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

        // TODO: Still relevant? Can we get rid of it?
        public ControlScheme CurrentControlScheme { get; private set; }

        public InputDevice LastUsedDevice { get; private set; }

        private ReadOnlyArray<InputDevice> PairedDevices => playerInput == null ? new ReadOnlyArray<InputDevice>() : playerInput.devices;
        public bool IsDevicePaired(InputDevice device) => PairedDevices.ContainsReference(device);

        // MARKER.ActionsProperties.Start
        public PlayerActions Player { get; }
        public UIActions UI { get; }
        // MARKER.ActionsProperties.End

        public InputActionAsset Asset { get; }

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
            
            // TODO: We are keeping track of the last used device with other methods now, can we get rid of this?
            playerInput.onActionTriggered += HandleAnyActionTriggered;
        }
        
        internal void Terminate()
        {
            playerInput.onActionTriggered -= HandleAnyActionTriggered;
            Asset.Disable();
            DisableKeyboardTextInput();
            DisableAllMapsAndRemoveCallbacks();
            Object.Destroy(playerInputGameObject);
        }

        private InputActionAsset InstantiateNewActions(InputActionAsset actions)
        {
            // TODO: Load bindings from disk based on player ID and use as overrides.
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
                    : InputActionReference.Create(Asset.FindAction(actionID, throwIfNotFound: false));
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
            InputActionMap map = Asset.FindActionMap(actionReference.action.actionMap.name);
            if (map == null) return;
            InputAction action = map.FindAction(actionReference.action.name);
            if (action == null) return;
            
            // The auto-generated code below ensures that the action used is from the correct asset AND behaves
            // identically to all direct action subscriptions in this wrapper system (where double subs are
            // prevented, subs to actions can be made at any time regardless of map/action/player state, and the
            // event signatures look the same as the ones subscribed to manually).

            // MARKER.ChangeSubscriptionIfStatements.Start
            if (Player.ActionMap == map)
            {
                if (action == Player.Move.InputAction)
                {
                    Player.OnMove -= callback;
                    if (subscribe) Player.OnMove += callback;
                }
                else if (action == Player.Look.InputAction)
                {
                    Player.OnLook -= callback;
                    if (subscribe) Player.OnLook += callback;
                }
                else if (action == Player.Fire.InputAction)
                {
                    Player.OnFire -= callback;
                    if (subscribe) Player.OnFire += callback;
                }
            }
            else if (UI.ActionMap == map)
            {
                if (action == UI.Navigate.InputAction)
                {
                    UI.OnNavigate -= callback;
                    if (subscribe) UI.OnNavigate += callback;
                }
                else if (action == UI.Submit.InputAction)
                {
                    UI.OnSubmit -= callback;
                    if (subscribe) UI.OnSubmit += callback;
                }
                else if (action == UI.Cancel.InputAction)
                {
                    UI.OnCancel -= callback;
                    if (subscribe) UI.OnCancel += callback;
                }
                else if (action == UI.Point.InputAction)
                {
                    UI.OnPoint -= callback;
                    if (subscribe) UI.OnPoint += callback;
                }
                else if (action == UI.Click.InputAction)
                {
                    UI.OnClick -= callback;
                    if (subscribe) UI.OnClick += callback;
                }
                else if (action == UI.ScrollWheel.InputAction)
                {
                    UI.OnScrollWheel -= callback;
                    if (subscribe) UI.OnScrollWheel += callback;
                }
                else if (action == UI.MiddleClick.InputAction)
                {
                    UI.OnMiddleClick -= callback;
                    if (subscribe) UI.OnMiddleClick += callback;
                }
                else if (action == UI.RightClick.InputAction)
                {
                    UI.OnRightClick -= callback;
                    if (subscribe) UI.OnRightClick += callback;
                }
                else if (action == UI.TrackedDevicePosition.InputAction)
                {
                    UI.OnTrackedDevicePosition -= callback;
                    if (subscribe) UI.OnTrackedDevicePosition += callback;
                }
                else if (action == UI.TrackedDeviceOrientation.InputAction)
                {
                    UI.OnTrackedDeviceOrientation -= callback;
                    if (subscribe) UI.OnTrackedDeviceOrientation += callback;
                }
            }
            // MARKER.ChangeSubscriptionIfStatements.End
        }

        #endregion

        #region Private Functionality
        
        private void HandleAnyActionTriggered(InputAction.CallbackContext callbackContext)
        {
            LastUsedDevice = callbackContext.control.device;
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
