using System;
using System.Collections.Generic;
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
    /// Main point of usage for all input in the game.
    /// ISW stands for "Input System Wrapper".
    /// </summary>
    public static partial class ISW
    {
        #region Fields & Properties

        private const string RUNTIME_INPUT_DATA_RESOURCES_PATH = "RuntimeInputData";
        
        /// <summary>
        /// For use with any localization system in your project: handle this event by taking the passed request,
        /// using localizationKey to find the right string in your localization system, and setting localizedDisplayName
        /// to that string.
        /// </summary>
        public static event Action<LocalizedStringRequest> OnLocalizedStringRequested;

        /// <summary>
        /// Use as a general purpose catch-all for when to update any UI that displays controls.
        /// Invoked on InputUserChange, on ControlScheme change, and on bindings changed.
        /// </summary>
        public static event Action OnControlsUpdated;

        /// <summary>
        /// Invoked on any button pressed on any connected device regardless of actions mapped, assets enabled, etc.
        /// </summary>
        public static event AnyButtonPressListener OnAnyButtonPress
        {
            add => anyButtonPressListenerCollection.Add(value);
            remove => anyButtonPressListenerCollection.Remove(value);
        }
        
        // TODO (architecture): Shortcoming here. OnInputUserChange doesn't always get called when a binding changes, so we have this as well.
        // Can we consolidate these events into a higher-level abstraction? Or separate them by desired events (binding change, control scheme change, etc with more granularity)
        public static event Action OnBindingsChanged;
        public static event Action<InputUserChangeInfo> OnAnyPlayerInputUserChange;
        public static event Action<InputPlayer> OnAnyPlayerControlSchemeChanged;
        public static event Action<char> OnAnyPlayerKeyboardTextInput;

        private static bool allowPlayerJoining;
        public static bool AllowPlayerJoining
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
        
        public static Vector2 MousePosition => Mouse.current.position.ReadValue();
        
        private static InputPlayer DefaultPlayer => playerCollection.DefaultPlayer;

        private static bool initialized;
        private static InputPlayerCollection playerCollection;
        private static RuntimeInputData runtimeInputData;
        private static InputActionRebindingExtensions.RebindingOperation rebindingOperation;
        private static AnyButtonPressListenerCollection anyButtonPressListenerCollection;
        
        #endregion

        #region Setup

        private static void InitializationProcess()
        {
            if (initialized)
            {
                return;
            }
            
            // Allows input system to work even when domain reload is disabled in editor.
            if (RuntimeSafeEditorUtility.IsDomainReloadDisabled())
            {
                ReflectionUtility.ResetStaticClassMembersToDefault(typeof(ISW));
            }
            
            SetUpQuittingConditions();

            runtimeInputData = Resources.Load<RuntimeInputData>(RUNTIME_INPUT_DATA_RESOURCES_PATH);
            if (runtimeInputData == null || runtimeInputData.InputActionAsset == null)
            {
                throw new Exception($"{nameof(RuntimeInputData)} is null or its input action asset is null - input will not work! Did you move the asset from its original location in 'Resources'?");
            }
            
            // Clear out anything in the scene that would interfere with the ISW's autonomous operation.
            ObjectUtility.DestroyObjectsOfType<PlayerInput, InputSystemUIInputModule, StandaloneInputModule, EventSystem>();
            
            // These registrations must occur before players get assigned InputActionAssets, or else issues resolving the bindings will arise.
            CustomSetupsRegisterer.PerformRegistrations(runtimeInputData);
            
            playerCollection = new InputPlayerCollection(runtimeInputData, HandlePlayerAdded, HandlePlayerRemoved);
#if UNITY_EDITOR
            playerCollection.EDITOR_OnPlayerInputContextChanged += EDITOR_HandlePlayerInputContextChanged;
#endif
            UpdateAfterPlayerCollectionChange();
            SetUpBindings();
            SetContextForAllPlayers(DefaultContext);
            
            anyButtonPressListenerCollection = new AnyButtonPressListenerCollection();
            ++InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange += HandleInputUserChange;
            OnAnyPlayerInputUserChange += BroadcastControlsUpdated;
            OnBindingsChanged += BroadcastControlsUpdated;
            OnAnyPlayerControlSchemeChanged += BroadcastControlsUpdated;

            initialized = true;
        }

        private static void SetUpQuittingConditions()
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

        private static void OnQuitting()
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

        public static InputPlayer GetPlayer(int playerID)
        {
            return playerCollection.GetOrAdd(playerID);
        }
        
        public static void AddPlayer(int playerID)
        {
            GetPlayer(playerID);
        }

        public static void RemovePlayer(int playerID)
        {
            playerCollection.Remove(playerID);
        }

        public static bool ControlSchemeHas<TDevice>(ControlScheme controlScheme, int playerID = 0) where TDevice : InputDevice
        {
            return GetPlayer(playerID).ControlSchemeHas<TDevice>(controlScheme);
        }

        public static void SetContextForAllPlayers(InputContext inputContext)
        {
            playerCollection.SetContextForAll(inputContext);
        }
        
        /// <summary>
        /// Try to get the ActionWrapper for the (deprecated) InputActionReference's action.
        /// Useful as a transitional tool from normal Unity Input System usage to full ISW integration.
        /// </summary>
        // TODO: remove this method eventually
        public static bool TryConvert(InputActionReference inputActionReference, int playerID, out ActionWrapper actionWrapper)
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
        public static bool TryConvert(InputActionReference inputActionReference, out ActionWrapper actionWrapper)
        {
            return TryConvert(inputActionReference, 0, out actionWrapper);
        }

        public static void ResetBindingForAction(ActionReference actionReference, ControlScheme controlScheme)
        {
            if (actionReference == null || actionReference.ActionWrapper == null)
            {
                return;
            }
            
            // Note that player ID is contained in the ActionReference.
            ActionBindingInfo actionBindingInfo = new ActionBindingInfo(actionReference.ActionWrapper, actionReference.CompositePart, controlScheme);
            BindingChanger.ResetBindingToDefaultForControlScheme(actionBindingInfo, controlScheme);
        }

        public static void ResetAllBindingsForControlScheme(ControlScheme controlScheme, int? playerID = null)
        {
            if (playerID.HasValue)
                BindingChanger.ResetBindingsToDefaultForControlScheme(GetPlayer(playerID.Value).Asset, controlScheme);
            else foreach (InputPlayer player in playerCollection)
                    BindingChanger.ResetBindingsToDefaultForControlScheme(player.Asset, controlScheme);
        }

        public static void LoadAllBindings(int? playerID = null)
        {
            if (playerID.HasValue)
                BindingSaveLoad.LoadBindingsFromDiskForPlayer(GetPlayer(playerID.Value));
            else foreach (InputPlayer player in playerCollection)
                    BindingSaveLoad.LoadBindingsFromDiskForPlayer(player);
        }

        public static void SaveAllBindings(int? playerID = null)
        {
            if (playerID.HasValue)
                BindingSaveLoad.SaveBindingsToDiskForPlayer(GetPlayer(playerID.Value));
            else foreach (InputPlayer player in playerCollection)
                    BindingSaveLoad.SaveBindingsToDiskForPlayer(player);
        }

        public static void ResetAllBindings(int? playerID = 0)
        {
            if (playerID.HasValue)
                BindingChanger.ResetBindingsToDefault(GetPlayer(playerID.Value).Asset);
            else foreach (InputPlayer player in playerCollection)
                    BindingChanger.ResetBindingsToDefault(player.Asset);
        }

        #endregion
        
        #region Internal Interface

        internal static void BroadcastLocalizedStringRequested(LocalizedStringRequest localizedStringRequest)
        {
            OnLocalizedStringRequested?.Invoke(localizedStringRequest);
        }
        
        internal static void BroadcastBindingsChanged()
        {
            OnBindingsChanged?.Invoke();
        }

        private static void BroadcastControlsUpdated(InputUserChangeInfo inputUserChangeInfo) => BroadcastControlsUpdated();
        private static void BroadcastControlsUpdated(InputPlayer inputPlayer) => BroadcastControlsUpdated();
        private static void BroadcastControlsUpdated()
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
        internal static void StartInteractiveRebind(ActionBindingInfo actionBindingInfo, Action<RebindInfo> callback = null)
        {
            if (rebindingOperation != null)
            {
                rebindingOperation.Cancel();
                rebindingOperation.Dispose();
            }

            if (BindingGetter.TryGetFirstBindingIndex(actionBindingInfo, out int bindingIndex))
            {
                rebindingOperation = BindingChanger.StartInteractiveRebind(runtimeInputData, actionBindingInfo, bindingIndex, callback);
            }
            else
            {
                ISWDebug.LogError("Rebinding failed: Action or binding index could not be found.");
                rebindingOperation?.Dispose();
                rebindingOperation = null;
                callback?.Invoke(new RebindInfo(actionBindingInfo.ActionWrapper, RebindInfo.Status.Failed, Array.Empty<BindingInfo>()));
            }
        }

        internal static bool TryGetCurrentBindingInfo(ActionWrapper actionWrapper, CompositePart compositePart, out IEnumerable<BindingInfo> bindingInfos)
        {
            if (!playerCollection.TryGetPlayer(actionWrapper.PlayerID, out InputPlayer player))
            {
                bindingInfos = default;
                return false;
            }

            ActionBindingInfo actionBindingInfo = new(actionWrapper, compositePart, player.CurrentControlScheme);
            return BindingGetter.TryGetBindingInfo(runtimeInputData, actionBindingInfo, out bindingInfos);
        }

        internal static bool TryGetBindingInfo(ActionBindingInfo actionBindingInfo, out IEnumerable<BindingInfo> bindingInfos)
        {
            return BindingGetter.TryGetBindingInfo(runtimeInputData, actionBindingInfo, out bindingInfos);
        }

        internal static bool TryGetActionWrapper(int playerID, InputAction inputAction, out ActionWrapper actionWrapper)
        {
            return GetPlayer(playerID).TryGetMatchingActionWrapper(inputAction, out actionWrapper);
        }
        
        internal static bool DoesPlayerExist(int playerID)
        {
            return playerCollection.TryGetPlayer(playerID, out _);
        }
        
        #endregion

        #region Private Runtime Functionality
        
        private static void HandleAnyPlayerInputUserChange(InputUserChangeInfo inputUserChangeInfo)
        {
            OnAnyPlayerInputUserChange?.Invoke(inputUserChangeInfo);
        }

        private static void HandleAnyPlayerControlSchemeChanged(InputPlayer inputPlayer)
        {
            OnAnyPlayerControlSchemeChanged?.Invoke(inputPlayer);
        }
        
        private static void HandleAnyPlayerKeyboardTextInput(char c)
        {
            OnAnyPlayerKeyboardTextInput?.Invoke(c);
        }
        
        private static void HandlePlayerAdded(InputPlayer inputPlayer) => UpdateAfterPlayerCollectionChange();
        private static void HandlePlayerRemoved(int playerID) => UpdateAfterPlayerCollectionChange();
        
        private static void UpdateAfterPlayerCollectionChange()
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

        private static void LoadBindingsForAllPlayers()
        {
            foreach (InputPlayer player in playerCollection)
            {
                BindingSaveLoad.LoadBindingsFromDiskForPlayer(player);
            }
        }
        
        private static void JoinPlayerByActivatedInputControl(InputControl inputControl)
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

        private static void HandleInputUserChange(InputUser inputUser, InputUserChange inputUserChange, InputDevice inputDevice)
        {
            playerCollection.HandleInputUserChange(inputUser, inputUserChange, inputDevice);
        }
        
        #endregion

        #region Editor-Only Debug
#if UNITY_EDITOR
        internal static event Action<int, InputContext> EDITOR_OnPlayerInputContextChanged;

        internal static bool EDITOR_IsInitialized => initialized;
        internal static InputContext EDITOR_GetDefaultContext() => DefaultContext;

        internal static bool EDITOR_TryGetPlayer(int playerID, out InputPlayer inputPlayer)
        {
            if (playerCollection == null)
            {
                inputPlayer = default;
                return false;
            }

            inputPlayer = playerCollection.GetOrAdd(playerID);
            return true;
        }

        private static void EDITOR_HandlePlayerInputContextChanged(InputPlayer inputPlayer)
        {
            EDITOR_OnPlayerInputContextChanged?.Invoke(inputPlayer.ID, inputPlayer.InputContext);
        }
#endif
        #endregion
    }
}
