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
    public class XXXActions
    {
        internal InputActionMap ActionMap { get; }
        
        public AnyValueActionWrapper Any { get; }
        public ValueActionWrapper<float> Analog { get; }
        public ValueActionWrapper<float> Axis { get; }
        public ValueActionWrapper<Bone> Bone { get; }
        public ValueActionWrapper<Vector2> Delta { get; }
        public ValueActionWrapper<int> Digital { get; }
        public ValueActionWrapper<double> Double { get; }
        public ValueActionWrapper<Vector2> Dpad { get; }
        public ValueActionWrapper<Eyes> Eyes { get; }
        public ValueActionWrapper<int> Integer { get; }
        public ValueActionWrapper<Pose> Pose { get; }
        public ValueActionWrapper<Quaternion> Quaternion { get; }
        public ValueActionWrapper<Vector2> Stick { get; }
        public ValueActionWrapper<float> Touch { get; }
        public ValueActionWrapper<Vector2> Vector2 { get; }
        public ValueActionWrapper<Vector3> Vector3 { get; }
        public ValueActionWrapper<float> Button { get; }
        public ValueActionWrapper<int> DiscreteButton { get; }
        
        private bool enabled;
        
        internal XXXActions(InputActionAsset asset)
        {
            ActionMap = asset.FindActionMap("XXX", throwIfNotFound: true);
            
            Any = new (ActionMap.FindAction("Any", throwIfNotFound: true));
            Analog = new (ActionMap.FindAction("Analog", throwIfNotFound: true));
            Axis = new (ActionMap.FindAction("Axis", throwIfNotFound: true));
            Bone = new (ActionMap.FindAction("Bone", throwIfNotFound: true));
            Delta = new (ActionMap.FindAction("Delta", throwIfNotFound: true));
            Digital = new (ActionMap.FindAction("Digital", throwIfNotFound: true));
            Double = new (ActionMap.FindAction("Double", throwIfNotFound: true));
            Dpad = new (ActionMap.FindAction("Dpad", throwIfNotFound: true));
            Eyes = new (ActionMap.FindAction("Eyes", throwIfNotFound: true));
            Integer = new (ActionMap.FindAction("Integer", throwIfNotFound: true));
            Pose = new (ActionMap.FindAction("Pose", throwIfNotFound: true));
            Quaternion = new (ActionMap.FindAction("Quaternion", throwIfNotFound: true));
            Stick = new (ActionMap.FindAction("Stick", throwIfNotFound: true));
            Touch = new (ActionMap.FindAction("Touch", throwIfNotFound: true));
            Vector2 = new (ActionMap.FindAction("Vector2", throwIfNotFound: true));
            Vector3 = new (ActionMap.FindAction("Vector3", throwIfNotFound: true));
            Button = new (ActionMap.FindAction("Button", throwIfNotFound: true));
            DiscreteButton = new (ActionMap.FindAction("DiscreteButton", throwIfNotFound: true));
        }
        
        internal void EnableAndRegisterCallbacks()
        {
            if (enabled)
            {
                return;
            }

            enabled = true;
            ActionMap.Enable();
            
            Any.RegisterCallbacks();
            Analog.RegisterCallbacks();
            Axis.RegisterCallbacks();
            Bone.RegisterCallbacks();
            Delta.RegisterCallbacks();
            Digital.RegisterCallbacks();
            Double.RegisterCallbacks();
            Dpad.RegisterCallbacks();
            Eyes.RegisterCallbacks();
            Integer.RegisterCallbacks();
            Pose.RegisterCallbacks();
            Quaternion.RegisterCallbacks();
            Stick.RegisterCallbacks();
            Touch.RegisterCallbacks();
            Vector2.RegisterCallbacks();
            Vector3.RegisterCallbacks();
            Button.RegisterCallbacks();
            DiscreteButton.RegisterCallbacks();
        }
        
        internal void DisableAndUnregisterCallbacks()
        {
            if (!enabled)
            {
                return;
            }

            enabled = false;
            ActionMap.Disable();

            Any.UnregisterCallbacks();
            Analog.UnregisterCallbacks();
            Axis.UnregisterCallbacks();
            Bone.UnregisterCallbacks();
            Delta.UnregisterCallbacks();
            Digital.UnregisterCallbacks();
            Double.UnregisterCallbacks();
            Dpad.UnregisterCallbacks();
            Eyes.UnregisterCallbacks();
            Integer.UnregisterCallbacks();
            Pose.UnregisterCallbacks();
            Quaternion.UnregisterCallbacks();
            Stick.UnregisterCallbacks();
            Touch.UnregisterCallbacks();
            Vector2.UnregisterCallbacks();
            Vector3.UnregisterCallbacks();
            Button.UnregisterCallbacks();
            DiscreteButton.UnregisterCallbacks();
        }
    }
}
