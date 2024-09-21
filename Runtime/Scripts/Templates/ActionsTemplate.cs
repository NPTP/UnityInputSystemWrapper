using System;
using UnityEngine.InputSystem;
// MARKER.Ignore.Start
// ---------------------------------- WARNING ! ---------------------------------------
// This template script is used to auto-generate new C# input classes and their respective
// .cs files. Do not modify it unless you know what you're doing!
// MARKER.Ignore.End

// MARKER.GeneratorNotice.Start
// MARKER.GeneratorNotice.End
// MARKER.Namespace.Start
namespace NPTP.InputSystemWrapper.Generated
// MARKER.Namespace.End
{
    // MARKER.ClassSignature.Start
    public class ActionsTemplate
    // MARKER.ClassSignature.End
    {
        internal InputActionMap ActionMap { get; }
        
        // MARKER.PublicEvents.Start
        private event Action<InputAction.CallbackContext> @_OnTemplateAction1;
        public event Action<InputAction.CallbackContext> @OnTemplateAction1
        {
            add { _OnTemplateAction1 -= value; _OnTemplateAction1 += value; }
            remove => _OnTemplateAction1 -= value;
        }
        
        private event Action<InputAction.CallbackContext> @_OnTemplateAction2;
        public event Action<InputAction.CallbackContext> @OnTemplateAction2
        {
            add { _OnTemplateAction2 -= value; _OnTemplateAction2 += value; }
            remove => _OnTemplateAction2 -= value;
        }
        // MARKER.PublicEvents.End

        // MARKER.ActionWrapperPublicProperties.Start
        public ActionWrapper TemplateAction1 { get; }
        public ActionWrapper TemplateAction2 { get; }
        // MARKER.ActionWrapperPublicProperties.End
        
        private bool enabled;
        
        // MARKER.ConstructorSignature.Start
        internal ActionsTemplate(InputActionAsset asset)
        // MARKER.ConstructorSignature.End
        {
            // MARKER.ActionMapAssignment.Start
            ActionMap = asset.FindActionMap("TemplateMap1", throwIfNotFound: true);
            // MARKER.ActionMapAssignment.End
            
            // MARKER.ActionWrapperAssignments.Start
            TemplateAction1 = new ActionWrapper(ActionMap.FindAction("TemplateAction1", throwIfNotFound: true));
            TemplateAction2 = new ActionWrapper(ActionMap.FindAction("TemplateAction2", throwIfNotFound: true));
            // MARKER.ActionWrapperAssignments.End
            // MARKER.Ignore.Start
            throw new NotImplementedException($"This template class {nameof(ActionsTemplate)} should never be instantiated!");
            // MARKER.Ignore.End
        }
        
        internal void EnableAndRegisterCallbacks()
        {
            if (enabled)
            {
                return;
            }

            enabled = true;
            ActionMap.Enable();
            
            // MARKER.ActionsSubscribe.Start
            TemplateAction1.InputAction.started += HandleTemplateAction1;
            TemplateAction1.InputAction.performed += HandleTemplateAction1;
            TemplateAction1.InputAction.canceled += HandleTemplateAction1;
            TemplateAction2.InputAction.started += HandleTemplateAction2;
            TemplateAction2.InputAction.performed += HandleTemplateAction2;
            TemplateAction2.InputAction.canceled += HandleTemplateAction2;
            // MARKER.ActionsSubscribe.End
        }
        
        internal void DisableAndUnregisterCallbacks()
        {
            if (!enabled)
            {
                return;
            }

            enabled = false;
            ActionMap.Disable();

            // MARKER.ActionsUnsubscribe.Start
            TemplateAction1.InputAction.started -= HandleTemplateAction1;
            TemplateAction1.InputAction.performed -= HandleTemplateAction1;
            TemplateAction1.InputAction.canceled -= HandleTemplateAction1;
            TemplateAction2.InputAction.started -= HandleTemplateAction2;
            TemplateAction2.InputAction.performed -= HandleTemplateAction2;
            TemplateAction2.InputAction.canceled -= HandleTemplateAction2;
            // MARKER.ActionsUnsubscribe.End
        }

        // MARKER.PrivateEventHandlers.Start
        private void HandleTemplateAction1(InputAction.CallbackContext context) => _OnTemplateAction1?.Invoke(context);
        private void HandleTemplateAction2(InputAction.CallbackContext context) => _OnTemplateAction2?.Invoke(context);
        // MARKER.PrivateEventHandlers.End
    }
}