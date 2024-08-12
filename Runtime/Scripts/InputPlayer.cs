using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using UnityInputSystemWrapper.Data;
using UnityInputSystemWrapper.Generated.MapActions;
using UnityInputSystemWrapper.Generated.MapCaches;
using Object = UnityEngine.Object;

namespace UnityInputSystemWrapper
{
    public class InputPlayer
    {
        #region Field & Properties

        public event Action<InputPlayer> OnEnabledOrDisabled;
        public event Action<ControlScheme> OnControlSchemeChanged;
        public event Action<char> OnKeyboardTextInput;

        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (playerInput == null)
                {
                    return;
                }
            
                enabled = value;
                playerInputGameObject.SetActive(value);
                if (value) CurrentContext = currentContext;
                else asset.Disable();
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
        public ControlScheme CurrentControlScheme { get; private set; }
        public InputDevice LastUsedDevice { get; private set; }

        public ReadOnlyArray<InputDevice> PairedDevices => playerInput == null ? new ReadOnlyArray<InputDevice>() : playerInput.devices;

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

        #endregion
        
        #region Setup

        // TODO: make internal
        public InputPlayer(RuntimeInputData runtimeInputData, PlayerID id, bool isMultiplayer, Transform parent)
        {
            asset = InstantiateNewActions(runtimeInputData.InputActionAsset);
            ID = id;

            // MARKER.MapAndActionsInstantiation.Start
            Player = new PlayerActions();
            playerMap = new PlayerMapCache(asset);
            UI = new UIActions();
            uIMap = new UIMapCache(asset);
            // MARKER.MapAndActionsInstantiation.End

            SetUpInputPlayerGameObject(isMultiplayer, parent);
            
            SetEventSystemActions(runtimeInputData);

            // TODO: Is there a better solution to keep track of the last used device than this? If so, let's implement it
            playerInput.onActionTriggered += HandleAnyActionTriggered;
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

        private void SetEventSystemActions(RuntimeInputData runtimeInputData)
        {
            uiInputModule.point = createLocalAssetReference(runtimeInputData.Point);
            uiInputModule.middleClick = createLocalAssetReference(runtimeInputData.MiddleClick);
            uiInputModule.rightClick = createLocalAssetReference(runtimeInputData.RightClick);
            uiInputModule.scrollWheel = createLocalAssetReference(runtimeInputData.ScrollWheel);
            uiInputModule.move = createLocalAssetReference(runtimeInputData.Move);
            uiInputModule.submit = createLocalAssetReference(runtimeInputData.Submit);
            uiInputModule.cancel = createLocalAssetReference(runtimeInputData.Cancel);
            uiInputModule.trackedDevicePosition = createLocalAssetReference(runtimeInputData.TrackedDevicePosition);
            uiInputModule.trackedDeviceOrientation = createLocalAssetReference(runtimeInputData.TrackedDeviceOrientation);
            uiInputModule.leftClick = createLocalAssetReference(runtimeInputData.LeftClick);

            InputActionReference createLocalAssetReference(InputActionReference reference)
            {
                if (reference == null || reference.action == null)
                    return null;
                
                return InputActionReference.Create(asset.FindAction(reference.action.name, throwIfNotFound: false));
            }
        }
        
        // TODO: make internal
        public void Terminate()
        {
            playerInput.onActionTriggered -= HandleAnyActionTriggered;
            asset.Disable();
            DisableKeyboardTextInput();
            RemoveAllMapActionCallbacks();
            Object.Destroy(playerInputGameObject);
        }

        #endregion
        
        #region Public Interface
        
        // TODO: make internal
        public void PairDevice(InputDevice device)
        {
            if (playerInput == null || !playerInput.user.valid)
            {
                return;
            }
         
            InputUser.PerformPairingWithDevice(device, playerInput.user);
            UpdateLastUsedDevice();
        }
        
        public void PairDevices(InputControlList<InputDevice> unpairedDevices)
        {
            if (playerInput == null || !playerInput.user.valid)
            {
                return;
            }

            foreach (InputDevice device in unpairedDevices)
            {
                InputUser.PerformPairingWithDevice(device, playerInput.user);
            }
        }

        public void UnpairDevice(InputDevice device)
        {
            if (playerInput == null || !playerInput.user.valid)
            {
                return;
            }
            
            playerInput.user.UnpairDevice(device);
            UpdateLastUsedDevice();
        }

        public void UnpairDevices()
        {
            if (playerInput == null || !playerInput.user.valid)
            {
                return;
            }
            
            playerInput.user.UnpairDevices();
            UpdateLastUsedDevice();
        }

        // TODO: make internal
        public void EnableAutoSwitching(bool enable)
        {
            if (playerInput == null)
            {
                return;
            }

            playerInput.neverAutoSwitchControlSchemes = !enable;
        }

        // TODO: make internal
        public bool IsUser(InputUser user) => playerInput != null && playerInput.user.id == user.id;

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

        // TODO: make internal
        public void FindActionEventAndSubscribe(InputActionReference actionReference, Action<InputAction.CallbackContext> callback, bool subscribe)
        {
            InputActionMap map = asset.FindActionMap(actionReference.action.actionMap.name);
            if (map == null) return;
            InputAction action = map.FindAction(actionReference.action.name);
            if (action == null) return;

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

        private void UpdateLastUsedDevice()
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
        
        public void ProcessControlsChange(InputDevice inputDevice)
        {
            if (playerInput == null || inputDevice == null)
            {
                return;
            }

            Debug.Log($"Last used device for {ID}: {inputDevice.name}");
            LastUsedDevice = inputDevice;
            
            ControlScheme? controlSchemeNullable = ControlSchemeNameToEnum(playerInput.currentControlScheme);
            if (controlSchemeNullable == null)
            {
                return;
            }

            ControlScheme controlScheme = controlSchemeNullable.Value;
            if (controlScheme == CurrentControlScheme)
            {
                return;
            }
            
            CurrentControlScheme = controlScheme;
            OnControlSchemeChanged?.Invoke(controlScheme);
        }
        
        private static ControlScheme? ControlSchemeNameToEnum(string controlSchemeName)
        {
            return controlSchemeName switch
            {
                // MARKER.ControlSchemeSwitch.Start
                "Keyboard&Mouse" => ControlScheme.KeyboardMouse,
                "Gamepad" => ControlScheme.Gamepad,
                "Touch" => ControlScheme.Touch,
                "Joystick" => ControlScheme.Joystick,
                "XR" => ControlScheme.XR,
                // MARKER.ControlSchemeSwitch.End
                _ => null
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
    }
}
