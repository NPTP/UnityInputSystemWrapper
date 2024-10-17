using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Enums
{
    public enum CompositePart
    {
        Positive,
        Negative,
        Up,
        Down,
        Left,
        Right,
        Forward,
        Backward
    }

    internal static class CompositePartExtensions
    {
        internal static bool CorrespondsToBinding(this CompositePart compositePart, InputBinding binding)
        {
            return binding.isPartOfComposite && binding.name == compositePart.ToString().ToLower();
        }
    }
}