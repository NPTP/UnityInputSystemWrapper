using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Actions
{
    public sealed class ButtonActionWrapper : ActionWrapper
    {
        public bool DownThisFrame => InputAction.WasPerformedThisFrame();
        public bool IsDown => InputAction.phase == InputActionPhase.Performed;

        public ButtonActionWrapper(InputAction inputAction) : base(inputAction) { }
    }
}