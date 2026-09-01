using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Enums
{
    /// <summary>
    /// One part of a composite binding, named as the input system names it. Which parts a composite has
    /// depends on the composite: an axis has positive and negative, a vector has directions, and the
    /// modifier composites have their modifiers and the binding they modify.
    /// </summary>
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
        Backward,
        Modifier,
        Modifier1,
        Modifier2,
        Button,
        Binding
    }

    internal static class CompositePartExtensions
    {
        internal static bool Matches(this CompositePart compositePart, InputBinding binding)
        {
            // The input system names a part after the composite's field, which is camel case: "up",
            // "modifier1", "button". Comparing case-insensitively is what lets these names line up.
            return binding.isPartOfComposite &&
                   string.Equals(binding.name, compositePart.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
