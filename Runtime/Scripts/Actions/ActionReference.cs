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
        [SerializeField] private CompositePart compositePart;
        
        public bool UseCompositePart => useCompositePart;
        public CompositePart CompositePart => compositePart;
        
        internal InputAction InternalAction => reference.action;
        
        private ActionWrapper actionWrapper;
        private ActionWrapper ActionWrapper => actionWrapper ??= Input.GetActionWrapperFromReference(this);

        public event Action<InputAction.CallbackContext> OnEvent
        {
            add => ActionWrapper.OnEvent += value;
            remove => ActionWrapper.OnEvent -= value;
        }

        /// <summary>
        /// Convert InputAction -> ActionReference.
        /// </summary>
        public static implicit operator ActionReference(InputAction inputAction)
        {
            return new ActionReference(inputAction);
        }

        /// <summary>
        /// Convert ActionWrapper -> ActionReference.
        /// </summary>
        public static implicit operator ActionReference(ActionWrapper actionWrapper)
        {
            return new ActionReference(actionWrapper.InputAction);
        }

        private ActionReference(InputAction action)
        {
            reference = InputActionReference.Create(action);
        }
    }
}
