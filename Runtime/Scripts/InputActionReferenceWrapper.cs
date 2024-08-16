using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper
{
    [Serializable]
    public struct InputActionReferenceWrapper
    {
        [SerializeField] private InputActionReference internalReference;
        
        public event Action<InputAction.CallbackContext> OnAction
        {
            add => Input.ChangeSubscription(internalReference, value, subscribe: true);
            remove => Input.ChangeSubscription(internalReference, value, subscribe: false);
        }

        // TODO: Remove after testing. The action itself should not be accessible here to avoid multiple sources of truth for input.
        public InputAction InputAction => internalReference.action;
        
#if UNITY_EDITOR
        public static string EDITOR_GetInternalReferenceName() => nameof(internalReference);
#endif
    }
}
