using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace NPTP.InputSystemWrapper.Player
{
    /// <summary>
    /// The pointer actions a player's event system uses while their virtual mouse is driving it, built in
    /// code and restricted to that one device.
    /// <para>
    /// The player's own actions cannot be used for this. They belong to a user whose paired devices are
    /// managed by the control scheme, which drops any device the scheme does not name - a virtual mouse
    /// included - so bindings to it resolve to nothing. A map of its own, holding the device directly,
    /// answers to that device and to nothing else.
    /// </para>
    /// </summary>
    internal sealed class VirtualMousePointerActions : IDisposable
    {
        private const string MAP_NAME = "ISWVirtualMousePointer";

        /// <summary>
        /// An asset of their own, because a reference can only be made to an action that lives in one, and
        /// because a device list set here covers every map in it.
        /// </summary>
        private readonly InputActionAsset asset;

        private readonly InputActionMap actionMap;

        internal InputActionReference Point { get; }
        internal InputActionReference LeftClick { get; }
        internal InputActionReference RightClick { get; }
        internal InputActionReference MiddleClick { get; }
        internal InputActionReference ScrollWheel { get; }

        private bool disposed;

        internal VirtualMousePointerActions(Mouse device)
        {
            asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = MAP_NAME;
            asset.hideFlags = HideFlags.HideAndDontSave;
            actionMap = asset.AddActionMap(MAP_NAME);

            Point = Add("Point", InputActionType.Value, "<Mouse>/position", "Vector2");
            LeftClick = Add("LeftClick", InputActionType.Button, "<Mouse>/leftButton", "Button");
            RightClick = Add("RightClick", InputActionType.Button, "<Mouse>/rightButton", "Button");
            MiddleClick = Add("MiddleClick", InputActionType.Button, "<Mouse>/middleButton", "Button");
            ScrollWheel = Add("ScrollWheel", InputActionType.Value, "<Mouse>/scroll", "Vector2");

            // The one device these read, so the bindings above resolve to the virtual mouse rather than to
            // every mouse present.
            asset.devices = new[] { (InputDevice)device };
            asset.Enable();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            asset.Disable();

            // Each reference is an object of its own, made to hand an action to the event system.
            Object.Destroy(Point);
            Object.Destroy(LeftClick);
            Object.Destroy(RightClick);
            Object.Destroy(MiddleClick);
            Object.Destroy(ScrollWheel);
            Object.Destroy(asset);
        }

        private InputActionReference Add(string name, InputActionType actionType, string binding, string expectedControlType)
        {
            InputAction action = actionMap.AddAction(name, actionType, binding, expectedControlLayout: expectedControlType);
            return InputActionReference.Create(action);
        }
    }
}
