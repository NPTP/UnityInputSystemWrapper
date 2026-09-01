using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;

using NPTP.InputSystemWrapper;

namespace NPTP.InputSystemWrapper.Actions
{
    /// <summary>
    /// Essential class containing a particular player's InputActions.
    /// Can never be instantiated by the user - it only exists at runtime if it exists in the input actions asset.
    /// As such, when we want access to one of these, we either access it directly (e.g. Input.Gameplay.Fire) or
    /// find it using TryConvert.
    /// </summary>
    public class ActionWrapper
    {
        internal int PlayerID { get; }
        internal InputAction InputAction { get; }

        private event Action<ActionEventInfo> onEvent;
        public event Action<ActionEventInfo> OnEvent
        {
            add { onEvent -= value; onEvent += value; }
            remove => onEvent -= value;
        }

        public bool DownThisFrame => InputAction.WasPerformedThisFrame() && (InputAction.type != InputActionType.PassThrough || !InputAction.WasReleasedThisFrame());
        public bool IsDown => InputAction.phase == InputActionPhase.Performed;

        internal ActionWrapper(int playerID, InputAction inputAction, Dictionary<Guid, ActionWrapper> table)
        {
            PlayerID = playerID;
            InputAction = inputAction;
            table.Add(inputAction.id, this);
        }

        internal void RegisterCallbacks()
        {
            InputAction.started += HandleActionEvent;
            InputAction.performed += HandleActionEvent;
            InputAction.canceled += HandleActionEvent;
        }

        internal void UnregisterCallbacks()
        {
            InputAction.started -= HandleActionEvent;
            InputAction.performed -= HandleActionEvent;
            InputAction.canceled -= HandleActionEvent;
        }

        internal void StartInteractiveRebind(ControlSchemeId controlSchemeId, int uiIndex, Action<RebindInfo> callback = null) =>
            InputRuntime.Current.StartInteractiveRebind(new ActionBindingInfo(this, CompositePart.DontIsolatePart, controlSchemeId, uiIndex), callback);

        internal void StartInteractiveRebind(ControlSchemeId controlSchemeId, CompositePart compositePart, int uiIndex, Action<RebindInfo> callback = null) =>
            InputRuntime.Current.StartInteractiveRebind(new ActionBindingInfo(this, compositePart, controlSchemeId, uiIndex), callback);

        internal void ResetBinding(ControlSchemeId controlSchemeId, int uiIndex) =>
            InputRuntime.Current.ResetBindingForAction(this, CompositePart.DontIsolatePart, controlSchemeId, uiIndex);

        internal void ResetAllBindings(ControlSchemeId controlSchemeId) =>
            InputRuntime.Current.ResetAllBindingsForAction(this, CompositePart.DontIsolatePart, controlSchemeId);

        /// <summary>Every slot of this action on the control scheme the player is currently using.</summary>
        public BindingSlots GetCurrentBindingSlots() => InputRuntime.Current.GetCurrentBindingSlots(this);

        /// <summary>
        /// The same slots, loaded in the background: the callback runs once they are ready.
        /// </summary>
        public void GetCurrentBindingSlotsAsync(Action<BindingSlots> onResolved) =>
            InputRuntime.Current.GetCurrentBindingSlotsAsync(this, onResolved);

        internal BindingSlots GetBindingSlots(ControlSchemeId controlSchemeId) =>
            InputRuntime.Current.GetBindingSlots(this, controlSchemeId);

        private void HandleActionEvent(InputAction.CallbackContext context) => onEvent?.Invoke(new ActionEventInfo(this, context));
    }
}
