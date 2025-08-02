using System.ComponentModel;
using NPTP.InputSystemWrapper.Utilities.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace NPTP.InputSystemWrapper.CustomSetups.Bindings
{
    /// <summary>
    /// Custom InputBindingComposite to fix an issue with Digital/Digital Normalized Vector2 Composites.
    /// The issue is that they return diagonals which the UI event system doesn't like for navigation,
    /// leading to inputs that feel incorrect (overly sensitive in the up/down direction, etc).
    /// Here we check what angle the Vector2 has made against four equal 90-degree quadrants, each
    /// representing an up/down/left/right direction, returning a value with NO diagonals so the
    /// event system isn't confused and control feels tight and good.
    /// NOTE: Only meant to be used on UI navigation, though! Not tested anywhere else.
    /// </summary>
    [DisplayStringFormat("{up}/{left}/{down}/{right}")] // This results in WASD.
    [DisplayName("4-Quadrant Digital Up/Down/Left/Right Composite")]
    public sealed class FourQuadrantDigitalVector2Composite : InputBindingComposite<Vector2>
    {
        /// <summary>
        /// Binding for the button that represents the up (that is, <c>(0,1)</c>) direction of the vector.
        /// </summary>
        /// <remarks>
        /// This property is automatically assigned by the input system.
        /// </remarks>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        [InputControl(layout = "Axis")] public int up = 0;

        /// <summary>
        /// Binding for the button represents the down (that is, <c>(0,-1)</c>) direction of the vector.
        /// </summary>
        /// <remarks>
        /// This property is automatically assigned by the input system.
        /// </remarks>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        [InputControl(layout = "Axis")] public int down = 0;

        /// <summary>
        /// Binding for the button represents the left (that is, <c>(-1,0)</c>) direction of the vector.
        /// </summary>
        /// <remarks>
        /// This property is automatically assigned by the input system.
        /// </remarks>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        [InputControl(layout = "Axis")] public int left = 0;

        /// <summary>
        /// Binding for the button that represents the right (that is, <c>(1,0)</c>) direction of the vector.
        /// </summary>
        /// <remarks>
        /// This property is automatically assigned by the input system.
        /// </remarks>
        [InputControl(layout = "Axis")] public int right = 0;

        [Tooltip("The magnitude of the stick value must exceed this value for any direction to register.")]
        public float deadZoneMagnitude = 0.5f;

        /// <inheritdoc />
        public override Vector2 ReadValue(ref InputBindingCompositeContext context)
        {
            float upValue = context.ReadValue<float>(up);
            float downValue = context.ReadValue<float>(down);
            float leftValue = context.ReadValue<float>(left);
            float rightValue = context.ReadValue<float>(right);
            Vector2 vector = new Vector2(-leftValue + rightValue, upValue - downValue);

            if (vector.magnitude < deadZoneMagnitude)
                return Vector2.zero;

            float angleDeg = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

            Vector2 returnValue = Vector2.zero;

            if (angleDeg.BetweenLowerInclusive(-45, 0) || angleDeg.BetweenLowerInclusive(0, 45))
                returnValue = Vector2.right;
            else if (angleDeg.BetweenLowerInclusive(45, 135))
                returnValue = Vector2.up;
            else if (angleDeg.BetweenInclusive(135, 180) || angleDeg.BetweenLowerInclusive(-180, -135))
                returnValue = Vector2.left;
            else if (angleDeg.BetweenLowerInclusive(-135, -45))
                returnValue = Vector2.down;

            return returnValue;
        }

        /// <inheritdoc />
        public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
        {
            Vector2 value = ReadValue(ref context);
            return value.magnitude;
        }
    }
}
