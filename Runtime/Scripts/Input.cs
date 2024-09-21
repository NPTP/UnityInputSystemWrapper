using System;
using System.Collections.Generic;
using System.Linq;
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

        private static InputPlayer Player1 => playerCollection[PlayerID.Player1];
        private static bool AllowPlayerJoining => false;
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.End
        
        // MARKER.DefaultContextProperty.Start
        private static InputContext DefaultContext => InputContext.Player;
        // MARKER.DefaultContextProperty.End

        private static HashSet<Action<InputControl>> anyButtonPressListeners;
        private static IDisposable anyButtonPressCaller;
        private static InputPlayerCollection playerCollection;
        private static RuntimeInputData runtimeInputData;

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
            if (runtimeInputData == null)
                throw new Exception($"{nameof(RuntimeInputData)} could not be loaded - input will not work!");
            if (runtimeInputData.InputActionAsset == null)
                throw new Exception($"{nameof(RuntimeInputData)} is missing an input action asset - input will not work!");

            int maxPlayers = Enum.GetValues(typeof(PlayerID)).Length;
            
            // TODO: Is this really necessary? It should probably just be a requirement of using this package that you clear your old input modules & event systems out...
            ObjectUtility.DestroyAllObjectsOfType<PlayerInput, InputSystemUIInputModule, StandaloneInputModule, EventSystem>();
            
            playerCollection = new InputPlayerCollection(runtimeInputData.InputActionAsset, maxPlayers);
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

        #region Public Interface
        
        // MARKER.EnableContextForAllPlayersSignature.Start
        private static void EnableContextForAllPlayers(InputContext inputContext)
        // MARKER.EnableContextForAllPlayersSignature.End
        {
            playerCollection.EnableContextForAll(inputContext);
        }
        
        // MARKER.TryGetActionBindingInfo.Start
        public static bool TryGetActionBindingInfo(InputAction action, InputDevice device, out IEnumerable<BindingInfo> bindingInfos)
        {
            return InputBindings.TryGetActionBindingInfo(runtimeInputData, Player1.Asset, action.name, device, out bindingInfos);
        }
        // MARKER.TryGetActionBindingInfo.End

        #endregion

        #region Private Runtime Functionality
        
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
