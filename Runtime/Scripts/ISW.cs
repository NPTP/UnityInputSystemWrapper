using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.AnyButtonPress;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Player;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper
{
    /// <summary>
    /// Main point of usage for all input in the game.
    /// ISW stands for "Input System Wrapper".
    /// This is a facade with no state of its own - everything lives on <see cref="InputRuntime"/>.
    /// </summary>
    public static partial class ISW
    {
        private static InputRuntime Runtime => InputRuntime.Current;
        private static InputPlayer DefaultPlayer => Runtime.DefaultPlayer;

        #region Events

        /// <summary>
        /// For use with any localization system in your project: handle this event by taking the passed request,
        /// using localizationKey to find the right string in your localization system, and setting localizedDisplayName
        /// to that string.
        /// </summary>
        public static event Action<LocalizedStringRequest> OnLocalizedStringRequested
        {
            add => Runtime.OnLocalizedStringRequested += value;
            remove => Runtime.OnLocalizedStringRequested -= value;
        }

        /// <summary>
        /// Use as a general purpose catch-all for when to update any UI that displays controls.
        /// Invoked on InputUserChange, on ControlScheme change, and on bindings changed.
        /// </summary>
        public static event Action OnControlsUpdated
        {
            add => Runtime.OnControlsUpdated += value;
            remove => Runtime.OnControlsUpdated -= value;
        }

        /// <summary>
        /// Invoked on any button pressed on any connected device regardless of actions mapped, assets enabled, etc.
        /// </summary>
        public static event AnyButtonPressListener OnAnyButtonPress
        {
            add => Runtime.OnAnyButtonPress += value;
            remove => Runtime.OnAnyButtonPress -= value;
        }

        public static event Action OnBindingsChanged
        {
            add => Runtime.OnBindingsChanged += value;
            remove => Runtime.OnBindingsChanged -= value;
        }

        public static event Action<InputUserChangeInfo> OnAnyPlayerInputUserChange
        {
            add => Runtime.OnAnyPlayerInputUserChange += value;
            remove => Runtime.OnAnyPlayerInputUserChange -= value;
        }

        public static event Action<InputPlayer> OnAnyPlayerControlSchemeChanged
        {
            add => Runtime.OnAnyPlayerControlSchemeChanged += value;
            remove => Runtime.OnAnyPlayerControlSchemeChanged -= value;
        }

        public static event Action<char> OnAnyPlayerKeyboardTextInput
        {
            add => Runtime.OnAnyPlayerKeyboardTextInput += value;
            remove => Runtime.OnAnyPlayerKeyboardTextInput -= value;
        }

        #endregion

        #region Public Interface

        public static bool AllowPlayerJoining
        {
            get => Runtime.AllowPlayerJoining;
            set => Runtime.AllowPlayerJoining = value;
        }

        public static Vector2 MousePosition => Mouse.current.position.ReadValue();

        public static InputPlayer GetPlayer(int playerID) => Runtime.GetPlayer(playerID);
        public static void AddPlayer(int playerID) => Runtime.AddPlayer(playerID);
        public static void RemovePlayer(int playerID) => Runtime.RemovePlayer(playerID);

        public static bool ControlSchemeHas<TDevice>(ControlScheme controlScheme, int playerID = 0) where TDevice : InputDevice =>
            Runtime.ControlSchemeHas<TDevice>(controlScheme, playerID);

        public static void SetContextForAllPlayers(InputContext inputContext) => Runtime.SetContextForAllPlayers(inputContext);

        /// <summary>
        /// Try to get the ActionWrapper for the (deprecated) InputActionReference's action.
        /// Useful as a transitional tool from normal Unity Input System usage to full ISW integration.
        /// </summary>
        // TODO: remove this method eventually
        public static bool TryConvert(InputActionReference inputActionReference, int playerID, out ActionWrapper actionWrapper) =>
            Runtime.TryConvert(inputActionReference, playerID, out actionWrapper);

        /// <summary>
        /// Single-player overload
        /// </summary>
        public static bool TryConvert(InputActionReference inputActionReference, out ActionWrapper actionWrapper) =>
            Runtime.TryConvert(inputActionReference, out actionWrapper);

        public static void ResetBindingForAction(ActionReference actionReference, ControlScheme controlScheme) =>
            Runtime.ResetBindingForAction(actionReference, controlScheme);

        public static void ResetAllBindingsForControlScheme(ControlScheme controlScheme, int? playerID = null) =>
            Runtime.ResetAllBindingsForControlScheme(controlScheme, playerID);

        public static void LoadAllBindings(int? playerID = null) => Runtime.LoadAllBindings(playerID);
        public static void SaveAllBindings(int? playerID = null) => Runtime.SaveAllBindings(playerID);
        public static void ResetAllBindings(int? playerID = 0) => Runtime.ResetAllBindings(playerID);

        #endregion

        #region Editor-Only Debug
#if UNITY_EDITOR
        internal static event Action<int, InputContext> EDITOR_OnPlayerInputContextChanged
        {
            add => InputRuntime.EDITOR_OnPlayerInputContextChanged += value;
            remove => InputRuntime.EDITOR_OnPlayerInputContextChanged -= value;
        }

        internal static bool EDITOR_IsInitialized => Runtime is { EDITOR_IsInitialized: true };
        internal static InputContext EDITOR_GetDefaultContext() => Runtime.EDITOR_GetDefaultContext();
        internal static bool EDITOR_TryGetPlayer(int playerID, out InputPlayer inputPlayer)
        {
            if (Runtime == null)
            {
                inputPlayer = default;
                return false;
            }

            return Runtime.EDITOR_TryGetPlayer(playerID, out inputPlayer);
        }
#endif
        #endregion
    }
}
