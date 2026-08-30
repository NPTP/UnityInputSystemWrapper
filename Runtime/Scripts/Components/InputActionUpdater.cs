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
    /// </summary>
    public class InputActionUpdater : MonoBehaviour
    {
        public event Action<BindingSlots> OnBindingsUpdated;

        [SerializeField] private ActionReference actionReference;
        public ActionReference ActionReference => actionReference;

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

            OnBindingsUpdated?.Invoke(actionReference.GetCurrentBindingSlots());
        }
    }
}