using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
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
        
        // TODO (multiplayer): Ability to tie the reference to a particular player ID (ie change to serialized field with an "AllPlayers" option?)
        internal PlayerID PlayerID => PlayerID.Player1;
        
        private ActionWrapper actionWrapper;
        public ActionWrapper ActionWrapper
        {
            get
            {
                if (actionWrapper != null)
                    return actionWrapper;

                if (reference == null || reference.action == null)
                    return null;
                
                Input.TryGetActionWrapper(PlayerID, reference.action, out actionWrapper);
                return actionWrapper;
            }
        }
        
        public static bool TryConvert(InputActionReference inputActionReference, out ActionReference actionReference)
        {
            if (inputActionReference != null && inputActionReference.action != null &&
                Input.TryConvert(inputActionReference, out ActionWrapper actionWrapper))
            {
                actionReference = new ActionReference(inputActionReference.action) { actionWrapper = actionWrapper };
                return true;
            }

            actionReference = null;
            return false;
        }
        
        public static bool TryConvert(InputAction inputAction, out ActionReference actionReference)
        {
            if (inputAction != null && Input.TryGetActionWrapper(PlayerID.Player1, inputAction, out ActionWrapper actionWrapper))
            {
                actionReference = new ActionReference(inputAction) { actionWrapper = actionWrapper };
                return true;
            }

            actionReference = null;
            return false;
        }
        
        public bool TryGetCurrentBindingInfo(out IEnumerable<BindingInfo> bindingInfos)
        {
            if (ActionWrapper == null)
            {
                bindingInfos = null;
                return false;
            }

            if (useCompositePart)
                return ActionWrapper.TryGetCurrentBindingInfo(compositePart, out bindingInfos);
            else
                return ActionWrapper.TryGetCurrentBindingInfo(out bindingInfos);
        }
        
        public bool TryGetBindingInfo(ControlScheme controlScheme, out IEnumerable<BindingInfo> bindingInfos)
        {
            if (ActionWrapper == null)
            {
                bindingInfos = null;
                return false;
            }

            if (useCompositePart)
                return ActionWrapper.TryGetBindingInfo(controlScheme, compositePart, out bindingInfos);
            else
                return ActionWrapper.TryGetBindingInfo(controlScheme, out bindingInfos);
        }

        public void StartInteractiveRebind(ControlScheme controlScheme, Action<RebindStatus> callback = null)
        {
            if (ActionWrapper == null)
            {
                return;
            }
            
            if (useCompositePart)
                ActionWrapper.StartInteractiveRebind(controlScheme, compositePart, callback);
            else
                ActionWrapper.StartInteractiveRebind(controlScheme, callback);
        }
        
        private ActionReference(InputAction action)
        {
            reference = InputActionReference.Create(action);
        }
    }
}
