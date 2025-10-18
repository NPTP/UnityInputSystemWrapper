using System;
using System.Collections.Generic;
using System.Linq;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Bindings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Generated.Actions;
using NPTP.InputSystemWrapper.CustomSetups;
using NPTP.InputSystemWrapper.Utilities;
using RebindingOperation = UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation;

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

        // MARKER.RuntimeInputDataPath.Start
        private const string RUNTIME_INPUT_DATA_PATH = "RuntimeInputData";
        // MARKER.RuntimeInputDataPath.End
        
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
        public static event Action<InputControl> OnAnyButtonPress
        {
            add => AddAnyButtonPressListener(value);
            remove => RemoveAnyButtonPressListener(value);
        }
        
        // TODO (architecture): Shortcoming here. OnInputUserChange doesn't always get called when a binding changes, so we have this as well.
        // Can we consolidate these events into a higher-level abstraction? Or separate them by desired events (binding change, control scheme change, etc with more granularity)
        public static event Action OnBindingsChanged;
        public static event Action<InputUserChangeInfo> OnAnyPlayerInputUserChange;
        public static event Action<InputPlayer> OnAnyPlayerControlSchemeChanged;
        public static event Action<char> OnAnyPlayerKeyboardTextInput;
        
        // MARKER.SinglePlayerFieldsAndProperties.Start
        // MARKER.SinglePlayerFieldsAndProperties.End
        
        private static bool allowPlayerJoining;
        public static bool AllowPlayerJoining
        {
            get => allowPlayerJoining;
            set
            {
                if (value == allowPlayerJoining)
                    return;

                allowPlayerJoining = value;
                if (value) OnAnyButtonPress += JoinPlayerByActivatedInputControl;
                else OnAnyButtonPress -= JoinPlayerByActivatedInputControl;
            }
        }
        
        public static Vector2 MousePosition => Mouse.current.position.ReadValue();

        // MARKER.DefaultContextProperty.Start
        private static InputContext DefaultContext => InputContext.Default;
        // MARKER.DefaultContextProperty.End
        private static InputPlayer DefaultPlayer => playerCollection.DefaultPlayer;

        private static bool initialized;
        private static HashSet<Action<InputControl>> anyButtonPressListeners;
        private static IDisposable anyButtonPressCaller;
        private static InputPlayerCollection playerCollection;
        private static RuntimeInputData runtimeInputData;
        private static RebindingOperation rebindingOperation;
        
        #endregion

        #region Setup
        
        // MARKER.Initialize.Start
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        // MARKER.Initialize.End
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
            
            SetUpTerminationConditions();
            
            runtimeInputData = Resources.Load<RuntimeInputData>(RUNTIME_INPUT_DATA_PATH);
            if (runtimeInputData == null || runtimeInputData.InputActionAsset == null)
            {
                throw new Exception($"{nameof(RuntimeInputData)} is null or its input action asset is null - input will not work!");
            }
            
            // Clear out anything in the scene that would interfere with the ISW's autonomous operation.
            ObjectUtility.DestroyObjectsOfType<PlayerInput, InputSystemUIInputModule, StandaloneInputModule, EventSystem>();
            
            // These registrations must occur before players get assigned InputActionAssets, or else issues resolving the bindings will arise.
            CustomSetupsRegisterer.PerformRegistrations(runtimeInputData);
            
            playerCollection = new InputPlayerCollection(runtimeInputData.InputActionAsset, HandlePlayerAdded, HandlePlayerRemoved);
#if UNITY_EDITOR
            playerCollection.EDITOR_OnPlayerInputContextChanged += EDITOR_HandlePlayerInputContextChanged;
#endif
            
            // MARKER.LoadAllBindingsOnInitialization.Start
            LoadBindingsForAllPlayers();
            // MARKER.LoadAllBindingsOnInitialization.End

            SetContextForAllPlayers(DefaultContext);
            
            anyButtonPressListeners = new HashSet<Action<InputControl>>();
            ++InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange += HandleInputUserChange;
            OnAnyPlayerInputUserChange += BroadcastControlsUpdated;
            OnBindingsChanged += BroadcastControlsUpdated;
            OnAnyPlayerControlSchemeChanged += BroadcastControlsUpdated;

            initialized = true;
        }

        private static void SetUpTerminationConditions()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= handlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += handlePlayModeStateChanged;
            void handlePlayModeStateChanged(PlayModeStateChange playModeStateChange)
            {
                if (playModeStateChange is PlayModeStateChange.ExitingPlayMode)
                {
                    Terminate();
                }
            }
#else
            Application.quitting -= Terminate;
            Application.quitting += Terminate;
#endif
        }

        private static void Terminate()
        {
            UnregisterAllAnyButtonPressListeners();
#if UNITY_EDITOR
            playerCollection.EDITOR_OnPlayerInputContextChanged -= EDITOR_HandlePlayerInputContextChanged;
#endif
            OnAnyPlayerInputUserChange -= BroadcastControlsUpdated;
            OnBindingsChanged -= BroadcastControlsUpdated;
            OnAnyPlayerControlSchemeChanged -= BroadcastControlsUpdated;
            
            playerCollection.Terminate();
            playerCollection = null;
            --InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange -= HandleInputUserChange;
        }

        #endregion
        
        #region Public Interface

        public static void AddPlayer(int playerID) => Player(playerID);
        public static InputPlayer Player(int playerID)
        {
            return playerCollection.GetOrAdd(playerID);
        }

        public static void RemovePlayer(int playerID)
        {
            playerCollection.Remove(playerID);
        }

        public static bool ControlSchemeHas<TDevice>(ControlScheme controlScheme, int playerID = 0) where TDevice : InputDevice
        {
            return Player(playerID).ControlSchemeHas<TDevice>(controlScheme);
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

        public static void ResetAllBindingsForControlScheme(ControlScheme controlScheme, int playerID = 0)
        {
            BindingChanger.ResetBindingsToDefaultForControlScheme(Player(playerID).Asset, controlScheme);
        }

        public static void LoadAllBindings(int playerID = 0)
        {
            BindingSaveLoad.LoadBindingsFromDiskForPlayer(Player(playerID));
        }

        public static void SaveAllBindings(int playerID = 0)
        {
            BindingSaveLoad.SaveBindingsToDiskForPlayer(Player(playerID));
        }

        public static void ResetAllBindings(int playerID = 0)
        {
            BindingChanger.ResetBindingsToDefault(Player(playerID).Asset);
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
                rebindingOperation = BindingChanger.StartInteractiveRebind(actionBindingInfo, bindingIndex, callback);
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
            return Player(playerID).TryGetMatchingActionWrapper(inputAction, out actionWrapper);
        }

        #endregion

        #region Private Runtime Functionality

        private static void UpdatePlayerCollectionListeners()
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
        
        private static void HandlePlayerAdded(InputPlayer inputPlayer)
        {
            UpdatePlayerCollectionListeners();
        }

        private static void HandlePlayerRemoved(int playerID)
        {
            UpdatePlayerCollectionListeners();
        }

        private static void AddAnyButtonPressListener(Action<InputControl> action)
        {
            if (action == null || anyButtonPressListeners.Contains(action))
                return;
            anyButtonPressListeners.Add(action);
            if (anyButtonPressCaller == null)
                anyButtonPressCaller = InputSystem.onAnyButtonPress.Call(HandleAnyButtonPressed);
        }
        
        private static void RemoveAnyButtonPressListener(Action<InputControl> value)
        {
            if (value == null || !anyButtonPressListeners.Contains(value))
                return;
            anyButtonPressListeners.Remove(value);
            DisposeAnyButtonPressCallerIfNoListeners();
        }
        
        private static void DisposeAnyButtonPressCallerIfNoListeners()
        {
            if (anyButtonPressListeners.Count == 0 && anyButtonPressCaller != null)
            {
                anyButtonPressCaller.Dispose();
                anyButtonPressCaller = null;
            }
        }

        private static void UnregisterAllAnyButtonPressListeners()
        {
            anyButtonPressListeners.Clear();
            DisposeAnyButtonPressCallerIfNoListeners();
        }

        private static void HandleAnyButtonPressed(InputControl inputControl)
        {
            InvokeAnyButtonPressListeners(inputControl);
        }

        private static void InvokeAnyButtonPressListeners(InputControl inputControl)
        {
            // Temp array for invocation instead of enumerating the anyButtonPressListeners hash set, since
            // listeners could unsubscribe during invocation which would modify the hashset.
            Action<InputControl>[] listeners = anyButtonPressListeners.ToArray();
            for (int i = 0; i < listeners.Length; i++)
                listeners[i]?.Invoke(inputControl);
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
                return;
            }

            // Mouse + Keyboard is always joined.
            if (device is Mouse or Keyboard)
            {
                return;
            }
            
            // Any devices already in use can't be stolen.
            if (playerCollection.IsDeviceLastUsedByAnyPlayer(device))
            {
                return;
            }

            // Allow stealing a device paired to, but currently unused by, another player.
            if (playerCollection.TryGetPlayerPairedWithDevice(device, out InputPlayer pairedPlayer))
            {
                pairedPlayer.UnpairDevice(device);
            }

            // Find a player to pair the device to.
            if (playerCollection.TryPairDeviceToFirstDisabledPlayer(device, out InputPlayer disabledPlayer))
            {
                disabledPlayer.Enabled = true;
                return;
            }

            // If no disabled players exist, create and pair to a new player.
            playerCollection.PairDeviceToNewPlayer(device);
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
