using System;
using System.Collections.Generic;
using InputSystemWrapper.Utilities.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Utilities;
using UnityInputSystemWrapper.Data;
using UnityInputSystemWrapper.Generated.MapActions;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityInputSystemWrapper
{
    /// <summary>
    /// Main point of usage for all input in the game.
    /// DO NOT CHANGE the "MARKER" lines - they assist with code auto-generation.
    /// </summary>
    public static class Input
    {
        #region Fields & Properties
        
        // MARKER.RuntimeInputAddress.Start
        private const string RUNTIME_INPUT_DATA_PATH = "RuntimeInputData";
        // MARKER.RuntimeInputAddress.End
        
        // This event will invoke regardless of contexts/maps being enabled/disabled.
        public static event Action OnAnyButtonPressed;

        public static bool AllowPlayerJoining { get; set; } = false;
        public static bool UseContextEventSystemActions => runtimeInputData.UseContextEventSystemActions;
        
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.Start
        public static event Action<ControlScheme> OnControlSchemeChanged
        {
            add => primaryPlayer.OnControlSchemeChanged += value;
            remove => primaryPlayer.OnControlSchemeChanged -= value;
        }
        public static event Action<char> OnKeyboardTextInput
        {
            add => primaryPlayer.OnKeyboardTextInput += value;
            remove => primaryPlayer.OnKeyboardTextInput -= value;
        }
        public static PlayerActions Player => primaryPlayer.Player;
        public static UIActions UI => primaryPlayer.UI;
        public static InputContext CurrentContext => primaryPlayer.CurrentContext;
        public static ControlScheme CurrentControlScheme => primaryPlayer.CurrentControlScheme;
        public static void EnableContext(InputContext context) => primaryPlayer.EnableContext(context);
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.End
        
        // MARKER.DefaultContextProperty.Start
        private static InputContext DefaultContext => InputContext.AllInputDisabled;
        // MARKER.DefaultContextProperty.End

        private static InputActionAsset PrimaryInputActionAsset => runtimeInputData.InputActionAsset;
        
        private static InputPlayerCollection playerCollection;
        private static InputPlayer primaryPlayer;
        private static RuntimeInputData runtimeInputData;
        private static PlayerInput primaryPlayerInput;
        private static InputSystemUIInputModule primaryUIInputModule;
        private static IDisposable anyButtonPressListener;
        
        #endregion

        #region Setup
        
        public static void InitializeBeforeSceneLoad()
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
            runtimeInputData = Resources.Load<RuntimeInputData>(RUNTIME_INPUT_DATA_PATH);
            InputActionAsset asset = runtimeInputData.InputActionAsset;
            if (asset == null)
            {
                throw new Exception($"Input manager's {nameof(runtimeInputData)} is missing an input action asset!");
            }

            playerCollection = new InputPlayerCollection();
            primaryPlayer = playerCollection.CreateAndAddPlayer(asset);
        }

        public static void InitializeAfterSceneLoad()
        {
            primaryPlayerInput = Object.FindObjectOfType<PlayerInput>();
            primaryUIInputModule = Object.FindObjectOfType<InputSystemUIInputModule>();

            if (primaryPlayerInput == null || primaryUIInputModule == null)
            {
                GameObject inputMgmtGameObject = new GameObject("Player [0] Input Management");
                if (primaryPlayerInput == null)
                    primaryPlayerInput = inputMgmtGameObject.AddComponent<PlayerInput>();
                if (primaryUIInputModule == null)
                    primaryUIInputModule = inputMgmtGameObject.AddComponent<InputSystemUIInputModule>();
                Object.DontDestroyOnLoad(inputMgmtGameObject);
            }
            
            primaryPlayer.PlayerInput = primaryPlayerInput;
            primaryPlayer.UIInputModule = primaryUIInputModule;
            primaryPlayerInput.actions = PrimaryInputActionAsset;
            primaryPlayerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            
            playerCollection.EnableContextForAll(DefaultContext);
            AddSubscriptions();
        }

        private static void Terminate()
        {
            RemoveSubscriptions();
            playerCollection.TerminateAll();
        }

        private static void AddSubscriptions()
        {
            anyButtonPressListener = InputSystem.onAnyButtonPress.Call(HandleAnyButtonPressed);
        }

        private static void RemoveSubscriptions()
        {
            anyButtonPressListener.Dispose();
        }

        #endregion

        #region Public Interface

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

            if (!runtimeInputData.BindingDataReferences.TryGetValue(controlScheme, out BindingDataAssetReference bindingDataAssetReference))
            {
                return false;
            }
            
            BindingData bindingData = bindingDataAssetReference.LoadAssetSynchronous<BindingData>();
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

        public static void ChangeSubscription(InputActionReference actionReference, Action<InputAction.CallbackContext> callback, bool subscribe)
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

        private static void HandleAnyButtonPressed(InputControl inputControl)
        {
            OnAnyButtonPressed?.Invoke();

            if (!AllowPlayerJoining)
            {
                return;
            }

            InputDevice device = inputControl.device;

            // Ignore presses on devices that are already used by a player.
            if (PlayerInput.FindFirstPairedToDevice(device) != null)
            {
                return;
            }
            
            CreateNewNonPrimaryPlayer(device);
        }

        private static void CreateNewNonPrimaryPlayer(InputDevice device)
        {
            // Note that 'PlayerInput.Instantiate' creates a new copy of the input actions asset for the new player.
            PlayerInput playerInput = PlayerInput.Instantiate(primaryPlayerInput.gameObject, pairWithDevice: device);
            
            // (If the player did not end up with a valid input setup, it will return null)
            if (playerInput == null)
            {
                return;
            }
            
            InputPlayer newPlayer = playerCollection.CreateAndAddPlayer(playerInput.actions);
            GameObject playerInputGameObject = playerInput.gameObject;
            playerInputGameObject.name = $"Player [{newPlayer.ID}] Input Management";
            newPlayer.PlayerInput = playerInput;
            newPlayer.UIInputModule = playerInputGameObject.AddComponent<InputSystemUIInputModule>();
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            Object.DontDestroyOnLoad(playerInputGameObject);
        }

        #endregion
    }
}
