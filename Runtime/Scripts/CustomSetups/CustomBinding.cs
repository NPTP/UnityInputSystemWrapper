using System;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.CustomSetups
{
    public abstract class CustomBinding : CustomSetup
    {
        internal override void Register() => InputSystem.RegisterBindingComposite(SetupType, Name);
    }
    
    public abstract class CustomBinding<T> : CustomBinding where T : InputBindingComposite
    {
        protected sealed override Type SetupType => typeof(T);
    }
}