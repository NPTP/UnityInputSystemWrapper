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

        public static event Action OnBindingOperationEnded;
        
        public static event Action<InputControl> OnAnyButtonPress
        {
            add => AddAnyButtonPressListener(value);
            remove => RemoveAnyButtonPressListener(value);
        }
        
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.Start
        public static event Action<DeviceControlInfo> OnDeviceControlChanged
        {
            add => Player1.OnDeviceControlChanged += value;
            remove => Player1.OnDeviceControlChanged -= value;
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
        public static InputDevice LastUsedDevice => Player1.LastUsedDevice;

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
            
            // TODO: Is this really necessary? It should probably just be a requirement of using this package that you clear your old input modules & event systems out. Plus, this makes startup slower than it needs to be.
            ObjectUtility.DestroyAllObjectsOfType<PlayerInput, InputSystemUIInputModule, StandaloneInputModule, EventSystem>();
            
            playerCollection = new InputPlayerCollection(runtimeInputData.InputActionAsset, maxPlayers);
            LoadBindingsFor(PlayerID.Player1);
            EnableContextForAllPlayers(DefaultContext);

            anyButtonPressListeners = new HashSet<Action<InputControl>>();
            ++InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange += HandleInputUserChange;
            BindingChanger.OnBindingOperationEnded += HandleBindingOperationEnded;
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
            BindingChanger.OnBindingOperationEnded -= HandleBindingOperationEnded;
        }

        #endregion
        
        #region Public Interface
        
        // TODO: Singleplayer version doesn't take player ID, multiplayer version does
        public static void StartInteractiveRebind(InputActionReferenceWrapper actionReference, PlayerID playerID, SupportedDevice device)
        {
            rebindingOperation?.Cancel();
            
            if (GetPlayer(playerID).TryGetMapAndActionInPlayerAsset(actionReference.InternalReference, out InputActionMap map, out InputAction action) &&
                BindingGetter.TryGetBindingIndexForDevice(action, device, out int bindingIndex))
            {
                rebindingOperation = BindingChanger.StartInteractiveRebind(action, bindingIndex);
            }
        }

        public static void ResetBinding(InputActionReferenceWrapper actionReference, PlayerID playerID, SupportedDevice device)
        {
            if (GetPlayer(playerID).TryGetMapAndActionInPlayerAsset(actionReference.InternalReference, out InputActionMap map, out InputAction action))
            {
                BindingChanger.ResetBindingToDefaultForDevice(action, device);
            }
        }

        public static void ResetAllBindings(PlayerID playerID, SupportedDevice device)
        {
            BindingChanger.ResetBindingsToDefaultForDevice(GetPlayer(playerID).Asset, device);
        }

        public static void LoadBindingsFor(PlayerID playerID)
        {
            BindingSaveLoad.LoadBindingsFromDiskForPlayer(GetPlayer(playerID));
        }

        public static void SaveBindingsFor(PlayerID playerID)
        {
            BindingSaveLoad.SaveBindingsToDiskForPlayer(GetPlayer(playerID));
        }
        
        // MARKER.EnableContextForAllPlayersSignature.Start
        private static void EnableContextForAllPlayers(InputContext inputContext)
        // MARKER.EnableContextForAllPlayersSignature.End
        {
            playerCollection.EnableContextForAll(inputContext);
        }
        
        // TODO: MP version of this
        // MARKER.TryGetActionBindingInfo.Start
        public static bool TryGetActionBindingInfo(InputActionReferenceWrapper actionReference, InputDevice device, out IEnumerable<BindingInfo> bindingInfos)
        {
            bindingInfos = null;
            return Player1.TryGetMapAndActionInPlayerAsset(actionReference.InternalReference, out InputActionMap map, out InputAction action) &&
                   BindingGetter.TryGetActionBindingInfo(runtimeInputData, action, device, out bindingInfos);
        }
        // MARKER.TryGetActionBindingInfo.End

        #endregion
        
        #region Internal Interface

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

        private static void HandleBindingOperationEnded()
        {
            OnBindingOperationEnded?.Invoke();
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
