using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper
{
    public sealed class ActionWrapper
    {
        internal InputAction InputAction { get; }
        public bool WasPerformedThisFrame => InputAction.WasPerformedThisFrame();
        public bool IsPerformed => InputAction.phase == InputActionPhase.Performed;
        
        internal ActionWrapper(InputAction inputAction)
        {
            InputAction = inputAction;
        }
    }
}