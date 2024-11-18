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
using NPTP.InputSystemWrapper.InputDevices;
using NPTP.InputSystemWrapper.Utilities;
using RebindingOperation = UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NPTP.InputSystemWrapper
{
    /// <summary>
    /// Main point of usage for all input in the game.
    /// </summary>
    public static class Input
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

        // TODO (architecture): Shortcoming here. OnInputUserChange doesn't always get called when a binding changes, so we have this as well.
        // Can we consolidate these events into a higher-level abstraction? Or separate them by desired events (binding change, control scheme change, etc with more granularity)
        public static event Action OnBindingsChanged;
        
        public static event Action<InputControl> OnAnyButtonPress
        {
            add => AddAnyButtonPressListener(value);
            remove => RemoveAnyButtonPressListener(value);
        }
        
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.Start
        public static event Action<InputUserChangeInfo> OnInputUserChange
        {
            add => Player1.OnInputUserChange += value;
            remove => Player1.OnInputUserChange -= value;
        }

        public static event Action<ControlScheme> OnControlSchemeChanged
        {
            add => Player1.OnControlSchemeChanged += value;
            remove => Player1.OnControlSchemeChanged -= value;
        }

        public static event Action<char> OnKeyboardTextInput
        {
            add => Player1.OnKeyboardTextInput += value;
            remove => Player1.OnKeyboardTextInput -= value;
        }

        public static PlayerActions Player => Player1.Player;
        public static UIActions UI => Player1.UI;
        public static XXXActions XXX => Player1.XXX;

        public static InputContext Context
        {
            get => Player1.InputContext;
            set => Player1.InputContext = value;
        }

        public static ControlScheme CurrentControlScheme => Player1.CurrentControlScheme;
        public static Vector2 MousePosition => Mouse.current.position.ReadValue();

        private static InputPlayer Player1 => GetPlayer(PlayerID.Player1);
        private static bool AllowPlayerJoining => false;
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.End

        // MARKER.DefaultContextProperty.Start
        private static InputContext DefaultContext => InputContext.Player;
        // MARKER.DefaultContextProperty.End

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
                ReflectionUtility.ResetStaticClassMembersToDefault(typeof(Input));
            }
            
            SetUpTerminationConditions();
            
            runtimeInputData = Resources.Load<RuntimeInputData>(RUNTIME_INPUT_DATA_PATH);
            if (runtimeInputData == null || runtimeInputData.InputActionAsset == null)
            {
                throw new Exception($"{nameof(RuntimeInputData)} is null or its input action asset is null - input will not work!");
            }
            
            int maxPlayers = Enum.GetValues(typeof(PlayerID)).Length;
            
            // TODO (optimization): Could make startup slow. It should probably just be a requirement of using this package that you clear your old input modules & event systems out.
            ObjectUtility.DestroyAllObjectsOfType<PlayerInput, InputSystemUIInputModule, StandaloneInputModule, EventSystem>();
            
            playerCollection = new InputPlayerCollection(runtimeInputData.InputActionAsset, maxPlayers);
#if UNITY_EDITOR
            playerCollection.EDITOR_OnPlayerInputContextChanged += EDITOR_HandlePlayerInputContextChanged;
#endif
            LoadBindingsForAllPlayers();
            EnableContextForAllPlayers(DefaultContext);
            
            anyButtonPressListeners = new HashSet<Action<InputControl>>();
            ++InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange += HandleInputUserChange;
            InputDeviceRegistration.PerformRegistrations();
            
            initialized = true;
        }

        private static void SetUpTerminationConditions()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= handlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += handlePlayModeStateChanged;
            void handlePlayModeStateChanged(PlayModeStateChange playModeStateChange)
            {
                if (playModeStateChange is PlayModeStateChange.ExitingPlayMode) Terminate();
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
            playerCollection.TerminateAll();
            --InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange -= HandleInputUserChange;
        }

        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Custom yield instruction for coroutines to make waiting for any button press a lot more syntactically convenient.
        /// Use like:
        /// yield return new Input.WaitForAnyButtonPress();
        /// </summary>
        public class WaitForAnyButtonPress : CustomYieldInstruction
        {
            public override bool keepWaiting => !anyButtonPressed;
            private bool anyButtonPressed;
            ~WaitForAnyButtonPress() => OnAnyButtonPress -= HandleAnyButtonPress;

            public WaitForAnyButtonPress()
            {
                OnAnyButtonPress += HandleAnyButtonPress;
            }

            private void HandleAnyButtonPress(InputControl inputControl)
            {
                anyButtonPressed = true;
                OnAnyButtonPress -= HandleAnyButtonPress;
            }
        }
        
        // TODO (multiplayer): MP method signature which takes a PlayerID
        public static bool ControlSchemeHas<TDevice>(ControlScheme controlScheme) where TDevice : InputDevice
        {
            return Player1.ControlSchemeHas<TDevice>(controlScheme);
        }
        
        /// <summary>
        /// Start an interactive rebind: wait for input from the given player and device to bind a new control to the action given in the action reference.
        /// </summary>
        /// <param name="actionReference">Action reference wrapper containing action to be rebound.</param>
        /// <param name="controlScheme">Control scheme for which to do the rebinding.</param>
        /// <param name="callback">Callback on rebind cancel/complete. Note that this callback will be invoked whether or not the binding was actually changed,
        /// and even if the rebind fails to execute. It is intended to help you manage control flow on your UI or wherever rebinding is happening.
        /// (Subscribe to Input.OnBindingsChanged to know when a binding has actually been set to a new value.)</param>
        // MARKER.StartInteractiveRebind.Start
        public static void StartInteractiveRebind(ActionReference actionReference, ControlScheme controlScheme, Action<RebindStatus> callback = null)
        // MARKER.StartInteractiveRebind.End
        {
            if (rebindingOperation != null)
            {
                rebindingOperation.Cancel();
                rebindingOperation.Dispose();
            }

            // MARKER.PlayerGetter.Start
            InputPlayer player = Player1;
            // MARKER.PlayerGetter.End

            if (player.TryGetMapAndActionInPlayerAsset(actionReference.InternalAction, out InputActionMap _, out InputAction action) &&
                BindingGetter.TryGetFirstBindingIndex(actionReference, action, controlScheme, out int bindingIndex))
            {
                rebindingOperation = BindingChanger.StartInteractiveRebind(action, bindingIndex, callback);
            }
            else
            {
                ISWDebug.LogError("Rebinding failed: Action or binding index could not be found.");
                rebindingOperation?.Dispose();
                rebindingOperation = null;
                callback?.Invoke(RebindStatus.Failed);
            }
        }

        /// <summary>
        /// Try to get the current binding info for the given action reference.
        /// If this returns true, the binding infos enumerable is guaranteed to have at least one element.
        /// </summary>
        // MARKER.TryGetCurrentBindingInfo.Start
        public static bool TryGetCurrentBindingInfo(ActionReference actionReference, out IEnumerable<BindingInfo> bindingInfos)
        // MARKER.TryGetCurrentBindingInfo.End
        {
            // MARKER.PlayerGetter.Start
            InputPlayer player = Player1;
            // MARKER.PlayerGetter.End
            return BindingGetter.TryGetBindingInfo(runtimeInputData, player, actionReference.ToActionInfo(), player.CurrentControlScheme, out bindingInfos);
        }

        /// <summary>
        /// Try to get the binding info for the given action reference and control scheme.
        /// If this returns true, the binding infos enumerable is guaranteed to have at least one element.
        /// </summary>
        // TODO (multiplayer): MP method signature which takes a PlayerID
        public static bool TryGetBindingInfo(ActionReference actionReference, ControlScheme controlScheme, out IEnumerable<BindingInfo> bindingInfos)
        {
            // MARKER.PlayerGetter.Start
            InputPlayer player = Player1;
            // MARKER.PlayerGetter.End
            return BindingGetter.TryGetBindingInfo(runtimeInputData, player, actionReference.ToActionInfo(), controlScheme, out bindingInfos);
        }
        
        // TODO (multiplayer): MP method signature which takes a PlayerID
        public static void ResetBindingForAction(ActionReference actionReference, ControlScheme controlScheme)
        {
            // MARKER.PlayerGetter.Start
            InputPlayer player = Player1;
            // MARKER.PlayerGetter.End
            BindingChanger.ResetBindingToDefaultForControlScheme(player, actionReference.ToActionInfo(), controlScheme);
        }

        // TODO (multiplayer): MP method signature which takes a PlayerID
        public static void ResetAllBindingsForControlScheme(ControlScheme controlScheme)
        {
            // MARKER.PlayerGetter.Start
            InputPlayer player = Player1;
            // MARKER.PlayerGetter.End
            BindingChanger.ResetBindingsToDefaultForControlScheme(player.Asset, controlScheme);
        }

        // TODO (multiplayer): MP method signature which takes a PlayerID
        public static void LoadAllBindings()
        {
            // MARKER.PlayerGetter.Start
            InputPlayer player = Player1;
            // MARKER.PlayerGetter.End
            BindingSaveLoad.LoadBindingsFromDiskForPlayer(player);
        }

        // TODO (multiplayer): MP method signature which takes a PlayerID
        public static void SaveAllBindings()
        {
            // MARKER.PlayerGetter.Start
            InputPlayer player = Player1;
            // MARKER.PlayerGetter.End
            BindingSaveLoad.SaveBindingsToDiskForPlayer(player);
        }

        // TODO (multiplayer): MP method signature which takes a PlayerID
        public static void ResetAllBindings()
        {
            // MARKER.PlayerGetter.Start
            InputPlayer player = Player1;
            // MARKER.PlayerGetter.End
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
        
        internal static bool TryGetCurrentBindingInfo(ActionWrapper actionWrapper, out IEnumerable<BindingInfo> bindingInfos)
        {
            if (!playerCollection.TryGetPlayerAssociatedWithAsset(actionWrapper.InputAction.actionMap.asset, out InputPlayer player))
            {
                bindingInfos = default;
                return false;
            }
            
            return BindingGetter.TryGetBindingInfo(runtimeInputData, player, new ActionInfo(actionWrapper.InputAction, false), player.CurrentControlScheme, out bindingInfos);
        }

        internal static bool TryGetCurrentBindingInfo(ActionWrapper actionWrapper, CompositePart compositePart, out IEnumerable<BindingInfo> bindingInfos)
        {
            if (playerCollection.TryGetPlayerAssociatedWithAsset(actionWrapper.InputAction.actionMap.asset, out InputPlayer player))
            {
                bindingInfos = default;
                return false;
            }
            
            return BindingGetter.TryGetBindingInfo(runtimeInputData, player, new ActionInfo(actionWrapper.InputAction, true, compositePart), player.CurrentControlScheme, out bindingInfos);
        }
        
        internal static bool TryGetBindingInfo(ActionWrapper actionWrapper, ControlScheme controlScheme, out IEnumerable<BindingInfo> bindingInfos)
        {
            if (playerCollection.TryGetPlayerAssociatedWithAsset(actionWrapper.InputAction.actionMap.asset, out InputPlayer player))
            {
                bindingInfos = default;
                return false;
            }
            
            return BindingGetter.TryGetBindingInfo(runtimeInputData, player, new ActionInfo(actionWrapper.InputAction, false), controlScheme, out bindingInfos);
        }

        internal static bool TryGetBindingInfo(ActionWrapper actionWrapper, ControlScheme controlScheme, CompositePart compositePart, out IEnumerable<BindingInfo> bindingInfos)
        {
            if (playerCollection.TryGetPlayerAssociatedWithAsset(actionWrapper.InputAction.actionMap.asset, out InputPlayer player))
            {
                bindingInfos = default;
                return false;
            }
            
            return BindingGetter.TryGetBindingInfo(runtimeInputData, player, new ActionInfo(actionWrapper.InputAction, true, compositePart), controlScheme, out bindingInfos);
        }

        // TODO (multiplayer): MP method signature which takes a PlayerID
        internal static ActionWrapper GetActionWrapperFromReference(ActionReference actionReference)
        {
            // MARKER.PlayerGetter.Start
            InputPlayer player = Player1;
            // MARKER.PlayerGetter.End
            
            return player.FindActionWrapper(actionReference);
        }
        
        // TODO (multiplayer): MP method signature which takes a PlayerID
        internal static ActionWrapper GetActionWrapperFromReference(InputActionReference inputActionReference)
        {
            // MARKER.PlayerGetter.Start
            InputPlayer player = Player1;
            // MARKER.PlayerGetter.End
            
            return player.FindActionWrapper(inputActionReference);
        }
        
        #endregion

        #region Private Runtime Functionality
        
        // MARKER.EnableContextForAllPlayersSignature.Start
        private static void EnableContextForAllPlayers(InputContext inputContext)
            // MARKER.EnableContextForAllPlayersSignature.End
        {
            playerCollection.EnableContextForAll(inputContext);
        }
        
        private static InputPlayer GetPlayer(PlayerID id)
        {
            return playerCollection[id];
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

            // Player joining is always disallowed in SinglePlayer mode.
            if (!AllowPlayerJoining)
            {
                return;
            }
            
            JoinPlayerByActivatedInputControl(inputControl);
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
            foreach (PlayerID playerID in Enum.GetValues(typeof(PlayerID)))
            {
                BindingSaveLoad.LoadBindingsFromDiskForPlayer(GetPlayer(playerID));
            }
        }
        
        private static void JoinPlayerByActivatedInputControl(InputControl inputControl)
        {
            InputDevice device = inputControl.device;

            // Mouse + Keyboard is always joined, currently used devices can't be stolen, and we can't join an inactive player if they're all already active.
            if (device is Mouse or Keyboard || playerCollection.IsDeviceLastUsedByAnyPlayer(device) || !playerCollection.AnyPlayerDisabled())
            {
                return;
            }

            // Allow "stealing" a device paired to, but currently unused by, another player.
            if (playerCollection.TryGetPlayerPairedWithDevice(device, out InputPlayer pairedPlayer))
            {
                pairedPlayer.UnpairDevice(device);
            }

            if (playerCollection.TryPairDeviceToFirstDisabledPlayer(device, out InputPlayer disabledPlayer))
            {
                disabledPlayer.Enabled = true;
            }
        }

        private static void HandleInputUserChange(InputUser inputUser, InputUserChange inputUserChange, InputDevice inputDevice)
        {
            playerCollection.HandleInputUserChange(inputUser, inputUserChange, inputDevice);
        }
        
        #endregion

        #region Editor-Only Debug
#if UNITY_EDITOR
        public static event Action<PlayerID, InputContext> EDITOR_OnPlayerInputContextChanged;

        public static bool EDITOR_IsInitialized => initialized;
        
        private static void EDITOR_HandlePlayerInputContextChanged(InputPlayer inputPlayer)
        {
            EDITOR_OnPlayerInputContextChanged?.Invoke(inputPlayer.ID, inputPlayer.InputContext);
        }
#endif
        #endregion
    }
}
