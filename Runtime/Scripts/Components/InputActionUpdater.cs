using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace NPTP.InputSystemWrapper.Components
{
    public class InputActionUpdater : MonoBehaviour
    {
        [SerializeField] private InputActionReferenceWrapper inputActionReference;
        
        [SerializeField] private UnityEvent<Sprite> setSpriteEvent;
        [SerializeField] private UnityEvent<string> setTextEvent;

        private void OnEnable()
        {
            Subscribe();
            UpdateEvents(Input.LastUsedDevice);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            Input.OnDeviceControlChanged += HandleDeviceControlChanged;
        }

        private void Unsubscribe()
        {
            Input.OnDeviceControlChanged -= HandleDeviceControlChanged;
        }

        private void HandleDeviceControlChanged(DeviceControlInfo deviceControlInfo)
        {
            UpdateEvents(Input.LastUsedDevice);
        }

        private void UpdateEvents(InputDevice device)
        {
            if (!Input.TryGetActionBindingInfo(inputActionReference.InputAction, device, out IEnumerable<BindingInfo> bindingInfo))
            {
                return;
            }
            
            int i = 0;
            foreach (BindingInfo info in bindingInfo)
            {
                if (info.Sprite != null)
                    Debug.Log($"Got sprite binding {i} for {inputActionReference.InputAction.name}: {info.Sprite.name}");
                setSpriteEvent?.Invoke(info.Sprite);

                if (!string.IsNullOrEmpty(info.DisplayName))
                    Debug.Log($"Got display name binding {i} for {inputActionReference.InputAction.name}: {info.DisplayName}");
                setTextEvent?.Invoke(info.DisplayName);
                    
                i++;
            }
        }
    }
}