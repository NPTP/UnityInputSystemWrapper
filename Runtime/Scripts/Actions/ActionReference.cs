using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

using NPTP.InputSystemWrapper;

namespace NPTP.InputSystemWrapper.Actions
{
    /// <summary>
    /// For referencing InputActions in the inspector, and being able to use these references in a way
    /// that actually refers to the same InputAction instances in the runtime player input assets instead
    /// of some arbitrary instance.
    /// </summary>
    [Serializable]
    public partial class ActionReference
    {
        public event Action<ActionEventInfo> OnEvent
        {
            add => ActionWrapper.OnEvent += value;
            remove => ActionWrapper.OnEvent -= value;
        }
        
        public bool DownThisFrame => ActionWrapper.DownThisFrame;
        public bool IsDown => ActionWrapper.IsDown;
        
        [SerializeField] private InputActionReference reference;
        
        [SerializeField] private bool useCompositePart; 
        public bool UseCompositePart => useCompositePart;
        
        [SerializeField] private CompositePart compositePart;
        public CompositePart CompositePart => compositePart;

        [SerializeField] private bool applyToAllPlayers;
        internal bool ApplyToAllPlayers => applyToAllPlayers;
        
        [SerializeField] private int playerID;
        internal int PlayerID => playerID;
        
        private ActionWrapper actionWrapper;
        internal ActionWrapper ActionWrapper
        {
            get
            {
                if (actionWrapper != null)
                {
                    return actionWrapper;
                }

                if (reference == null || reference.action == null)
                {
                    return null;
                }
                
                InputRuntime.Current.TryGetActionWrapper(PlayerID, reference.action, out actionWrapper);
                return actionWrapper;
            }
        }

        public string ActionName => ActionWrapper != null ? ActionWrapper.InputAction.name : "Not found";
        
        public static bool TryConvert(InputActionReference inputActionReference, out ActionReference actionReference)
        {
            if (inputActionReference != null && inputActionReference.action != null &&
                InputRuntime.Current.TryConvert(inputActionReference, out ActionWrapper actionWrapper))
            {
                actionReference = new ActionReference(inputActionReference.action) { actionWrapper = actionWrapper };
                return true;
            }

            actionReference = null;
            return false;
        }
        
        public static bool TryConvert(InputAction inputAction, int playerID, out ActionReference actionReference)
        {
            if (inputAction != null && InputRuntime.Current.TryGetActionWrapper(playerID, inputAction, out ActionWrapper actionWrapper))
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

            return useCompositePart
                ? ActionWrapper.TryGetCurrentBindingInfo(compositePart, out bindingInfos)
                : ActionWrapper.TryGetCurrentBindingInfo(out bindingInfos);
        }
        
        public bool TryGetBindingInfo(ControlScheme controlScheme, out IEnumerable<BindingInfo> bindingInfos)
        {
            if (ActionWrapper == null)
            {
                bindingInfos = null;
                return false;
            }

            return useCompositePart
                ? ActionWrapper.TryGetBindingInfo(controlScheme, compositePart, out bindingInfos)
                : ActionWrapper.TryGetBindingInfo(controlScheme, out bindingInfos);
        }

        public void StartInteractiveRebind(ControlScheme controlScheme, Action<RebindInfo> callback = null)
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
