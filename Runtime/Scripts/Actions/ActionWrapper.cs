using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;

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

        public void StartInteractiveRebind(ControlScheme controlScheme, Action<RebindInfo> callback = null) =>
            ISW.StartInteractiveRebind(new ActionBindingInfo(this, CompositePart.DontIsolatePart, controlScheme), callback);

        public void StartInteractiveRebind(ControlScheme controlScheme, CompositePart compositePart, Action<RebindInfo> callback = null) =>
            ISW.StartInteractiveRebind(new ActionBindingInfo(this, compositePart, controlScheme), callback);

        public bool TryGetCurrentBindingInfo(out IEnumerable<BindingInfo> bindingInfos) =>
            ISW.TryGetCurrentBindingInfo(this, CompositePart.DontIsolatePart, out bindingInfos);

        public bool TryGetCurrentBindingInfo(CompositePart compositePart, out IEnumerable<BindingInfo> bindingInfos) =>
            ISW.TryGetCurrentBindingInfo(this, compositePart, out bindingInfos);

        public bool TryGetBindingInfo(ControlScheme controlScheme, out IEnumerable<BindingInfo> bindingInfos) =>
            ISW.TryGetBindingInfo(new ActionBindingInfo(this, CompositePart.DontIsolatePart, controlScheme), out bindingInfos);

        public bool TryGetBindingInfo(ControlScheme controlScheme, CompositePart compositePart, out IEnumerable<BindingInfo> bindingInfos) =>
            ISW.TryGetBindingInfo(new ActionBindingInfo(this, compositePart, controlScheme), out bindingInfos);

        private void HandleActionEvent(InputAction.CallbackContext context) => onEvent?.Invoke(new ActionEventInfo(this, context));
    }
}
