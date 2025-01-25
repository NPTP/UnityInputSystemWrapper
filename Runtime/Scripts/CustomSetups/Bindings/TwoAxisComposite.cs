using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace NPTP.InputSystemWrapper.CustomSetups.Bindings
{
    [DisplayStringFormat("Axes {xAxis}, {yAxis}")]
    [DisplayName("X and Y Axes Composite")]
    public class TwoAxisComposite : InputBindingComposite<Vector2>
    {
        [InputControl(layout = "Axis")] public int xAxis;
        [InputControl(layout = "Axis")] public int yAxis;

        public override Vector2 ReadValue(ref InputBindingCompositeContext context)
        {
            return new Vector2(
                context.ReadValue<float>(xAxis),
                context.ReadValue<float>(yAxis) * -1f // Axis is inverted.
            );
        }

        internal static void Register()
        {
            InputSystem.RegisterBindingComposite<TwoAxisComposite>();
        }
    }
}