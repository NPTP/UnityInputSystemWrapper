using System;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Actions
{
    public abstract class ActionWrapper
    {
        internal InputAction InputAction { get; }
        
        // MARKER.ActionReferenceProperty.Start
        // MARKER.ActionReferenceProperty.End
        
        private event Action<InputAction.CallbackContext> onEvent;
        public event Action<InputAction.CallbackContext> OnEvent
        {
            add { onEvent -= value; onEvent += value; }
            remove => onEvent -= value;
        }

        internal ActionWrapper(InputAction inputAction)
        {
            InputAction = inputAction;
            
            // MARKER.ActionReferenceInitializer.Start
            // MARKER.ActionReferenceInitializer.End
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
    }
}
