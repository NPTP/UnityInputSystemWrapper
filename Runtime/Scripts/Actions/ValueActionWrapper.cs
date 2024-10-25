using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Actions
{
    public abstract class ValueActionWrapper : ActionWrapper
    {
        protected ValueActionWrapper(InputAction inputAction) : base(inputAction)
        {
        }
    }
    
    public sealed class ValueActionWrapper<T> : ValueActionWrapper where T : struct
    {
        public T ReadValue() => InputAction.ReadValue<T>();

        public ValueActionWrapper(InputAction inputAction) : base(inputAction)
        {
        }
    }

    public sealed class AnyValueActionWrapper : ValueActionWrapper
    {
        public object ReadValue() => InputAction.ReadValueAsObject();
        
        public AnyValueActionWrapper(InputAction inputAction) : base(inputAction)
        {
        }
    }
}