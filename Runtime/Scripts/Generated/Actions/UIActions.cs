using NPTP.InputSystemWrapper.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using Button = UnityEngine.InputSystem.HID.HID.Button;

// ------------------------------------------------------------------------------------------
// This file was automatically generated by InputScriptGenerator. Do not modify it manually.
// ------------------------------------------------------------------------------------------
namespace NPTP.InputSystemWrapper.Generated.Actions
{
    public class UIActions
    {
        internal InputActionMap ActionMap { get; }
        
        public ValueActionWrapper<Vector2> Navigate { get; }
        public ActionWrapper Submit { get; }
        public ActionWrapper Cancel { get; }
        public ValueActionWrapper<Vector2> Point { get; }
        public ValueActionWrapper<float> Click { get; }
        public ValueActionWrapper<Vector2> ScrollWheel { get; }
        public ValueActionWrapper<float> MiddleClick { get; }
        public ValueActionWrapper<float> RightClick { get; }
        public ValueActionWrapper<Vector3> TrackedDevicePosition { get; }
        public ValueActionWrapper<Quaternion> TrackedDeviceOrientation { get; }
        
        private bool enabled;
        
        internal UIActions(InputActionAsset asset)
        {
            ActionMap = asset.FindActionMap("UI", throwIfNotFound: true);
            
            Navigate = new (ActionMap.FindAction("Navigate", throwIfNotFound: true));
            Submit = new (ActionMap.FindAction("Submit", throwIfNotFound: true));
            Cancel = new (ActionMap.FindAction("Cancel", throwIfNotFound: true));
            Point = new (ActionMap.FindAction("Point", throwIfNotFound: true));
            Click = new (ActionMap.FindAction("Click", throwIfNotFound: true));
            ScrollWheel = new (ActionMap.FindAction("ScrollWheel", throwIfNotFound: true));
            MiddleClick = new (ActionMap.FindAction("MiddleClick", throwIfNotFound: true));
            RightClick = new (ActionMap.FindAction("RightClick", throwIfNotFound: true));
            TrackedDevicePosition = new (ActionMap.FindAction("TrackedDevicePosition", throwIfNotFound: true));
            TrackedDeviceOrientation = new (ActionMap.FindAction("TrackedDeviceOrientation", throwIfNotFound: true));
        }
        
        internal void EnableAndRegisterCallbacks()
        {
            if (enabled)
            {
                return;
            }

            enabled = true;
            ActionMap.Enable();
            
            Navigate.RegisterCallbacks();
            Submit.RegisterCallbacks();
            Cancel.RegisterCallbacks();
            Point.RegisterCallbacks();
            Click.RegisterCallbacks();
            ScrollWheel.RegisterCallbacks();
            MiddleClick.RegisterCallbacks();
            RightClick.RegisterCallbacks();
            TrackedDevicePosition.RegisterCallbacks();
            TrackedDeviceOrientation.RegisterCallbacks();
        }
        
        internal void DisableAndUnregisterCallbacks()
        {
            if (!enabled)
            {
                return;
            }

            enabled = false;
            ActionMap.Disable();

            Navigate.UnregisterCallbacks();
            Submit.UnregisterCallbacks();
            Cancel.UnregisterCallbacks();
            Point.UnregisterCallbacks();
            Click.UnregisterCallbacks();
            ScrollWheel.UnregisterCallbacks();
            MiddleClick.UnregisterCallbacks();
            RightClick.UnregisterCallbacks();
            TrackedDevicePosition.UnregisterCallbacks();
            TrackedDeviceOrientation.UnregisterCallbacks();
        }
    }
}
