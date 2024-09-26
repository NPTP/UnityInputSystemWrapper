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
        
        public event Action<InputAction.CallbackContext> OnAction
        {
            add => Input.ChangeSubscription(internalReference, value, subscribe: true);
            remove => Input.ChangeSubscription(internalReference, value, subscribe: false);
        }
    }
}
