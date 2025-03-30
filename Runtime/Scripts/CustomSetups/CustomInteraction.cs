using System;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.CustomSetups
{
    public abstract class CustomInteraction : CustomSetup
    {
        internal override void Register() => InputSystem.RegisterInteraction(SetupType, Name);
    }
    
    public abstract class CustomInteraction<T> : CustomInteraction where T : IInputInteraction
    {
        protected sealed override Type SetupType => typeof(T);
    }
}