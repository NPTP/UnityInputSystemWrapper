using System;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.AnyButtonPress;
using NPTP.InputSystemWrapper.Bindings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.CustomSetups;
using NPTP.InputSystemWrapper.Player;
using NPTP.InputSystemWrapper.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NPTP.InputSystemWrapper
{
    /// <summary>
    /// Owns all input state for the game. A single ambient instance is created on initialization and
    /// reached through <see cref="Current"/>; the generated ISW class is the public facade over it.
    /// </summary>
    internal sealed class InputRuntime
    {
        internal static InputRuntime Current { get; private set; }

        #region Fields & Properties

        private const string INPUT_DATA_RESOURCES_PATH = "InputData";

        /// <summary>
        /// For use with any localization system in your project: handle this event by taking the passed request,
        /// using localizationKey to find the right string in your localization system, and setting localizedDisplayName
        /// to that string.
        /// </summary>
        public event Action<LocalizedStringRequest> OnLocalizedStringRequested;

        /// <summary>
        /// Raised when a player's bindings are saved, if the binding serialization mode includes Event.
        /// Handle it by storing the request's JSON against its player ID.
        /// </summary>
        public event Action<BindingsSaveRequest> OnBindingsSaveRequested;

        /// <summary>
        /// Raised when a player's bindings are loaded, if the binding serialization mode includes Event.
        /// Handle it by setting the request's json to what was last saved for its player ID.
        /// </summary>
        public event Action<BindingsLoadRequest> OnBindingsLoadRequested;

        /// <summary>
        /// Use as a general purpose catch-all for when to update any UI that displays controls.
        /// Invoked on InputUserChange, on ControlScheme change, and on bindings changed.
        /// </summary>
        public event Action OnControlsUpdated;

        /// <summary>
        /// Invoked on any button pressed on any connected device regardless of actions mapped, assets enabled, etc.
        /// </summary>
        public event AnyButtonPressListener OnAnyButtonPress
        {
            add => anyButtonPressListenerCollection.Add(value);
            remove => anyButtonPressListenerCollection.Remove(value);
        }

        // TODO (architecture): Shortcoming here. OnInputUserChange doesn't always get called when a binding changes, so we have this as well.
        // Can we consolidate these events into a higher-level abstraction? Or separate them by desired events (binding change, control scheme change, etc with more granularity)
        public event Action OnBindingsChanged;
        public event Action<InputUserChangeInfo> OnAnyPlayerInputUserChange;
        public event Action<InputPlayer> OnAnyPlayerControlSchemeChanged;
        public event Action<char> OnAnyPlayerKeyboardTextInput;

        private bool allowPlayerJoining;
        public bool AllowPlayerJoining
        {
            get => allowPlayerJoining;
            set
            {
                if (value == allowPlayerJoining)
                    return;

                allowPlayerJoining = value;
                playerCollection.SetMultiplayer(value);
                if (value) OnAnyButtonPress += JoinPlayerByActivatedInputControl;
                else OnAnyButtonPress -= JoinPlayerByActivatedInputControl;
            }
        }

        public Vector2 MousePosition => Mouse.current.position.ReadValue();

        internal InputPlayer DefaultPlayer => playerCollection.DefaultPlayer;

        private bool initialized;
        private InputPlayerCollection playerCollection;
        private InputData inputData;
        private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
        private AnyButtonPressListenerCollection anyButtonPressListenerCollection;

        #endregion

        #region Setup

        internal static bool PlayerExists(int playerID) => Current != null && Current.DoesPlayerExist(playerID);

        internal static void Initialize()
        {
            // Current is set before anything is set up, because setting up reaches back through it: a
            // player's saved bindings can be asked for through an event, which is broadcast from here.
            InputRuntime runtime = new();
            Current = runtime;
            runtime.SetUp();
        }

        private InputRuntime()
        {
            SetUpQuittingConditions();

            inputData = Resources.Load<InputData>(INPUT_DATA_RESOURCES_PATH);
            if (inputData == null || inputData.InputActionAsset == null)
            {
                throw new Exception($"{nameof(InputData)} is null or its input action asset is null - input will not work! Did you move the asset from its original location in 'Resources'?");
            }
        }

        private void SetUp()
        {
            // Clear out anything in the scene that would interfere with the ISW's autonomous operation.
            ObjectUtility.DestroyObjectsOfType<PlayerInput, InputSystemUIInputModule, StandaloneInputModule, EventSystem>();

            // These registrations must occur before players get assigned InputActionAssets, or else issues resolving the bindings will arise.
            CustomSetupsRegisterer.PerformRegistrations(inputData);

            playerCollection = new InputPlayerCollection(inputData, HandlePlayerAdded, HandlePlayerRemoved);
#if UNITY_EDITOR
            playerCollection.EDITOR_OnPlayerInputContextChanged += EDITOR_HandlePlayerInputContextChanged;
#endif
            UpdateAfterPlayerCollectionChange();
            if (inputData.LoadAllBindingOverridesOnInitialize) LoadBindingsForAllPlayers();
            playerCollection.SetContextForAll(inputData.DefaultContextId);

            anyButtonPressListenerCollection = new AnyButtonPressListenerCollection();
            ++InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange += HandleInputUserChange;
            OnAnyPlayerInputUserChange += BroadcastControlsUpdated;
            OnBindingsChanged += BroadcastControlsUpdated;
            OnAnyPlayerControlSchemeChanged += BroadcastControlsUpdated;

            initialized = true;
        }

        private void SetUpQuittingConditions()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= handlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += handlePlayModeStateChanged;
            void handlePlayModeStateChanged(PlayModeStateChange playModeStateChange)
            {
                if (playModeStateChange is PlayModeStateChange.ExitingPlayMode)
                {
                    OnQuitting();
                }
            }
#endif
        }

        private void OnQuitting()
        {
#if UNITY_EDITOR
            anyButtonPressListenerCollection.Clear();
            playerCollection.EDITOR_OnPlayerInputContextChanged -= EDITOR_HandlePlayerInputContextChanged;
            OnAnyPlayerInputUserChange -= BroadcastControlsUpdated;
            OnBindingsChanged -= BroadcastControlsUpdated;
            OnAnyPlayerControlSchemeChanged -= BroadcastControlsUpdated;
            playerCollection.Terminate();
            --InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange -= HandleInputUserChange;
#endif
        }

        #endregion

        #region Public Interface

        public InputPlayer GetPlayer(int playerID)
        {
            return playerCollection.GetOrAdd(playerID);
        }

        public void RemovePlayer(int playerID)
        {
            playerCollection.Remove(playerID);
        }

        internal bool ControlSchemeHas<TDevice>(ControlSchemeId controlSchemeId, int playerID = 0) where TDevice : InputDevice
        {
            return GetPlayer(playerID).ControlSchemeHas<TDevice>(controlSchemeId);
        }

        internal void SetContextForAllPlayers(InputContextId inputContextId)
        {
            playerCollection.SetContextForAll(inputContextId);
        }

        /// <summary>
        /// Try to get the ActionWrapper for the (deprecated) InputActionReference's action.
        /// Useful as a transitional tool from normal Unity Input System usage to full ISW integration.
        /// </summary>
        // TODO: remove this method eventually
        public bool TryConvert(InputActionReference inputActionReference, int playerID, out ActionWrapper actionWrapper)
        {
            if (inputActionReference != null && inputActionReference.action != null)
            {
                InputPlayer player = playerCollection.GetOrAdd(playerID);
                return player.TryGetMatchingActionWrapper(inputActionReference.action, out actionWrapper);
            }

            actionWrapper = null;
            return false;
        }

        /// <summary>
        /// Single-player overload
        /// </summary>
        public bool TryConvert(InputActionReference inputActionReference, out ActionWrapper actionWrapper)
        {
            return TryConvert(inputActionReference, 0, out actionWrapper);
        }

        /// <summary>Put one of an action's slots back to its default, leaving its others alone.</summary>
        internal void ResetBindingForAction(ActionWrapper actionWrapper, CompositePart compositePart, ControlSchemeId controlSchemeId, int uiIndex)
        {
            if (actionWrapper == null)
            {
                return;
            }

            BindingChanger.ResetBindingToDefaultForSlot(inputData, new ActionBindingInfo(actionWrapper, compositePart, controlSchemeId, uiIndex));
        }

        internal void ResetBindingForAction(ActionReference actionReference, ControlSchemeId controlSchemeId, int uiIndex)
        {
            ResetBindingForAction(actionReference?.ActionWrapper, GetCompositePart(actionReference), controlSchemeId, uiIndex);
        }

        /// <summary>Put every one of an action's bindings on this control scheme back to its default.</summary>
        internal void ResetAllBindingsForAction(ActionWrapper actionWrapper, CompositePart compositePart, ControlSchemeId controlSchemeId)
        {
            if (actionWrapper == null)
            {
                return;
            }

            BindingChanger.ResetBindingToDefaultForControlScheme(new ActionBindingInfo(actionWrapper, compositePart, controlSchemeId), controlSchemeId);
        }

        internal void ResetAllBindingsForAction(ActionReference actionReference, ControlSchemeId controlSchemeId)
        {
            ResetAllBindingsForAction(actionReference?.ActionWrapper, GetCompositePart(actionReference), controlSchemeId);
        }

        // An ActionReference carries its own composite part and player ID.
        private static CompositePart GetCompositePart(ActionReference actionReference)
        {
            return actionReference == null ? CompositePart.DontIsolatePart : actionReference.CompositePart;
        }

        internal void ResetAllBindingsForControlScheme(ControlSchemeId controlSchemeId, int? playerID = null)
        {
            if (playerID.HasValue)
                BindingChanger.ResetBindingsToDefaultForControlScheme(GetPlayer(playerID.Value).Asset, controlSchemeId);
            else foreach (InputPlayer player in playerCollection)
                    BindingChanger.ResetBindingsToDefaultForControlScheme(player.Asset, controlSchemeId);
        }

        public void LoadAllBindings(int? playerID = null)
        {
            if (playerID.HasValue)
                BindingSaveLoad.LoadBindingsForPlayer(GetPlayer(playerID.Value), inputData.BindingSerializationMode);
            else foreach (InputPlayer player in playerCollection)
                    BindingSaveLoad.LoadBindingsForPlayer(player, inputData.BindingSerializationMode);
        }

        public void SaveAllBindings(int? playerID = null)
        {
            if (playerID.HasValue)
                BindingSaveLoad.SaveBindingsForPlayer(GetPlayer(playerID.Value), inputData.BindingSerializationMode);
            else foreach (InputPlayer player in playerCollection)
                    BindingSaveLoad.SaveBindingsForPlayer(player, inputData.BindingSerializationMode);
        }

        public void ResetAllBindings(int? playerID = 0)
        {
            if (playerID.HasValue)
                BindingChanger.ResetBindingsToDefault(GetPlayer(playerID.Value).Asset);
            else foreach (InputPlayer player in playerCollection)
                    BindingChanger.ResetBindingsToDefault(player.Asset);
        }

        #endregion

        #region Internal Interface

        internal ControlSchemeId GetControlSchemeId(int index) => inputData.GetControlSchemeId(index);

        internal void BroadcastLocalizedStringRequested(LocalizedStringRequest localizedStringRequest)
        {
            OnLocalizedStringRequested?.Invoke(localizedStringRequest);
        }

        internal void BroadcastBindingsChanged()
        {
            OnBindingsChanged?.Invoke();
        }

        internal void BroadcastBindingsSaveRequested(BindingsSaveRequest request)
        {
            OnBindingsSaveRequested?.Invoke(request);
        }

        internal void BroadcastBindingsLoadRequested(BindingsLoadRequest request)
        {
            OnBindingsLoadRequested?.Invoke(request);
        }

        private void BroadcastControlsUpdated(InputUserChangeInfo inputUserChangeInfo) => BroadcastControlsUpdated();
        private void BroadcastControlsUpdated(InputPlayer inputPlayer) => BroadcastControlsUpdated();
        private void BroadcastControlsUpdated()
        {
            OnControlsUpdated?.Invoke();
        }

        /// <summary>
        /// Start an interactive rebind: wait for input from the given player and device to bind a new control to the action given in the action reference.
        /// </summary>
        /// <param name="actionBindingInfo">ActionInfo struct containing information pertinent to rebinding.</param>
        /// <param name="callback">Callback on rebind cancel/complete. Note that this callback will be invoked whether or not the binding was actually changed,
        /// and even if the rebind fails to execute. It is intended to help you manage control flow on your UI or wherever rebinding is happening.
        /// (Subscribe to Input.OnBindingsChanged to know when a binding has actually been set to a new value.)</param>
        internal void StartInteractiveRebind(ActionBindingInfo actionBindingInfo, Action<RebindInfo> callback = null)
        {
            if (rebindingOperation != null)
            {
                rebindingOperation.Cancel();
                rebindingOperation.Dispose();
            }

            if (TryGetBindingIndexToRebind(actionBindingInfo, out int bindingIndex))
            {
                rebindingOperation = BindingChanger.StartInteractiveRebind(inputData, actionBindingInfo, bindingIndex, callback);
            }
            else
            {
                rebindingOperation?.Dispose();
                rebindingOperation = null;
                callback?.Invoke(new RebindInfo(actionBindingInfo.ActionWrapper, RebindInfo.Status.Failed, BindingSlots.Empty));
            }
        }

        /// <summary>
        /// The binding a rebind writes to: the slot at the requested UI index, narrowed to one part if
        /// that slot is a composite.
        /// </summary>
        private bool TryGetBindingIndexToRebind(ActionBindingInfo actionBindingInfo, out int bindingIndex)
        {
            bindingIndex = -1;
            InputAction action = actionBindingInfo.ActionWrapper.InputAction;
            BindingSlots bindingSlots = BindingSlots.Resolve(inputData, action, actionBindingInfo.ControlSchemeId);

            if (!bindingSlots.TryGetAtUIIndex(actionBindingInfo.UIIndex, out BindingSlot bindingSlot))
            {
                return false;
            }

            if (bindingSlot.IsComposite && actionBindingInfo.DontUseCompositePart)
            {
                ISWDebug.LogError($"Binding at UI index {actionBindingInfo.UIIndex} of action {action.name} is a composite, " +
                                  "which has to be rebound one part at a time. Specify a composite part.");
                return false;
            }

            if (!bindingSlot.TryGetBindingIndexForPart(action, actionBindingInfo.CompositePart, out bindingIndex))
            {
                ISWDebug.LogError($"Binding at UI index {actionBindingInfo.UIIndex} of action {action.name} has no " +
                                  $"{actionBindingInfo.CompositePart} part.");
                return false;
            }

            return true;
        }

        /// <summary>The slots of an action on whichever control scheme the player is currently using.</summary>
        internal BindingSlots GetCurrentBindingSlots(ActionWrapper actionWrapper)
        {
            return !playerCollection.TryGetPlayer(actionWrapper.PlayerID, out InputPlayer player)
                ? BindingSlots.Empty
                : GetBindingSlots(actionWrapper, player.CurrentControlSchemeId);
        }

        internal BindingSlots GetBindingSlots(ActionWrapper actionWrapper, ControlSchemeId controlSchemeId)
        {
            return BindingSlots.Resolve(inputData, actionWrapper.InputAction, controlSchemeId);
        }

        /// <summary>
        /// The slots of an action on the player's current control scheme, loaded in the background.
        /// </summary>
        internal void GetCurrentBindingSlotsAsync(ActionWrapper actionWrapper, Action<BindingSlots> onResolved)
        {
            if (!playerCollection.TryGetPlayer(actionWrapper.PlayerID, out InputPlayer player))
            {
                onResolved?.Invoke(BindingSlots.Empty);
                return;
            }

            BindingSlots.ResolveAsync(inputData, actionWrapper.InputAction, player.CurrentControlSchemeId, onResolved);
        }

        internal bool TryGetActionWrapper(int playerID, InputAction inputAction, out ActionWrapper actionWrapper)
        {
            return GetPlayer(playerID).TryGetMatchingActionWrapper(inputAction, out actionWrapper);
        }

        internal bool DoesPlayerExist(int playerID)
        {
            return playerCollection.TryGetPlayer(playerID, out _);
        }

        #endregion

        #region Private Runtime Functionality

        private void HandleAnyPlayerInputUserChange(InputUserChangeInfo inputUserChangeInfo)
        {
            OnAnyPlayerInputUserChange?.Invoke(inputUserChangeInfo);
        }

        private void HandleAnyPlayerControlSchemeChanged(InputPlayer inputPlayer)
        {
            OnAnyPlayerControlSchemeChanged?.Invoke(inputPlayer);
        }

        private void HandleAnyPlayerKeyboardTextInput(char c)
        {
            OnAnyPlayerKeyboardTextInput?.Invoke(c);
        }

        private void HandlePlayerAdded(InputPlayer inputPlayer) => UpdateAfterPlayerCollectionChange();
        private void HandlePlayerRemoved(int playerID) => UpdateAfterPlayerCollectionChange();

        private void UpdateAfterPlayerCollectionChange()
        {
            foreach (InputPlayer player in playerCollection)
            {
                player.OnInputUserChange -= HandleAnyPlayerInputUserChange;
                player.OnInputUserChange += HandleAnyPlayerInputUserChange;

                player.OnControlSchemeChanged -= HandleAnyPlayerControlSchemeChanged;
                player.OnControlSchemeChanged += HandleAnyPlayerControlSchemeChanged;

                player.OnKeyboardTextInput -= HandleAnyPlayerKeyboardTextInput;
                player.OnKeyboardTextInput += HandleAnyPlayerKeyboardTextInput;
            }
        }

        private void LoadBindingsForAllPlayers()
        {
            foreach (InputPlayer player in playerCollection)
            {
                BindingSaveLoad.LoadBindingsForPlayer(player, inputData.BindingSerializationMode);
            }
        }

        private void JoinPlayerByActivatedInputControl(InputControl inputControl)
        {
            InputDevice device = inputControl.device;

            if (device == null)
            {
                Debug.Log("Device is null");
                return;
            }

            // Mouse + Keyboard is always joined.
            if (device is Mouse or Keyboard)
            {
                Debug.Log("Device is MKB");
                return;
            }

            // Any devices already in use can't be stolen.
            if (playerCollection.IsDeviceLastUsedByAnyPlayer(device))
            {
                Debug.Log($"Already using {device.name}");
                return;
            }

            // Allow stealing a device paired to, but currently unused by, another player.
            if (playerCollection.TryGetPlayerPairedWithDevice(device, out InputPlayer pairedPlayer))
            {
                Debug.Log($"Unpairing {device.name} from player {pairedPlayer.ID}");
                pairedPlayer.UnpairDevice(device);
            }

            // Find a player to pair the device to.
            if (playerCollection.TryPairDeviceToFirstDisabledPlayer(device, out InputPlayer disabledPlayer))
            {
                Debug.Log("Paired to disabled player");
                disabledPlayer.Enabled = true;
                return;
            }

            // If no disabled players exist, create and pair to a new player.
            playerCollection.PairDeviceToNewPlayer(device);
            Debug.Log("Paired to new player");
        }

        private void HandleInputUserChange(InputUser inputUser, InputUserChange inputUserChange, InputDevice inputDevice)
        {
            playerCollection.HandleInputUserChange(inputUser, inputUserChange, inputDevice);
        }

        #endregion

        #region Editor-Only Debug
#if UNITY_EDITOR
        internal static event Action<int, InputContextId> EDITOR_OnPlayerInputContextChanged;

        internal bool EDITOR_IsInitialized => initialized;
        internal InputContextId EDITOR_GetDefaultContext() => inputData.DefaultContextId;
        internal string EDITOR_GetContextName(InputContextId id) => inputData.GetContextDefinition(id)?.Name ?? id.Index.ToString();

        internal bool EDITOR_TryGetPlayer(int playerID, out InputPlayer inputPlayer)
        {
            if (playerCollection == null)
            {
                inputPlayer = default;
                return false;
            }

            inputPlayer = playerCollection.GetOrAdd(playerID);
            return true;
        }

        private void EDITOR_HandlePlayerInputContextChanged(InputPlayer inputPlayer)
        {
            EDITOR_OnPlayerInputContextChanged?.Invoke(inputPlayer.ID, inputPlayer.InputContextId);
        }
#endif
        #endregion
    }
}
