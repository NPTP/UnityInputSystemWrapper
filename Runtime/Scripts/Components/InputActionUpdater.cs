using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Data;
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
        public static event Action<IEnumerable<BindingInfo>> OnBindingsUpdated;
        
        [SerializeField] private InputActionReferenceWrapper inputActionReference;
        
        private void Start()
        {
            UpdateEvents(Input.LastUsedDevice);
        }

        private void OnEnable()
        {
            Input.OnDeviceControlChanged += HandleDeviceControlChanged;
            BindingChanger.OnBindingOperationEnded += HandleBindingOperationEnded;
            UpdateEvents(Input.LastUsedDevice);
        }

        private void OnDisable()
        {
            Input.OnDeviceControlChanged -= HandleDeviceControlChanged;
            BindingChanger.OnBindingOperationEnded -= HandleBindingOperationEnded;
        }

        private void HandleDeviceControlChanged(DeviceControlInfo deviceControlInfo)
        {
            UpdateEvents(Input.LastUsedDevice);
        }

        private void HandleBindingOperationEnded()
        {
            UpdateEvents(Input.LastUsedDevice);
        }

        private void UpdateEvents(InputDevice device)
        {
            if (!Input.TryGetActionBindingInfo(inputActionReference, device, out IEnumerable<BindingInfo> bindingInfo))
            {
                return;
            }
            
            OnBindingsUpdated?.Invoke(bindingInfo);
        }
    }
}