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
namespace NPTP.InputSystemWrapper.Generated.MapActions
// MARKER.Namespace.End
{
    // MARKER.InterfaceName.Start
    public interface ITemplateActions
    // MARKER.InterfaceName.End
    {
        // MARKER.InterfaceMembers.Start
        void OnTemplateAction1(InputAction.CallbackContext context);
        void OnTemplateAction2(InputAction.CallbackContext context);
        // MARKER.InterfaceMembers.End
    }
    
    // MARKER.ClassSignature.Start
    public class MapActionsTemplate : ITemplateActions
    // MARKER.ClassSignature.End
    {
        // MARKER.Ignore.Start
        public MapActionsTemplate() => throw new NotImplementedException($"This template class {nameof(MapActionsTemplate)} should never be instantiated!");
        // MARKER.Ignore.End
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

        // MARKER.InterfaceMethodImplementations.Start
        void ITemplateActions.OnTemplateAction1(InputAction.CallbackContext context) => _OnTemplateAction1?.Invoke(context);
        void ITemplateActions.OnTemplateAction2(InputAction.CallbackContext context) => _OnTemplateAction2?.Invoke(context);
        // MARKER.InterfaceMethodImplementations.End
    }
}