using System;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Player;
using UnityEngine;

using NPTP.InputSystemWrapper;

namespace NPTP.InputSystemWrapper.Components
{
    /// <summary>
    /// Place this component, choose an input action, and subscribe to its OnBindingsUpdated event.
    /// In the handler for that event, you will receive the action's binding slots (names & sprites, by UI
    /// index) whenever the bindings should change (device changes, bindings changed by player, etc.). You
    /// can then use those for your UI displays.
    /// <para>
    /// The slots belong to this component and are replaced on every update, so read what you need in the
    /// handler rather than holding onto them.
    /// </para>
    /// </summary>
    public class InputActionUpdater : MonoBehaviour
    {
        public event Action<BindingSlots> OnBindingsUpdated;

        [SerializeField] private ActionReference actionReference;
        public ActionReference ActionReference => actionReference;

        private BindingSlots bindingSlots;

        private void Start()
        {
            UpdateEvents();
        }

        private void OnEnable()
        {
            InputRuntime.Current.OnAnyPlayerInputUserChange += HandleAnyPlayerInputUserChange;
            InputRuntime.Current.OnBindingsChanged += HandleBindingsChanged;
            UpdateEvents();
        }

        private void OnDisable()
        {
            InputRuntime.Current.OnAnyPlayerInputUserChange -= HandleAnyPlayerInputUserChange;
            InputRuntime.Current.OnBindingsChanged -= HandleBindingsChanged;
        }

        private void OnDestroy()
        {
            bindingSlots?.Dispose();
            bindingSlots = null;
        }

        private void HandleAnyPlayerInputUserChange(InputUserChangeInfo inputUserChangeInfo)
        {
            UpdateEvents();
        }

        private void HandleBindingsChanged()
        {
            UpdateEvents();
        }

        private void UpdateEvents()
        {
            if (actionReference == null)
            {
                return;
            }

            // The previous set is given back only once its replacement is in hand, so data shared by both
            // stays loaded across the swap.
            BindingSlots previous = bindingSlots;
            bindingSlots = actionReference.GetCurrentBindingSlots();
            previous?.Dispose();

            OnBindingsUpdated?.Invoke(bindingSlots);
        }
    }
}