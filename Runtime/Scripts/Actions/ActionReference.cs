using System;
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

        /// <summary>
        /// Which part of a composite this refers to, or DontIsolatePart for the binding as a whole.
        /// </summary>
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

        /// <summary>Every slot of the referenced action on the control scheme the player is currently using.</summary>
        public BindingSlots GetCurrentBindingSlots()
        {
            return ActionWrapper == null ? BindingSlots.Empty : ActionWrapper.GetCurrentBindingSlots();
        }

        /// <summary>
        /// The same slots, loaded in the background: the callback runs once they are ready.
        /// </summary>
        public void GetCurrentBindingSlotsAsync(Action<BindingSlots> onResolved)
        {
            if (ActionWrapper == null)
            {
                onResolved?.Invoke(BindingSlots.Empty);
                return;
            }

            ActionWrapper.GetCurrentBindingSlotsAsync(onResolved);
        }

        internal BindingSlots GetBindingSlots(ControlSchemeId controlSchemeId)
        {
            return ActionWrapper == null ? BindingSlots.Empty : ActionWrapper.GetBindingSlots(controlSchemeId);
        }

        internal void StartInteractiveRebind(ControlSchemeId controlSchemeId, int uiIndex, Action<RebindInfo> callback = null)
        {
            if (ActionWrapper == null)
            {
                return;
            }

            // DontIsolatePart means the whole binding, so there is no second path to take.
            ActionWrapper.StartInteractiveRebind(controlSchemeId, compositePart, uiIndex, callback);
        }

        internal void ResetBinding(ControlSchemeId controlSchemeId, int uiIndex) =>
            InputRuntime.Current.ResetBindingForAction(this, controlSchemeId, uiIndex);

        internal void ResetAllBindings(ControlSchemeId controlSchemeId) =>
            InputRuntime.Current.ResetAllBindingsForAction(this, controlSchemeId);

        private ActionReference(InputAction action)
        {
            reference = InputActionReference.Create(action);
        }

        protected ActionReference()
        {
        }
    }
}
