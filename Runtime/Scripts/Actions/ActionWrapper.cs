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
        internal InputAction InputAction { get; }
        
        private event Action<InputAction.CallbackContext> onEvent;
        public event Action<InputAction.CallbackContext> OnEvent
        {
            add { onEvent -= value; onEvent += value; }
            remove => onEvent -= value;
        }
        
        public bool DownThisFrame => InputAction.WasPerformedThisFrame() && (InputAction.type != InputActionType.PassThrough || !InputAction.WasReleasedThisFrame());
        public bool IsDown => InputAction.phase == InputActionPhase.Performed;
        
        public static bool TryConvert(InputActionReference inputActionReference, out ActionWrapper actionWrapper)
        {
            if (inputActionReference != null && inputActionReference.action != null)
            {
                actionWrapper = Input.GetActionWrapperFromReference(inputActionReference);
                return actionWrapper != null;
            }

            actionWrapper = null;
            return false;
        }
        
        public bool TryGetCurrentBindingInfo(out IEnumerable<BindingInfo> bindingInfos)
        {
            return Input.TryGetCurrentBindingInfo(this, out bindingInfos);
        }

        public bool TryGetCurrentBindingInfo(CompositePart compositePart, out IEnumerable<BindingInfo> bindingInfos)
        {
            return Input.TryGetCurrentBindingInfo(this, compositePart, out bindingInfos);
        }
        
        public bool TryGetBindingInfo(ControlScheme controlScheme, out IEnumerable<BindingInfo> bindingInfos)
        {
            return Input.TryGetBindingInfo(this, controlScheme, out bindingInfos);
        }

        public bool TryGetBindingInfo(ControlScheme controlScheme, CompositePart compositePart, out IEnumerable<BindingInfo> bindingInfos)
        {
            return Input.TryGetBindingInfo(this, controlScheme, compositePart, out bindingInfos);
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
        
        internal ActionWrapper(InputAction inputAction)
        {
            InputAction = inputAction;
        }

        private void HandleActionEvent(InputAction.CallbackContext context) => onEvent?.Invoke(context);
    }
}
