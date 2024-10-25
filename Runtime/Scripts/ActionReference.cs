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
        
        // TODO (cleanup): can probably remove this property, unlikely to need it beyond the debugging stages.
        public string ActionName => reference.action.name;

        public event Action<InputAction.CallbackContext> OnAction
        {
            add => Input.ChangeSubscription(reference, value, subscribe: true);
            remove => Input.ChangeSubscription(reference, value, subscribe: false);
        }
    }
}
