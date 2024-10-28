using System;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper
{
    [Serializable]
    public class ActionReference
    {
        [SerializeField] private InputActionReference reference;
        [SerializeField] private bool useCompositePart; 
        [SerializeField] private CompositePart compositePart;
        
        public bool UseCompositePart => useCompositePart;
        public CompositePart CompositePart => compositePart;
        
        internal InputActionReference InternalReference => reference;
        
        public event Action<InputAction.CallbackContext> OnEvent
        {
            add => Input.ChangeSubscription(reference, value, subscribe: true);
            remove => Input.ChangeSubscription(reference, value, subscribe: false);
        }

        /// <summary>
        /// Convert an InputActionReference to this type ActionReference.
        /// </summary>
        public static implicit operator ActionReference(InputActionReference inputActionReference)
        {
            return new ActionReference(inputActionReference.action);
        }
        
        /// <summary>
        /// Convert an InputAction to an ActionReference.
        /// </summary>
        public static implicit operator ActionReference(InputAction inputAction)
        {
            return new ActionReference(inputAction);
        }
        
        private ActionReference(InputAction action)
        {
            reference = InputActionReference.Create(action);
        }
    }
}
