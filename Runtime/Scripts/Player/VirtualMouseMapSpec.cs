using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Player
{
    /// <summary>
    /// What an action map must hold to drive a virtual mouse. One source of truth for the runtime that
    /// reads such a map, the inspector that checks one, and the button that writes one.
    /// </summary>
    internal static class VirtualMouseMapSpec
    {
        internal const string DEFAULT_MAP_NAME = "VirtualMouse";

        /// <summary>One action a virtual mouse map must have.</summary>
        internal readonly struct ActionSpec
        {
            internal string Name { get; }
            internal InputActionType ActionType { get; }
            internal string ExpectedControlType { get; }

            /// <summary>What a newly written map binds this to, so it drives a mouse straight away.</summary>
            internal string DefaultBinding { get; }

            internal ActionSpec(string name, InputActionType actionType, string expectedControlType, string defaultBinding)
            {
                Name = name;
                ActionType = actionType;
                ExpectedControlType = expectedControlType;
                DefaultBinding = defaultBinding;
            }
        }

        internal const string MOVE = "Move";
        internal const string LEFT_BUTTON = "LeftButton";
        internal const string RIGHT_BUTTON = "RightButton";
        internal const string MIDDLE_BUTTON = "MiddleButton";
        internal const string SCROLL_WHEEL = "ScrollWheel";

        /// <summary>Every action the map must have, and nothing else.</summary>
        internal static IReadOnlyList<ActionSpec> Actions { get; } = new List<ActionSpec>
        {
            new(MOVE, InputActionType.Value, "Vector2", "<Gamepad>/leftStick"),
            new(LEFT_BUTTON, InputActionType.Button, "Button", "<Gamepad>/buttonSouth"),
            new(RIGHT_BUTTON, InputActionType.Button, "Button", "<Gamepad>/buttonEast"),
            new(MIDDLE_BUTTON, InputActionType.Button, "Button", "<Gamepad>/buttonWest"),
            new(SCROLL_WHEEL, InputActionType.Value, "Vector2", "<Gamepad>/rightStick")
        };

        /// <summary>
        /// What is wrong with a map, or empty when it holds exactly the actions above with the right types.
        /// A null map is reported as missing entirely.
        /// </summary>
        internal static List<string> Problems(InputActionMap actionMap)
        {
            List<string> problems = new();
            if (actionMap == null)
            {
                problems.Add("The map does not exist in the input action asset.");
                return problems;
            }

            foreach (ActionSpec actionSpec in Actions)
            {
                InputAction action = actionMap.FindAction(actionSpec.Name, throwIfNotFound: false);
                if (action == null)
                {
                    problems.Add($"Missing action \"{actionSpec.Name}\" ({actionSpec.ActionType.ToString()}, {actionSpec.ExpectedControlType}).");
                    continue;
                }

                if (action.type != actionSpec.ActionType)
                {
                    problems.Add($"Action \"{actionSpec.Name}\" is a {action.type.ToString()}, and must be a {actionSpec.ActionType.ToString()}.");
                }

                if (!string.IsNullOrEmpty(action.expectedControlType) &&
                    action.expectedControlType != actionSpec.ExpectedControlType)
                {
                    problems.Add($"Action \"{actionSpec.Name}\" expects {action.expectedControlType}, and must expect {actionSpec.ExpectedControlType}.");
                }
            }

            foreach (InputAction action in actionMap.actions)
            {
                if (!IsInSpec(action.name))
                {
                    problems.Add($"Action \"{action.name}\" does not belong in a virtual mouse map and should be removed.");
                }
            }

            return problems;
        }

        private static bool IsInSpec(string actionName)
        {
            foreach (ActionSpec actionSpec in Actions)
            {
                if (actionSpec.Name == actionName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
