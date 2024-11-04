using NPTP.InputSystemWrapper.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using Button = UnityEngine.InputSystem.HID.HID.Button;
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
        
        // MARKER.ActionWrapperPublicProperties.Start
        public ActionWrapper TemplateAction1 { get; }
        public ValueActionWrapper<Vector2> TemplateAction2 { get; }
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
            TemplateAction1 = new (ActionMap.FindAction("TemplateAction1", throwIfNotFound: true));
            TemplateAction2 = new (ActionMap.FindAction("TemplateAction2", throwIfNotFound: true));
            // MARKER.ActionWrapperAssignments.End
            // MARKER.Ignore.Start
            throw new System.NotImplementedException($"This template class {nameof(ActionsTemplate)} should never be instantiated!");
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
            
            // MARKER.RegisterCallbacks.Start
            TemplateAction1.RegisterCallbacks();
            TemplateAction2.RegisterCallbacks();
            // MARKER.RegisterCallbacks.End
        }
        
        internal void DisableAndUnregisterCallbacks()
        {
            if (!enabled)
            {
                return;
            }

            enabled = false;
            ActionMap.Disable();

            // MARKER.UnregisterCallbacks.Start
            TemplateAction1.UnregisterCallbacks();
            TemplateAction2.UnregisterCallbacks();
            // MARKER.UnregisterCallbacks.End
        }
    }
}