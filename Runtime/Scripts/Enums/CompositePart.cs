using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Enums
{
    public enum CompositePart
    {
        [InspectorName("Don't Isolate Part")]
        DontIsolatePart = 0,
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
        internal static bool Matches(this CompositePart compositePart, InputBinding binding)
        {
            return binding.isPartOfComposite && binding.name == compositePart.ToBindingName();
        }

        private static string ToBindingName(this CompositePart compositePart)
        {
            return compositePart.ToString();
        }
    }
}