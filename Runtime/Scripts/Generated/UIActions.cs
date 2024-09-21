using System;
using UnityEngine.InputSystem;

// ------------------------------------------------------------------------------------------
// This file was automatically generated by InputScriptGenerator. Do not modify it manually.
// ------------------------------------------------------------------------------------------
namespace NPTP.InputSystemWrapper.Generated.Actions
{
    public class UIActions
    {
        internal InputActionMap ActionMap { get; }
        
        private event Action<InputAction.CallbackContext> @_OnNavigate;
        public event Action<InputAction.CallbackContext> @OnNavigate
        {
            add { _OnNavigate -= value; _OnNavigate += value; }
            remove => _OnNavigate -= value;
        }

        private event Action<InputAction.CallbackContext> @_OnSubmit;
        public event Action<InputAction.CallbackContext> @OnSubmit
        {
            add { _OnSubmit -= value; _OnSubmit += value; }
            remove => _OnSubmit -= value;
        }

        private event Action<InputAction.CallbackContext> @_OnCancel;
        public event Action<InputAction.CallbackContext> @OnCancel
        {
            add { _OnCancel -= value; _OnCancel += value; }
            remove => _OnCancel -= value;
        }

        private event Action<InputAction.CallbackContext> @_OnPoint;
        public event Action<InputAction.CallbackContext> @OnPoint
        {
            add { _OnPoint -= value; _OnPoint += value; }
            remove => _OnPoint -= value;
        }

        private event Action<InputAction.CallbackContext> @_OnClick;
        public event Action<InputAction.CallbackContext> @OnClick
        {
            add { _OnClick -= value; _OnClick += value; }
            remove => _OnClick -= value;
        }

        private event Action<InputAction.CallbackContext> @_OnScrollWheel;
        public event Action<InputAction.CallbackContext> @OnScrollWheel
        {
            add { _OnScrollWheel -= value; _OnScrollWheel += value; }
            remove => _OnScrollWheel -= value;
        }

        private event Action<InputAction.CallbackContext> @_OnMiddleClick;
        public event Action<InputAction.CallbackContext> @OnMiddleClick
        {
            add { _OnMiddleClick -= value; _OnMiddleClick += value; }
            remove => _OnMiddleClick -= value;
        }

        private event Action<InputAction.CallbackContext> @_OnRightClick;
        public event Action<InputAction.CallbackContext> @OnRightClick
        {
            add { _OnRightClick -= value; _OnRightClick += value; }
            remove => _OnRightClick -= value;
        }

        private event Action<InputAction.CallbackContext> @_OnTrackedDevicePosition;
        public event Action<InputAction.CallbackContext> @OnTrackedDevicePosition
        {
            add { _OnTrackedDevicePosition -= value; _OnTrackedDevicePosition += value; }
            remove => _OnTrackedDevicePosition -= value;
        }

        private event Action<InputAction.CallbackContext> @_OnTrackedDeviceOrientation;
        public event Action<InputAction.CallbackContext> @OnTrackedDeviceOrientation
        {
            add { _OnTrackedDeviceOrientation -= value; _OnTrackedDeviceOrientation += value; }
            remove => _OnTrackedDeviceOrientation -= value;
        }

        public ActionWrapper Navigate { get; }
        public ActionWrapper Submit { get; }
        public ActionWrapper Cancel { get; }
        public ActionWrapper Point { get; }
        public ActionWrapper Click { get; }
        public ActionWrapper ScrollWheel { get; }
        public ActionWrapper MiddleClick { get; }
        public ActionWrapper RightClick { get; }
        public ActionWrapper TrackedDevicePosition { get; }
        public ActionWrapper TrackedDeviceOrientation { get; }
        
        private bool enabled;
        
        internal UIActions(InputActionAsset asset)
        {
            ActionMap = asset.FindActionMap("UI", throwIfNotFound: true);
            
            Navigate = new ActionWrapper(ActionMap.FindAction("Navigate", throwIfNotFound: true));
            Submit = new ActionWrapper(ActionMap.FindAction("Submit", throwIfNotFound: true));
            Cancel = new ActionWrapper(ActionMap.FindAction("Cancel", throwIfNotFound: true));
            Point = new ActionWrapper(ActionMap.FindAction("Point", throwIfNotFound: true));
            Click = new ActionWrapper(ActionMap.FindAction("Click", throwIfNotFound: true));
            ScrollWheel = new ActionWrapper(ActionMap.FindAction("ScrollWheel", throwIfNotFound: true));
            MiddleClick = new ActionWrapper(ActionMap.FindAction("MiddleClick", throwIfNotFound: true));
            RightClick = new ActionWrapper(ActionMap.FindAction("RightClick", throwIfNotFound: true));
            TrackedDevicePosition = new ActionWrapper(ActionMap.FindAction("TrackedDevicePosition", throwIfNotFound: true));
            TrackedDeviceOrientation = new ActionWrapper(ActionMap.FindAction("TrackedDeviceOrientation", throwIfNotFound: true));
        }
        
        internal void EnableAndRegisterCallbacks()
        {
            if (enabled)
            {
                return;
            }

            enabled = true;
            ActionMap.Enable();
            
            Navigate.InputAction.started += HandleNavigate;
            Navigate.InputAction.performed += HandleNavigate;
            Navigate.InputAction.canceled += HandleNavigate;
            Submit.InputAction.started += HandleSubmit;
            Submit.InputAction.performed += HandleSubmit;
            Submit.InputAction.canceled += HandleSubmit;
            Cancel.InputAction.started += HandleCancel;
            Cancel.InputAction.performed += HandleCancel;
            Cancel.InputAction.canceled += HandleCancel;
            Point.InputAction.started += HandlePoint;
            Point.InputAction.performed += HandlePoint;
            Point.InputAction.canceled += HandlePoint;
            Click.InputAction.started += HandleClick;
            Click.InputAction.performed += HandleClick;
            Click.InputAction.canceled += HandleClick;
            ScrollWheel.InputAction.started += HandleScrollWheel;
            ScrollWheel.InputAction.performed += HandleScrollWheel;
            ScrollWheel.InputAction.canceled += HandleScrollWheel;
            MiddleClick.InputAction.started += HandleMiddleClick;
            MiddleClick.InputAction.performed += HandleMiddleClick;
            MiddleClick.InputAction.canceled += HandleMiddleClick;
            RightClick.InputAction.started += HandleRightClick;
            RightClick.InputAction.performed += HandleRightClick;
            RightClick.InputAction.canceled += HandleRightClick;
            TrackedDevicePosition.InputAction.started += HandleTrackedDevicePosition;
            TrackedDevicePosition.InputAction.performed += HandleTrackedDevicePosition;
            TrackedDevicePosition.InputAction.canceled += HandleTrackedDevicePosition;
            TrackedDeviceOrientation.InputAction.started += HandleTrackedDeviceOrientation;
            TrackedDeviceOrientation.InputAction.performed += HandleTrackedDeviceOrientation;
            TrackedDeviceOrientation.InputAction.canceled += HandleTrackedDeviceOrientation;
        }
        
        internal void DisableAndUnregisterCallbacks()
        {
            if (!enabled)
            {
                return;
            }

            enabled = false;
            ActionMap.Disable();

            Navigate.InputAction.started -= HandleNavigate;
            Navigate.InputAction.performed -= HandleNavigate;
            Navigate.InputAction.canceled -= HandleNavigate;
            Submit.InputAction.started -= HandleSubmit;
            Submit.InputAction.performed -= HandleSubmit;
            Submit.InputAction.canceled -= HandleSubmit;
            Cancel.InputAction.started -= HandleCancel;
            Cancel.InputAction.performed -= HandleCancel;
            Cancel.InputAction.canceled -= HandleCancel;
            Point.InputAction.started -= HandlePoint;
            Point.InputAction.performed -= HandlePoint;
            Point.InputAction.canceled -= HandlePoint;
            Click.InputAction.started -= HandleClick;
            Click.InputAction.performed -= HandleClick;
            Click.InputAction.canceled -= HandleClick;
            ScrollWheel.InputAction.started -= HandleScrollWheel;
            ScrollWheel.InputAction.performed -= HandleScrollWheel;
            ScrollWheel.InputAction.canceled -= HandleScrollWheel;
            MiddleClick.InputAction.started -= HandleMiddleClick;
            MiddleClick.InputAction.performed -= HandleMiddleClick;
            MiddleClick.InputAction.canceled -= HandleMiddleClick;
            RightClick.InputAction.started -= HandleRightClick;
            RightClick.InputAction.performed -= HandleRightClick;
            RightClick.InputAction.canceled -= HandleRightClick;
            TrackedDevicePosition.InputAction.started -= HandleTrackedDevicePosition;
            TrackedDevicePosition.InputAction.performed -= HandleTrackedDevicePosition;
            TrackedDevicePosition.InputAction.canceled -= HandleTrackedDevicePosition;
            TrackedDeviceOrientation.InputAction.started -= HandleTrackedDeviceOrientation;
            TrackedDeviceOrientation.InputAction.performed -= HandleTrackedDeviceOrientation;
            TrackedDeviceOrientation.InputAction.canceled -= HandleTrackedDeviceOrientation;
        }

        private void HandleNavigate(InputAction.CallbackContext context) => _OnNavigate?.Invoke(context);
        private void HandleSubmit(InputAction.CallbackContext context) => _OnSubmit?.Invoke(context);
        private void HandleCancel(InputAction.CallbackContext context) => _OnCancel?.Invoke(context);
        private void HandlePoint(InputAction.CallbackContext context) => _OnPoint?.Invoke(context);
        private void HandleClick(InputAction.CallbackContext context) => _OnClick?.Invoke(context);
        private void HandleScrollWheel(InputAction.CallbackContext context) => _OnScrollWheel?.Invoke(context);
        private void HandleMiddleClick(InputAction.CallbackContext context) => _OnMiddleClick?.Invoke(context);
        private void HandleRightClick(InputAction.CallbackContext context) => _OnRightClick?.Invoke(context);
        private void HandleTrackedDevicePosition(InputAction.CallbackContext context) => _OnTrackedDevicePosition?.Invoke(context);
        private void HandleTrackedDeviceOrientation(InputAction.CallbackContext context) => _OnTrackedDeviceOrientation?.Invoke(context);
    }
}