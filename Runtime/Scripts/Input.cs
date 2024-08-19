using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using NPTP.InputSystemWrapper.Generated.MapActions;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Data;
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
        
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.Start

        public static event Action<InputControl> OnAnyButtonPress
        {
            add
            {
                if (value == null || anyButtonPressListeners.Contains(value))
                    return;
                anyButtonPressListeners.Add(value);
                if (anyButtonPressCaller == null)
                    anyButtonPressCaller = InputSystem.onAnyButtonPress.Call(HandleAnyButtonPressed);
            }
            remove
            {
                if (value == null || !anyButtonPressListeners.Contains(value))
                    return;
                anyButtonPressListeners.Remove(value);
                TearDownAnyButtonPressCaller();
            }
        }
        
        private static bool AllowPlayerJoining => false;
        private static InputPlayer GetPlayer(PlayerID id) => playerCollection[id];
        public static event Action<DeviceControlInfo> OnDeviceControlChanged
        {
            add => GetPlayer(PlayerID.Player1).OnDeviceControlChanged += value;
            remove => GetPlayer(PlayerID.Player1).OnDeviceControlChanged -= value;
        }
        public static event Action<char> OnKeyboardTextInput
        {
            add => GetPlayer(PlayerID.Player1).OnKeyboardTextInput += value;
            remove => GetPlayer(PlayerID.Player1).OnKeyboardTextInput -= value;
        }
        public static PlayerActions Player => GetPlayer(PlayerID.Player1).Player;
        public static UIActions UI => GetPlayer(PlayerID.Player1).UI;
        public static InputContext CurrentContext => GetPlayer(PlayerID.Player1).CurrentContext;
        public static ControlScheme CurrentControlScheme => GetPlayer(PlayerID.Player1).CurrentControlScheme;
        public static InputDevice LastUsedDevice => GetPlayer(PlayerID.Player1).LastUsedDevice;
        public static void EnableContext(InputContext context) => GetPlayer(PlayerID.Player1).CurrentContext = context;
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.End
        
        // MARKER.DefaultContextProperty.Start
        private static InputContext DefaultContext => InputContext.Player;
        // MARKER.DefaultContextProperty.End

        private static readonly HashSet<Action<InputControl>> anyButtonPressListeners = new();
        private static IDisposable anyButtonPressCaller;
        private static InputPlayerCollection playerCollection;
        private static RuntimeInputData runtimeInputData;

        #endregion

        #region Setup

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            SetUpTerminationConditions();
            
            runtimeInputData = Resources.Load<RuntimeInputData>(RUNTIME_INPUT_DATA_PATH);
            InputActionAsset asset = runtimeInputData.InputActionAsset;
            if (asset == null)
            {
                throw new Exception($"{runtimeInputData.GetType().Name} is missing its input action asset!");
            }
            
            int maxPlayers = Enum.GetValues(typeof(PlayerID)).Length;
            ObjectUtility.DestroyAllObjectsOfType<PlayerInput, InputSystemUIInputModule, StandaloneInputModule, EventSystem>();
            playerCollection = new InputPlayerCollection(asset, maxPlayers);
            EnableContextForAllPlayers(DefaultContext);
            
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
        
        public static void EnableContextForAllPlayers(InputContext context)
        {
            playerCollection.EnableContextForAll(context);
        }
        
        // TODO: Remove this overload entirely in SP, and remove device argument from params of 2nd overload, and use LastUsedDevice property instead
        public static bool TryGetActionBindingInfo(InputAction action, PlayerID playerID, out BindingInfo bindingInfo)
        {
            // MARKER.TryGetActionBindingInfoPlayerDependent.Start
            return TryGetActionBindingInfo(action, LastUsedDevice, out bindingInfo);
            // MARKER.TryGetActionBindingInfoPlayerDependent.End
        }
        
        public static bool TryGetActionBindingInfo(InputAction action, InputDevice device, out BindingInfo bindingInfo)
        {
            bindingInfo = default;
            
            if (device == null)
            {
                return false;
            }
            
            if (!runtimeInputData.TryGetBindingData(device, out BindingData bindingData))
            {
                return false;
            }

            // TODO: Support returning multiple control paths, since an action may have multiple bindings on a single device
            if (!TryGetControlPath(action, device, out string controlPath))
            {
                return false;
            }
            
            return bindingData.TryGetBindingInfo(controlPath, out bindingInfo);
        }
        
        #endregion

        #region Private Runtime Functionality
        
        private static bool TryGetControlPath(InputAction action, InputDevice device, out string controlPath)
        {
            controlPath = default;
            
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                InputControl control = InputControlPath.TryFindControl(device, binding.effectivePath);
                if (control != null && control.device == device)
                {
                    controlPath = control.path[(2 + control.device.name.Length)..];
                    return true;
                }
            }

            return false;
        }

        private static void TearDownAnyButtonPressCaller()
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
            TearDownAnyButtonPressCaller();
        }
        
        private static void HandleInputUserChange(InputUser inputUser, InputUserChange inputUserChange, InputDevice inputDevice)
        {
            playerCollection.HandleInputUserChange(inputUser, inputUserChange, inputDevice);
        }

        private static void HandleAnyButtonPressed(InputControl inputControl)
        {
            // Temp array for invocation since listeners can unsubscribe and modify the hashset during enumeration
            Action<InputControl>[] listeners = anyButtonPressListeners.ToArray();
            for (int i = 0; i < listeners.Length; i++)
                listeners[i]?.Invoke(inputControl);

            // Player joining is always disallowed in SP mode.
            if (!AllowPlayerJoining)
            {
                return;
            }
            
            InputDevice device = inputControl.device;
            
            if (device is Mouse or Keyboard ||
                playerCollection.IsDeviceLastUsedByAnyPlayer(device) ||
                !playerCollection.AnyPlayerDisabled())
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

        #endregion
    }
}
