using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace NPTP.InputSystemWrapper.CustomSetups
{
    public abstract class CustomLayout : CustomSetup
    {
        protected abstract InputDeviceMatcher Matches { get; }
        internal override void Register() => InputSystem.RegisterLayout(SetupType, Name, Matches);
    }
    
    public abstract class CustomLayout<T> : CustomLayout where T : InputControl
    {
        protected sealed override Type SetupType => typeof(T);
    }
}