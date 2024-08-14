using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using NPTP.InputSystemWrapper.Generated.MapActions;
using NPTP.InputSystemWrapper.AutopopulatedEnums;
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
        
        public static event Action OnAnyButtonPressed;
        
        private static int listenForAnyButtonPress;
        public static int ListenForAnyButtonPress
        {
            set
            {
                if (value > 0 && anyButtonPressListener == null)
                {
                    anyButtonPressListener = InputSystem.onAnyButtonPress.Call(HandleAnyButtonPressed);
                }
                else if (value == 0 && anyButtonPressListener != null)
                {
                    anyButtonPressListener.Dispose();
                    anyButtonPressListener = null;
                }
                else if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Cannot be negative");
                }
                
                listenForAnyButtonPress = value;
            }
        }
        
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.Start
        private static bool allowPlayerJoining;
        public static bool AllowPlayerJoining
        {
            get => allowPlayerJoining;
            set
            {
                if (value == allowPlayerJoining) return;
                allowPlayerJoining = value;
                ListenForAnyButtonPress = value ? listenForAnyButtonPress + 1 : listenForAnyButtonPress - 1;
            }
        }
        public static InputPlayer Player(PlayerID id) => playerCollection[id];
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.End
        
        // MARKER.DefaultContextProperty.Start
        private static InputContext DefaultContext => InputContext.Player;
        // MARKER.DefaultContextProperty.End

        private static InputPlayerCollection playerCollection;
        private static RuntimeInputData runtimeInputData;
        private static IDisposable anyButtonPressListener;

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
            ListenForAnyButtonPress = 0;
            playerCollection.TerminateAll();
            --InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange -= HandleInputUserChange;
        }

        #endregion

        #region Public Interface

        public static void EnableContextForAllPlayers(InputContext context)
        {
            playerCollection.EnableContextForAll(context);
        }

        public static List<BindingPathInfo> GetAllBindingPathInfos(InputAction action)
        {
            List<BindingPathInfo> bindingPathInfos = new();
            foreach (InputControl inputControl in action.controls)
            {
                if (TryGetBindingPathInfo(inputControl, out BindingPathInfo bindingPathInfo))
                {
                    bindingPathInfos.Add(bindingPathInfo);
                }
            }

            return bindingPathInfos;
        }
        
        public static bool TryGetBindingPathInfo(InputControl inputControl, out BindingPathInfo bindingPathInfo)
        {
            bindingPathInfo = default;
            ParseInputControlPath(inputControl, out string deviceName, out string controlPath);
            ControlScheme controlScheme = ResolveDeviceToControlScheme(deviceName);

            if (!runtimeInputData.BindingDataReferences.TryGetValue(controlScheme, out BindingDataReference bindingDataReference))
            {
                return false;
            }
            
            BindingData bindingData = bindingDataReference.Load();
            if (bindingData == null)
            {
                return false;
            }
            
            if (!bindingData.BindingDisplayInfo.TryGetValue(controlPath, out bindingPathInfo))
            {
                return false;
            }
            
            bindingPathInfo.SetInputControlDisplayName(inputControl.displayName);

            return true;
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
        
        private static void HandleInputUserChange(InputUser inputUser, InputUserChange inputUserChange, InputDevice inputDevice)
        {
            playerCollection.HandleInputUserChange(inputUser, inputUserChange, inputDevice);
        }

        private static void HandleAnyButtonPressed(InputControl inputControl)
        {
            OnAnyButtonPressed?.Invoke();
            
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

        private static ControlScheme ResolveDeviceToControlScheme(string deviceName)
        {
            return deviceName switch
            {
                // TODO: Control scheme names may change. Have to discover these manually and update them in the generation step.
                "Mouse" => ControlScheme.KeyboardMouse,
                "Keyboard" => ControlScheme.KeyboardMouse,
                _ => ControlScheme.Gamepad
            };
        }

        private static void ParseInputControlPath(InputControl inputControl, out string deviceName, out string controlPath)
        {
            deviceName = inputControl.device.name;
            controlPath = inputControl.path[(2 + deviceName.Length)..];
        }

        #endregion
    }
}
