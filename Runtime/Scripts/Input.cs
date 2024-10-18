using System;
using System.Collections.Generic;
using System.Linq;
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

        // TODO: Shortcoming here. OnInputUserChange doesn't always get called when a binding changes, so we have this as well. Can we consolidate these events into a higher-level abstraction?
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

        public static event Action<char> OnKeyboardTextInput
        {
            add => Player1.OnKeyboardTextInput += value;
            remove => Player1.OnKeyboardTextInput -= value;
        }

        public static PlayerActions Player => Player1.Player;
        public static UIActions UI => Player1.UI;

        public static InputContext Context
        {
            get => Player1.InputContext;
            set => Player1.InputContext = value;
        }

        public static ControlScheme CurrentControlScheme => Player1.CurrentControlScheme;

        private static InputPlayer Player1 => GetPlayer(PlayerID.Player1);
        private static bool AllowPlayerJoining => false;
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.End
        
        // MARKER.DefaultContextProperty.Start
        private static InputContext DefaultContext => InputContext.Player;
        // MARKER.DefaultContextProperty.End

        private static HashSet<Action<InputControl>> anyButtonPressListeners;
        private static IDisposable anyButtonPressCaller;
        private static InputPlayerCollection playerCollection;
        private static RuntimeInputData runtimeInputData;
        private static RebindingOperation rebindingOperation;

        #endregion

        #region Setup

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            // Allows input system to work even when domain reload is disabled in editor.
            if (RuntimeSafeEditorUtility.IsDomainReloadDisabled())
                ReflectionUtility.ResetStaticClassMembersToDefault(typeof(Input));
            
            SetUpTerminationConditions();
            
            runtimeInputData = Resources.Load<RuntimeInputData>(RUNTIME_INPUT_DATA_PATH);
            if (runtimeInputData == null || runtimeInputData.InputActionAsset == null)
                throw new Exception($"{nameof(RuntimeInputData)} is null or its input action asset is null - input will not work!");
            
            int maxPlayers = Enum.GetValues(typeof(PlayerID)).Length;
            
            // TODO (optimization): Is this really necessary? It should probably just be a requirement of using this package that you clear your old input modules & event systems out. Plus, this makes startup slower than it needs to be.
            ObjectUtility.DestroyAllObjectsOfType<PlayerInput, InputSystemUIInputModule, StandaloneInputModule, EventSystem>();
            
            playerCollection = new InputPlayerCollection(runtimeInputData.InputActionAsset, maxPlayers);
            LoadBindingsForAllPlayers();
            EnableContextForAllPlayers(DefaultContext);

            anyButtonPressListeners = new HashSet<Action<InputControl>>();
            ++InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange += HandleInputUserChange;
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
            playerCollection.TerminateAll();
            --InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange -= HandleInputUserChange;
        }

        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Start an interactive rebind: wait for input from the given player and device to bind a new control to the action given in the action reference.
        /// </summary>
        /// <param name="actionReference">Reference wrapper containing action to be rebound.</param>
        /// <param name="controlScheme">Control scheme for which to do the rebinding.</param>
        /// <param name="callback">Callback on rebind cancel/complete. Note that this callback will be invoked whether or not the binding was actually changed,
        /// and even if the rebind fails to execute. It is intended to help you manage control flow on your UI or wherever rebinding is happening.
        /// (Subscribe to Input.OnBindingsChanged to know when a binding has actually been set to a new value.)</param>
        public static void StartInteractiveRebind(InputActionReferenceWrapper actionReference, ControlScheme controlScheme, Action callback = null)
        {
            if (rebindingOperation != null)
            {
                rebindingOperation.Cancel();
                rebindingOperation.Dispose();
            }

            // TODO: Multiplayer version that takes PlayerID in method signature to rebind specific player selected here.
            InputPlayer player = GetPlayer(PlayerID.Player1);

            if (player.TryGetMapAndActionInPlayerAsset(actionReference.InternalReference, out InputActionMap _, out InputAction action) &&
                BindingGetter.TryGetFirstBindingIndex(actionReference, action, controlScheme, out int bindingIndex))
            {
                rebindingOperation = BindingChanger.StartInteractiveRebind(action, bindingIndex, callback);
            }
            else
            {
                Debug.LogError("Rebinding failed: Action or binding index could not be found.");
                rebindingOperation?.Dispose();
                rebindingOperation = null;
                callback?.Invoke();
            }
        }

        /// <summary>
        /// Try to get the current binding info for the given action reference.
        /// </summary>
        // MARKER.TryGetCurrentBindingInfo.Start
        public static bool TryGetCurrentBindingInfo(InputActionReferenceWrapper actionReferenceWrapper, out IEnumerable<BindingInfo> bindingInfos)
        {
            return BindingGetter.TryGetCurrentBindingInfo(runtimeInputData, Player1, actionReferenceWrapper, out bindingInfos);
        }
        // MARKER.TryGetCurrentBindingInfo.End
        
        // TODO (multiplayer): MP version which takes a PlayerID and uses GetPlayer(id)
        public static void ResetBindingForAction(InputActionReferenceWrapper actionReference, ControlScheme controlScheme)
        {
            BindingChanger.ResetBindingToDefaultForControlScheme(Player1, actionReference, controlScheme);
        }

        // TODO (multiplayer): MP version which takes a PlayerID and uses GetPlayer(id)
        public static void ResetAllBindingsForControlScheme(ControlScheme controlScheme)
        {
            BindingChanger.ResetBindingsToDefaultForControlScheme(Player1.Asset, controlScheme);
        }

        // TODO (multiplayer): MP version which takes a PlayerID and uses GetPlayer(id)
        public static void LoadAllBindings()
        {
            BindingSaveLoad.LoadBindingsFromDiskForPlayer(Player1);
        }

        // TODO (multiplayer): MP version which takes a PlayerID and uses GetPlayer(id)
        public static void SaveAllBindings()
        {
            BindingSaveLoad.SaveBindingsToDiskForPlayer(Player1);
        }

        // TODO (multiplayer): MP version which takes a PlayerID and uses GetPlayer(id)
        public static void ResetAllBindings()
        {
            BindingChanger.ResetBindingsToDefault(Player1.Asset);
        }

        #endregion
        
        #region Internal Interface

        internal static void BroadcastBindingsChanged()
        {
            OnBindingsChanged?.Invoke();
        }
        
        internal static void ChangeSubscription(InputActionReference actionReference, Action<InputAction.CallbackContext> callback, bool subscribe)
        {
            if (actionReference == null)
            {
                Debug.LogError("Trying to subscribe to a nonexistent action reference.");
                return;
            }

            if (callback == null)
            {
                Debug.LogError("Trying to subscribe with a nonexistent callback.");
                return;
            }
            
            playerCollection.FindActionEventAndSubscribeAll(actionReference, callback, subscribe);
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
    }
}
