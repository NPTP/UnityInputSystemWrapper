using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Actions
{
    internal readonly struct ActionInfo
    {
        internal InputAction InputAction { get; }
        internal bool UseCompositePart { get; }
        internal CompositePart CompositePart { get; }
        
        internal ActionInfo(InputAction inputAction, bool useCompositePart, CompositePart compositePart)
        {
            InputAction = inputAction;
            UseCompositePart = useCompositePart;
            CompositePart = compositePart;
        }
        
        internal ActionInfo(InputAction inputAction, bool useCompositePart) : this()
        {
            InputAction = inputAction;
            UseCompositePart = useCompositePart;
        }
    }
}
