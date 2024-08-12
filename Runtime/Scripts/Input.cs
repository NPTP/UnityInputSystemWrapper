using System;
using System.Collections.Generic;
using InputSystemWrapper.Utilities.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
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
    /// </summary>
    public static class Input
    {
        #region Fields & Properties
        
        // MARKER.RuntimeInputDataPath.Start
        private const string RUNTIME_INPUT_DATA_PATH = "RuntimeInputData";
        // MARKER.RuntimeInputDataPath.End
        
        // This event will invoke regardless of contexts/maps being enabled/disabled.
        public static event Action OnAnyButtonPressed;

        public static bool AllowPlayerJoining { get; set; } = false;
        
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.Start
        public static InputPlayer GetPlayer(PlayerID id) => playerCollection[id];
        // MARKER.SingleOrMultiPlayerFieldsAndProperties.End
        
        // MARKER.DefaultContextProperty.Start
        private static InputContext DefaultContext => InputContext.AllInputDisabled;
        // MARKER.DefaultContextProperty.End

        private static InputPlayerCollection playerCollection;
        private static RuntimeInputData runtimeInputData;
        private static IDisposable anyButtonPressListener;

        #endregion

        #region Setup

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeBeforeSceneLoad()
        {
            SetUpTerminationConditions();
            runtimeInputData = Resources.Load<RuntimeInputData>(RUNTIME_INPUT_DATA_PATH);
            InputActionAsset asset = runtimeInputData.InputActionAsset;
            if (asset == null) throw new Exception($"{runtimeInputData.GetType().Name} is missing its input action asset!");
            int maxPlayers = Enum.GetValues(typeof(PlayerID)).Length;
            playerCollection = new InputPlayerCollection(asset, maxPlayers);
            
            Object.FindObjectsOfType<PlayerInput>().DestroyAll();
            Object.FindObjectsOfType<InputSystemUIInputModule>().DestroyAll();
            playerCollection.SetUpSceneObjects();
            EnableContextForAllPlayers(DefaultContext);
            AddAnyButtonPressListener();
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
            --InputUser.listenForUnpairedDeviceActivity;
            RemoveAnyButtonPressListener();
            playerCollection.TerminateAll();
        }

        private static void AddAnyButtonPressListener()
        {
            // TODO: Bug in Input System 1.3.0 where text inputs cause an exception with this listener.
            // anyButtonPressListener = InputSystem.onAnyButtonPress.Call(HandleAnyButtonPressed);
        }

        private static void RemoveAnyButtonPressListener()
        {
            // TODO: Bug in Input System 1.3.0 where text inputs cause an exception with this listener.
            // anyButtonPressListener.Dispose();
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

        // TODO: Hide certain methods in internal assembly, like this one that should only be called from InputActionReferenceWrapper
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

            // Ignore presses on devices that are already being used by a player.
            if (playerCollection.IsDeviceCurrentlyUsed(device))
            {
                Debug.Log($"Device {device.name} is currently in use");
                return;
            }
            
            CreateNewNonPrimaryPlayer(device);
        }

        private static void CreateNewNonPrimaryPlayer(InputDevice device)
        {
            // TODO
        }

        #endregion
    }
}
