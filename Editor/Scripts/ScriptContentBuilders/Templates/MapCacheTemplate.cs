// MARKER.Ignore.Start
using System;
// MARKER.Ignore.End
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Generated.MapActions;
using UnityEngine.InputSystem;

// MARKER.Ignore.Start
// ---------------------------------- WARNING ! ---------------------------------------
// This template script is used to auto-generate new C# input classes and their respective
// .cs files. Do not modify it unless you know what you're doing!
// MARKER.Ignore.End

// MARKER.GeneratorNotice.Start
// MARKER.GeneratorNotice.End
// MARKER.Namespace.Start
namespace NPTP.InputSystemWrapper.Generated.MapCaches
// MARKER.Namespace.End
{
    // MARKER.ClassSignature.Start
    public class MapCacheTemplate
    // MARKER.ClassSignature.End
    {
        // MARKER.InterfacesList.Start
        private readonly List<ITemplateActions> interfaces = new();
        // MARKER.InterfacesList.End

        public InputActionMap ActionMap { get; }
        public void Enable() => ActionMap.Enable();
        public void Disable() => ActionMap.Disable();

        // MARKER.InputActionFields.Start
        public InputAction TemplateAction1 { get; }
        public InputAction TemplateAction2 { get; }
        public InputAction TemplateAction3 { get; }
        // MARKER.InputActionFields.End

        // MARKER.ConstructorSignature.Start
        public MapCacheTemplate(InputActionAsset asset)
        // MARKER.ConstructorSignature.End
        {
            // MARKER.ActionMapAssignment.Start
            ActionMap = asset.FindActionMap("Gameplay", throwIfNotFound: true);
            // MARKER.ActionMapAssignment.End
            
            // MARKER.InputActionAssignments.Start
            TemplateAction1 = ActionMap.FindAction("TemplateAction1", throwIfNotFound: true);
            TemplateAction2 = ActionMap.FindAction("TemplateAction2", throwIfNotFound: true);
            TemplateAction3 = ActionMap.FindAction("TemplateAction3", throwIfNotFound: true);
            // MARKER.InputActionAssignments.End
            // MARKER.Ignore.Start
            throw new NotImplementedException($"This template class {nameof(MapCacheTemplate)} should never be instantiated!");
            // MARKER.Ignore.End
        }

        // MARKER.AddCallbacksSignature.Start
        public void AddCallbacks(ITemplateActions instance)
        // MARKER.AddCallbacksSignature.End
        {
            if (instance == null || interfaces.Contains(instance))
            {
                return;
            }
            
            interfaces.Add(instance);
            
            // MARKER.ActionsSubscribe.Start
            TemplateAction1.started += instance.OnTemplateAction1;
            TemplateAction1.performed += instance.OnTemplateAction1;
            TemplateAction1.canceled += instance.OnTemplateAction1;
            TemplateAction2.started += instance.OnTemplateAction2;
            TemplateAction2.performed += instance.OnTemplateAction2;
            TemplateAction2.canceled += instance.OnTemplateAction2;
            // MARKER.ActionsSubscribe.End
        }

        // MARKER.RemoveCallbacksSignature.Start
        public void RemoveCallbacks(ITemplateActions instance)
        // MARKER.RemoveCallbacksSignature.End
        {
            if (!interfaces.Remove(instance))
            {
                return;
            }

            // MARKER.ActionsUnsubscribe.Start
            TemplateAction1.started -= instance.OnTemplateAction1;
            TemplateAction1.performed -= instance.OnTemplateAction1;
            TemplateAction1.canceled -= instance.OnTemplateAction1;
            TemplateAction2.started -= instance.OnTemplateAction2;
            TemplateAction2.performed -= instance.OnTemplateAction2;
            TemplateAction2.canceled -= instance.OnTemplateAction2;
            // MARKER.ActionsUnsubscribe.End
        }
    }
}