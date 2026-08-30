using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Actions
{
    public readonly struct ActionEventInfo
    {
        public InputActionPhase Phase { get; }
        public int PlayerID => actionWrapper.PlayerID;
        
        private readonly ActionWrapper actionWrapper;

        public ActionEventInfo(ActionWrapper actionWrapper, InputAction.CallbackContext callbackContext)
        {
            Phase = callbackContext.phase;
            this.actionWrapper = actionWrapper;
        }
        
        public T ReadValue<T>() where T : struct
        {
            return actionWrapper.InputAction.ReadValue<T>();
        }
    }
    
    // TODO: ActionWrapper needs a base class, then inheriting classes with different events, one for ActionEventInfo and another for this ActionEventInfo<T>
    public readonly struct ActionEventInfo<T> where T : struct
    {
        public InputActionPhase Phase { get; }
        public int PlayerID => valueActionWrapper.PlayerID;
        public T Value => valueActionWrapper.ReadValue();
        
        private readonly ValueActionWrapper<T> valueActionWrapper;

        public ActionEventInfo(ValueActionWrapper<T> valueActionWrapper, InputAction.CallbackContext callbackContext)
        {
            Phase = callbackContext.phase;
            this.valueActionWrapper = valueActionWrapper;
        }
    }
}
