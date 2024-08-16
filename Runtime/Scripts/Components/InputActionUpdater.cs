using NPTP.InputSystemWrapper.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Components
{
    // TODO: Support picking an individual player to find a binding
    public class InputActionUpdater : MonoBehaviour
    {
        public UnityEvent<Sprite> setSpriteEvent;
        public UnityEvent<string> setTextEvent;

        [SerializeField] private InputActionReferenceWrapper inputActionReference;

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
            if (Input.TryGetActionBindingInfo(inputActionReference.InputAction, device, out BindingInfo bindingInfo))
            {
                setSpriteEvent?.Invoke(bindingInfo.Sprite);
                setTextEvent?.Invoke(bindingInfo.DisplayName);
            }
        }
    }
}