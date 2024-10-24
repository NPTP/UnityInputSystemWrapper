using System;
using UnityEngine.InputSystem;

// ------------------------------------------------------------------------------------------
// This file was automatically generated by InputScriptGenerator. Do not modify it manually.
// ------------------------------------------------------------------------------------------
namespace NPTP.InputSystemWrapper.Generated.Actions
{
    public class PlayerActions
    {
        private event Action<InputAction.CallbackContext> @_OnMove;
        public event Action<InputAction.CallbackContext> @OnMove
        {
            add { _OnMove -= value; _OnMove += value; }
            remove => _OnMove -= value;
        }

        private event Action<InputAction.CallbackContext> @_OnLook;
        public event Action<InputAction.CallbackContext> @OnLook
        {
            add { _OnLook -= value; _OnLook += value; }
            remove => _OnLook -= value;
        }

        private event Action<InputAction.CallbackContext> @_OnFire;
        public event Action<InputAction.CallbackContext> @OnFire
        {
            add { _OnFire -= value; _OnFire += value; }
            remove => _OnFire -= value;
        }

        internal InputActionMap ActionMap { get; }
        
        public ActionWrapper Move { get; }
        public ActionWrapper Look { get; }
        public ActionWrapper Fire { get; }
        
        private bool enabled;
        
        internal PlayerActions(InputActionAsset asset)
        {
            ActionMap = asset.FindActionMap("Player", throwIfNotFound: true);
            
            Move = new ActionWrapper(ActionMap.FindAction("Move", throwIfNotFound: true));
            Look = new ActionWrapper(ActionMap.FindAction("Look", throwIfNotFound: true));
            Fire = new ActionWrapper(ActionMap.FindAction("Fire", throwIfNotFound: true));
        }
        
        internal void EnableAndRegisterCallbacks()
        {
            if (enabled)
            {
                return;
            }

            enabled = true;
            ActionMap.Enable();
            
            Move.InputAction.started += HandleMove;
            Move.InputAction.performed += HandleMove;
            Move.InputAction.canceled += HandleMove;
            Look.InputAction.started += HandleLook;
            Look.InputAction.performed += HandleLook;
            Look.InputAction.canceled += HandleLook;
            Fire.InputAction.started += HandleFire;
            Fire.InputAction.performed += HandleFire;
            Fire.InputAction.canceled += HandleFire;
        }
        
        internal void DisableAndUnregisterCallbacks()
        {
            if (!enabled)
            {
                return;
            }

            enabled = false;
            ActionMap.Disable();

            Move.InputAction.started -= HandleMove;
            Move.InputAction.performed -= HandleMove;
            Move.InputAction.canceled -= HandleMove;
            Look.InputAction.started -= HandleLook;
            Look.InputAction.performed -= HandleLook;
            Look.InputAction.canceled -= HandleLook;
            Fire.InputAction.started -= HandleFire;
            Fire.InputAction.performed -= HandleFire;
            Fire.InputAction.canceled -= HandleFire;
        }

        private void HandleMove(InputAction.CallbackContext context) => _OnMove?.Invoke(context);
        private void HandleLook(InputAction.CallbackContext context) => _OnLook?.Invoke(context);
        private void HandleFire(InputAction.CallbackContext context) => _OnFire?.Invoke(context);
    }
}
