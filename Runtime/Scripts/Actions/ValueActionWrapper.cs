using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Actions
{
    public abstract class ValueActionWrapper : ActionWrapper
    {
        protected ValueActionWrapper(int playerID, InputAction inputAction, Dictionary<Guid, ActionWrapper> table) : base(playerID, inputAction, table)
        {
        }
    }
    
    public sealed class ValueActionWrapper<T> : ValueActionWrapper where T : struct
    {
        public T ReadValue() => InputAction.ReadValue<T>();

        internal ValueActionWrapper(int playerID, InputAction inputAction, Dictionary<Guid, ActionWrapper> table) : base(playerID, inputAction, table)
        {
        }
    }

    public sealed class AnyValueActionWrapper : ValueActionWrapper
    {
        public object ReadValue() => InputAction.ReadValueAsObject();
        
        internal AnyValueActionWrapper(int playerID, InputAction inputAction, Dictionary<Guid, ActionWrapper> table) : base(playerID, inputAction, table)
        {
        }
    }
}