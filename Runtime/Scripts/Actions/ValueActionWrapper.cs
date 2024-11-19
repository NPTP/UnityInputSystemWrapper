using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Actions
{
    public abstract class ValueActionWrapper : ActionWrapper
    {
        protected ValueActionWrapper(InputAction inputAction, Dictionary<Guid, ActionWrapper> table) : base(inputAction, table)
        {
        }
    }
    
    public sealed class ValueActionWrapper<T> : ValueActionWrapper where T : struct
    {
        public T ReadValue() => InputAction.ReadValue<T>();

        internal ValueActionWrapper(InputAction inputAction, Dictionary<Guid, ActionWrapper> table) : base(inputAction, table)
        {
        }
    }

    public sealed class AnyValueActionWrapper : ValueActionWrapper
    {
        public object ReadValue() => InputAction.ReadValueAsObject();
        
        internal AnyValueActionWrapper(InputAction inputAction, Dictionary<Guid, ActionWrapper> table) : base(inputAction, table)
        {
        }
    }
}