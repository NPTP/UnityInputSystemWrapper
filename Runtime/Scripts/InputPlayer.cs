using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityInputSystemWrapper.Generated.MapActions;
using UnityInputSystemWrapper.Generated.MapCaches;
using Object = UnityEngine.Object;

namespace UnityInputSystemWrapper
{
    public class InputPlayer
    {
        #region Field & Properties
        
        public event Action<ControlScheme> OnControlSchemeChanged;
        public event Action<char> OnKeyboardTextInput;

        public int ID { get; }
        public InputContext CurrentContext { get; private set; }
        public ControlScheme CurrentControlScheme { get; private set; }
        public PlayerInput PlayerInput
        {
            set
            {
                if (playerInput == value) return;
                if (playerInput != null) playerInput.onControlsChanged -= HandleControlsChanged;
                playerInput = value;
                if (playerInput != null) playerInput.onControlsChanged += HandleControlsChanged;
            }
        }
        public InputSystemUIInputModule UIInputModule
        {
            set
            {
                uiInputModule = value;
                if (playerInput != null) playerInput.uiInputModule = value;
            }
        }
        
        // MARKER.MapActionsProperties.Start
        public PlayerActions Player { get; }
        public UIActions UI { get; }
        // MARKER.MapActionsProperties.End
        
        // MARKER.MapCacheFields.Start
        private readonly PlayerMapCache playerMap;
        private readonly UIMapCache uIMap;
        // MARKER.MapCacheFields.End
        
        private readonly InputActionAsset asset;
        private PlayerInput playerInput;
        private InputSystemUIInputModule uiInputModule;
        
        #endregion

        public InputPlayer(InputActionAsset asset, int id)
        {
            this.asset = asset;
            ID = id;

            // MARKER.MapAndActionsInstantiation.Start
            Player = new PlayerActions();
            playerMap = new PlayerMapCache(asset);
            UI = new UIActions();
            uIMap = new UIMapCache(asset);
            // MARKER.MapAndActionsInstantiation.End
        }

        #region Public Interface
        
        public void Terminate()
        {
            asset.Disable();
            DisableKeyboardTextInput();
            RemoveAllMapActionCallbacks();
            Object.Destroy(playerInput);
            Object.Destroy(uiInputModule);
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
        
        public void EnableContext(InputContext context)
        {
            CurrentContext = context;
            EnableMapsForContext(context);
        }
        
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

        private void HandleControlsChanged(PlayerInput pi)
        {
            ControlScheme? controlSchemeNullable = ControlSchemeNameToEnum(pi.currentControlScheme);
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
            switch (context)
            {
                // MARKER.EnableContextSwitchMembers.Start
                case InputContext.AllInputDisabled:
                    DisableKeyboardTextInput();
                    playerMap.Disable();
                    playerMap.RemoveCallbacks(Player);
                    uIMap.Disable();
                    uIMap.RemoveCallbacks(UI);
                    SetUIEventSystemActions(null, null, null, null, null, null, null, null, null, null);
                    break;
                case InputContext.Player:
                    DisableKeyboardTextInput();
                    playerMap.Enable();
                    playerMap.AddCallbacks(Player);
                    uIMap.Disable();
                    uIMap.RemoveCallbacks(UI);
                    SetUIEventSystemActions(null, null, null, null, null, null, null, null, null, null);
                    break;
                case InputContext.UI:
                    EnableKeyboardTextInput();
                    playerMap.Disable();
                    playerMap.RemoveCallbacks(Player);
                    uIMap.Enable();
                    uIMap.AddCallbacks(UI);
                    SetUIEventSystemActions(null, null, null, null, null, null, null, null, null, null);
                    break;
                // MARKER.EnableContextSwitchMembers.End
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            }
        }
        
        private void SetUIEventSystemActions(
            InputAction point,
            InputAction leftClick,
            InputAction middleClick,
            InputAction rightClick,
            InputAction scrollWheel,
            InputAction move,
            InputAction submit,
            InputAction cancel,
            InputAction trackedDevicePosition,
            InputAction trackedDeviceOrientation)
        {
            if (uiInputModule == null || !Input.UseContextEventSystemActions)
            {
                return;
            }
            
            uiInputModule.point = InputActionReference.Create(point);
            uiInputModule.leftClick = InputActionReference.Create(leftClick);
            uiInputModule.middleClick = InputActionReference.Create(middleClick);
            uiInputModule.rightClick = InputActionReference.Create(rightClick);
            uiInputModule.scrollWheel = InputActionReference.Create(scrollWheel);
            uiInputModule.move = InputActionReference.Create(move);
            uiInputModule.submit = InputActionReference.Create(submit);
            uiInputModule.cancel = InputActionReference.Create(cancel);
            uiInputModule.trackedDevicePosition = InputActionReference.Create(trackedDevicePosition);
            uiInputModule.trackedDeviceOrientation = InputActionReference.Create(trackedDeviceOrientation);
        }
        
        #endregion
    }
}
