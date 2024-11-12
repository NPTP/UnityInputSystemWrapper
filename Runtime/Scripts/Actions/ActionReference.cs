using System;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Actions
{
    /// <summary>
    /// For referencing InputActions in the inspector, and being able to use these references in a way
    /// that actually refers to the same InputAction instances in the runtime player input assets instead
    /// of some arbitrary instance.
    /// </summary>
    [Serializable]
    public class ActionReference
    {
        [SerializeField] private InputActionReference reference;
        
        [SerializeField] private bool useCompositePart; 
        public bool UseCompositePart => useCompositePart;
        
        [SerializeField] private CompositePart compositePart;
        public CompositePart CompositePart => compositePart;
        
        internal InputAction InternalAction => reference.action;
        
        private ActionWrapper action;
        public ActionWrapper Action => action ??= Input.GetActionWrapperFromReference(this);

        /// <summary>
        /// Convert InputAction -> ActionReference.
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
