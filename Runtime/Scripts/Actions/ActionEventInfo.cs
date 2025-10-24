using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Actions
{
    public readonly struct ActionEventInfo
    {
        public InputActionPhase Phase { get; }
        public int PlayerID => action.PlayerID;
        
        private readonly ActionWrapper action;

        public ActionEventInfo(ActionWrapper actionWrapper, InputAction.CallbackContext callbackContext)
        {
            Phase = callbackContext.phase;
            action = actionWrapper;
        }
        
        public T ReadValue<T>() where T : struct
        {
            return action.InputAction.ReadValue<T>();
        }
    }
    
    // TODO: ActionWrapper needs a base class, then inheriting classes with different events, one for ActionEventInfo and another for this ActionEventInfo<T>
    public readonly struct ActionEventInfo<T> where T : struct
    {
        public InputActionPhase Phase { get; }
        public int PlayerID => action.PlayerID;
        public T Value => action.ReadValue();
        
        private readonly ValueActionWrapper<T> action;

        public ActionEventInfo(ValueActionWrapper<T> actionWrapper, InputAction.CallbackContext callbackContext)
        {
            Phase = callbackContext.phase;
            action = actionWrapper;
        }
    }
}
