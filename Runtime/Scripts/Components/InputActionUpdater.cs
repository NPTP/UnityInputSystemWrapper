using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Components
{
    /// <summary>
    /// Place this component, choose an input action, and subscribe to its OnBindingsUpdated event.
    /// In the handler for that event, you will receive binding info (names & sprites) whenever the bindings
    /// should change (device changes, bindings changed by player, etc.). You can then use the binding info
    /// for your UI displays.
    /// </summary>
    public class InputActionUpdater : MonoBehaviour
    {
        public event Action<IEnumerable<BindingInfo>> OnBindingsUpdated;
        
        [SerializeField] private InputActionReferenceWrapper inputActionReference;
        
        private void Start()
        {
            UpdateEvents();
        }

        private void OnEnable()
        {
            Input.OnInputUserChange += HandleInputUserChange;
            Input.OnBindingsChanged += HandleBindingsChanged;
            UpdateEvents();
        }

        private void OnDisable()
        {
            Input.OnInputUserChange -= HandleInputUserChange;
            Input.OnBindingsChanged -= HandleBindingsChanged;
        }

        private void HandleInputUserChange(InputUserChangeInfo inputUserChangeInfo)
        {
            UpdateEvents();
        }

        private void HandleBindingsChanged()
        {
            UpdateEvents();
        }

        private void UpdateEvents()
        {
            if (!Input.TryGetCurrentBindingInfo(inputActionReference, out IEnumerable<BindingInfo> bindingInfo))
            {
                return;
            }
            
            OnBindingsUpdated?.Invoke(bindingInfo);
        }
    }
}