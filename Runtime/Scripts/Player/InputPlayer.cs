using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.AnyButtonPress;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using Object = UnityEngine.Object;

namespace NPTP.InputSystemWrapper.Player
{
    public sealed class InputPlayer
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
                    InputContextId = inputContextId;
                else
                    Asset.Disable();
                // UpdateLastUsedDevice();
                OnEnabledOrDisabled?.Invoke(this);
            }
        }

        private InputContextId inputContextId;
        internal InputContextId InputContextId
        {
            get => inputContextId;
            set
            {
                inputContextId = value;
                EnableMapsForContext(value);
#if UNITY_EDITOR
                EDITOR_OnInputContextChanged?.Invoke(this);
#endif
            }
        }

        public int ID { get; }

        private ControlSchemeId currentControlSchemeId = ControlSchemeId.None;
        internal ControlSchemeId CurrentControlSchemeId
        {
            get => currentControlSchemeId;
            private set
            {
                if (currentControlSchemeId == value)
                    return;

                currentControlSchemeId = value;
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

        /// <summary>Where the virtual mouse hangs, so it goes away with the player.</summary>
        internal Transform PlayerInputTransform => playerInputGameObject == null ? null : playerInputGameObject.transform;

        internal InputUser User => playerInput == null ? default : playerInput.user;

        private PlayerVirtualMouse virtualMouse;

        /// <summary>
        /// A mouse this player drives with the actions of the virtual mouse map, for pointing at a UI with
        /// a gamepad. Off until it is switched on.
        /// </summary>
        internal PlayerVirtualMouse VirtualMouse => virtualMouse ??= new PlayerVirtualMouse(this, inputData);

        internal Dictionary<Guid, ActionWrapper> ActionWrapperTable => actionWrapperTable;

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

        private readonly Dictionary<string, IActionMapWrapper> actionMapWrappers = new();

        /// <summary>
        /// Set once by the generated code, which is the only place that knows the concrete actions types.
        /// Invoked for each new player to fill its action map wrapper table, keyed by action map name.
        /// </summary>
        internal static Action<InputPlayer, Dictionary<string, IActionMapWrapper>> ActionMapWrapperFactory;
        private InputData inputData;

        #endregion

        #region Setup & Teardown

        internal void Terminate()
        {
            virtualMouse?.Disable();
            Enabled = false;
            anyButtonPressListenerCollection?.Clear();
            DisableKeyboardTextInput();
            DisableAllMapsAndRemoveCallbacks();
        }

        internal InputPlayer(InputData inputData, int id, bool isMultiplayer, Transform parent)
        {
            this.inputData = inputData;
            Asset = InstantiateNewActions(inputData.InputActionAsset);
            ID = id;

            ActionMapWrapperFactory?.Invoke(this, actionMapWrappers);

            SetUpInputPlayerGameObject(isMultiplayer, parent);
            PopulateEventSystemActionsPool();

            // Input context gets set by the runtime after this instantiation, which sets up maps & event
            // system actions/overrides, so we don't have to handle that here.
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
                name = $"Player[{ID}]Input",
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

            // TODO: Unity means to add a "None" behavior to the InputSystem which we will use once it's available.
            // This is because any events here are unnecessary overhead that we don't use.
            // C# events are just the lowest overhead in the meantime.
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

            // Set this manually because the initial control scheme gets set before we are able to respond to it with event handlers.
            CurrentControlSchemeId = inputData.GetControlSchemeId(playerInput.currentControlScheme);
        }

        private void SetEventSystemOptions()
        {
            EventSystemOptions options = inputData.EventSystemOptions;
            if (options == null)
            {
                return;
            }

            uiInputModule.moveRepeatDelay = options.MoveRepeatDelay;
            uiInputModule.moveRepeatRate = options.MoveRepeatRate;
            uiInputModule.deselectOnBackgroundClick = options.DeselectOnBackgroundClick;
            uiInputModule.pointerBehavior = options.PointerBehavior;
            uiInputModule.cursorLockBehavior = options.CursorLockBehavior;
        }

        /// <summary>
        /// Adds all default and override event system InputActionReferences to a shared pool to
        /// reduce duplication and lookup time.
        /// </summary>
        private void PopulateEventSystemActionsPool()
        {
            if (inputData.EventSystemOptions != null)
            {
                foreach (EventSystemActionBinding binding in inputData.EventSystemOptions.DefaultActions)
                    AddToEventSystemActionsPool(binding.ActionID);
            }

            if (inputData.ContextDefinitions == null)
            {
                return;
            }

            foreach (InputContextDefinition contextDefinition in inputData.ContextDefinitions)
                foreach (EventSystemActionBinding binding in contextDefinition.EventSystemActionOverrides)
                    AddToEventSystemActionsPool(binding.ActionID);
        }

        private void AddToEventSystemActionsPool(string actionID)
        {
            if (string.IsNullOrEmpty(actionID) || eventSystemActionsPool.ContainsKey(actionID))
            {
                return;
            }

            eventSystemActionsPool.Add(actionID, CreateInputActionReferenceToPlayerAsset(actionID));
        }

        private InputActionReference GetPooledEventSystemAction(string actionID)
        {
            return string.IsNullOrEmpty(actionID) || !eventSystemActionsPool.TryGetValue(actionID, out InputActionReference reference)
                ? null
                : reference;
        }

        private void SetDefaultEventSystemActions()
        {
            if (inputData.EventSystemOptions == null)
            {
                return;
            }

            foreach (EventSystemActionBinding binding in inputData.EventSystemOptions.DefaultActions)
                ApplyEventSystemAction(binding.ActionType, GetPooledEventSystemAction(binding.ActionID));
        }

        private void ApplyEventSystemAction(EventSystemActionType actionType, InputActionReference reference)
        {
            switch (actionType)
            {
                case EventSystemActionType.Point: uiInputModule.point = reference; break;
                case EventSystemActionType.LeftClick: uiInputModule.leftClick = reference; break;
                case EventSystemActionType.MiddleClick: uiInputModule.middleClick = reference; break;
                case EventSystemActionType.RightClick: uiInputModule.rightClick = reference; break;
                case EventSystemActionType.ScrollWheel: uiInputModule.scrollWheel = reference; break;
                case EventSystemActionType.Move: uiInputModule.move = reference; break;
                case EventSystemActionType.Submit: uiInputModule.submit = reference; break;
                case EventSystemActionType.Cancel: uiInputModule.cancel = reference; break;
                case EventSystemActionType.TrackedDevicePosition: uiInputModule.trackedDevicePosition = reference; break;
                case EventSystemActionType.TrackedDeviceOrientation: uiInputModule.trackedDeviceOrientation = reference; break;
                default: throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }
        }

        /// <summary>
        /// Point this player's event system at a virtual mouse's own actions, so its pointer drives their UI.
        /// </summary>
        internal void ApplyVirtualMousePointerActions(VirtualMousePointerActions pointerActions)
        {
            ApplyEventSystemAction(EventSystemActionType.Point, pointerActions.Point);
            ApplyEventSystemAction(EventSystemActionType.LeftClick, pointerActions.LeftClick);
            ApplyEventSystemAction(EventSystemActionType.RightClick, pointerActions.RightClick);
            ApplyEventSystemAction(EventSystemActionType.MiddleClick, pointerActions.MiddleClick);
            ApplyEventSystemAction(EventSystemActionType.ScrollWheel, pointerActions.ScrollWheel);
        }

        /// <summary>
        /// Put the event system back on the player's own actions: the defaults, then whatever the context
        /// they are in overrides, which is the same order a context change applies them in.
        /// </summary>
        internal void RestoreEventSystemActions()
        {
            SetDefaultEventSystemActions();

            InputContextDefinition contextDefinition = inputData.GetContextDefinition(inputContextId);
            if (contextDefinition == null)
            {
                return;
            }

            foreach (EventSystemActionBinding binding in contextDefinition.EventSystemActionOverrides)
            {
                ApplyEventSystemAction(binding.ActionType, GetPooledEventSystemAction(binding.ActionID));
            }
        }

        private void DisableAllMapsAndRemoveCallbacks()
        {
            foreach (IActionMapWrapper actionMapWrapper in actionMapWrappers.Values)
                actionMapWrapper.DisableAndUnregisterCallbacks();
        }

        private void EnableMapsForContext(InputContextId context)
        {
            if (!Enabled)
            {
                return;
            }

            SetDefaultEventSystemActions();

            InputContextDefinition contextDefinition = inputData.GetContextDefinition(context);
            if (contextDefinition == null)
            {
                throw new ArgumentOutOfRangeException(nameof(context), context, $"No {nameof(InputContextDefinition)} exists for this context. Re-run input code generation.");
            }

            if (contextDefinition.EnableKeyboardTextInput) EnableKeyboardTextInput();
            else DisableKeyboardTextInput();

            foreach (KeyValuePair<string, IActionMapWrapper> pair in actionMapWrappers)
            {
                if (Array.IndexOf(contextDefinition.ActiveMapNames, pair.Key) >= 0)
                    pair.Value.EnableAndRegisterCallbacks();
                else
                    pair.Value.DisableAndUnregisterCallbacks();
            }

            foreach (EventSystemActionBinding binding in contextDefinition.EventSystemActionOverrides)
                ApplyEventSystemAction(binding.ActionType, GetPooledEventSystemAction(binding.ActionID));
        }


        private InputActionReference CreateInputActionReferenceToPlayerAsset(string actionID)
        {
            return string.IsNullOrEmpty(actionID)
                ? null
                : InputActionReference.Create(Asset.FindAction(actionID, throwIfNotFound: false));
        }

        #endregion

        #region Internal

        /// <summary>
        /// Get the actions object for one action map by its name in the input action asset.
        /// The generated extension methods on this type are the type-safe way in.
        /// </summary>
        internal IActionMapWrapper GetActionMapWrapper(string mapName)
        {
            return actionMapWrappers.TryGetValue(mapName, out IActionMapWrapper wrapper) ? wrapper : null;
        }

        internal bool ControlSchemeHas<TDevice>(ControlSchemeId controlSchemeId) where TDevice : InputDevice
        {
            for (int i = 0; i < Asset.controlSchemes.Count; i++)
            {
                InputControlScheme inputControlScheme = Asset.controlSchemes[i];
                if (inputControlScheme.name != controlSchemeId.Name)
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
                    CurrentControlSchemeId = inputData.GetControlSchemeId(playerInput.currentControlScheme);
                    break;
            }

            OnInputUserChange?.Invoke(new InputUserChangeInfo(this, inputUserChange));
        }

        /// <summary>
        /// Whether this player is driving a mouse with the virtual mouse map's actions, for pointing at a
        /// UI with a gamepad. The cursor and how it behaves are set on the input data.
        /// </summary>
        public bool VirtualMouseEnabled => virtualMouse != null && virtualMouse.Enabled;

        /// <summary>
        /// Where this player's virtual mouse is, in screen pixels, or zero while they are not driving one.
        /// Separate from the system mouse's position, which it does not touch.
        /// </summary>
        public Vector2 VirtualMousePosition => virtualMouse == null ? Vector2.zero : virtualMouse.Position;

        /// <summary>
        /// Start driving a mouse from this player's virtual mouse actions. The cursor is put under the
        /// given parent, or left at the scene's root when there is none.
        /// </summary>
        public void EnableVirtualMouse(RectTransform cursorParent = null)
        {
            VirtualMouse.Enable(cursorParent);
        }

        /// <summary>Stop driving a virtual mouse and take its device away.</summary>
        public void DisableVirtualMouse()
        {
            virtualMouse?.Disable();
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
