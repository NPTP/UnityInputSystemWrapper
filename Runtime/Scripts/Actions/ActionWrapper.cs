using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Actions
{
    public class ActionWrapper
    {
        internal InputAction InputAction { get; }
        
        private event Action<InputAction.CallbackContext> onEvent;
        public event Action<InputAction.CallbackContext> OnEvent
        {
            add { onEvent -= value; onEvent += value; }
            remove => onEvent -= value;
        }
        
        public bool DownThisFrame => InputAction.WasPerformedThisFrame();
        public bool IsDown => InputAction.phase == InputActionPhase.Performed;

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

        internal ActionWrapper(InputAction inputAction)
        {
            InputAction = inputAction;
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

        private void HandleActionEvent(InputAction.CallbackContext context) => onEvent?.Invoke(context);

        public static implicit operator ActionWrapper(ActionReference actionReference)
        {
            return Input.GetActionWrapperFromReference(actionReference);
        }
    }
}
