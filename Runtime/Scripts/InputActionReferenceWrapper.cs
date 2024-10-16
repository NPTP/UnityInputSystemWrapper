using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper
{
    [Serializable]
    public class InputActionReferenceWrapper
    {
        [SerializeField] private InputActionReference internalReference;
        internal InputActionReference InternalReference => internalReference;
        
        // TODO (cleanup): can probably remove this property, unlikely to need it beyond the debugging stages.
        public string ActionName => internalReference.action.name;

        public event Action<InputAction.CallbackContext> OnAction
        {
            add => Input.ChangeSubscription(internalReference, value, subscribe: true);
            remove => Input.ChangeSubscription(internalReference, value, subscribe: false);
        }
    }
}
