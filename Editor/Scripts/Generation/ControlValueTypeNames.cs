using System;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// The C# type an action's values are read as. Code generation uses this to pick a wrapper's type
    /// argument, and the inspector uses it to decide which actions an ActionReference&lt;T&gt; accepts, so
    /// both agree by construction.
    /// </summary>
    internal static class ControlValueTypeNames
    {
        /// <summary>
        /// Converts Unity's InputAction.expectedControlType string into the C# type name for that value
        /// (e.g. "Integer" becomes "int").
        /// </summary>
        internal static string FromControlType(string controlType)
        {
            return controlType switch
            {
                "Analog" => "float",
                "Axis" => "float",
                "Bone" => "Bone",
                "Button" => "float",
                "Delta" => "Vector2",
                "Digital" => "int",
                "DiscreteButton" => "int",
                "Double" => "double",
                "Dpad" => "Vector2",
                "Eyes" => "Eyes",
                "Integer" => "int",
                "Pose" => "Pose",
                "Quaternion" => "Quaternion",
                "Stick" => "Vector2",
                "Touch" => "float", // TODO (control types): Unknown
                "Vector2" => "Vector2",
                "Vector3" => "Vector3",
                _ => controlType
            };
        }

        /// <summary>
        /// The same name for a C# type, so a type argument can be compared against what an action reads.
        /// Keywords are used for the primitives, matching what <see cref="FromControlType"/> produces.
        /// </summary>
        internal static string FromType(Type type)
        {
            if (type == typeof(float)) return "float";
            if (type == typeof(int)) return "int";
            if (type == typeof(double)) return "double";
            if (type == typeof(bool)) return "bool";
            return type.Name;
        }

        /// <summary>
        /// The type an action's value is read as, or null for an action with no value to read. Buttons are
        /// excluded: code generation gives them a plain ActionWrapper, which has no ReadValue.
        /// </summary>
        internal static string FromAction(InputAction action)
        {
            if (action.type is not (InputActionType.Value or InputActionType.PassThrough) ||
                string.IsNullOrEmpty(action.expectedControlType))
            {
                return null;
            }

            return FromControlType(action.expectedControlType);
        }
    }
}
