using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Player;
using UnityEngine;

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
        
        [SerializeField] private ActionReference actionReference;
        public ActionReference ActionReference => actionReference;

        private void Start()
        {
            UpdateEvents();
        }

        private void OnEnable()
        {
            ISW.OnAnyPlayerInputUserChange += HandleAnyPlayerInputUserChange;
            ISW.OnBindingsChanged += HandleBindingsChanged;
            UpdateEvents();
        }

        private void OnDisable()
        {
            ISW.OnAnyPlayerInputUserChange -= HandleAnyPlayerInputUserChange;
            ISW.OnBindingsChanged -= HandleBindingsChanged;
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
            if (actionReference == null || !actionReference.TryGetCurrentBindingInfo(out IEnumerable<BindingInfo> bindingInfo))
            {
                return;
            }
            
            OnBindingsUpdated?.Invoke(bindingInfo);
        }
    }
}