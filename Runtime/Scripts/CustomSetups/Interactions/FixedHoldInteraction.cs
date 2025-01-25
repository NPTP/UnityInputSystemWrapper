using System.ComponentModel;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Editor;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NPTP.InputSystemWrapper.InputDevices.Interactions
{
    /// <summary>
    /// Code almost fully copied from Unity's own HoldInteraction, but with crucial parts of the "Process" method
    /// fixed to prevent unpredictable or undesired behaviour from actions with a FixedHoldInteraction (and
    /// other internal assembly references replaced with usable ones).
    /// For example, with Unity's HoldInteraction, in some cases (e.g. during InputActionMap enabling/disabling)
    /// it is possible for an InputAction to go from InputActionPhase.Waiting -> InputActionPhase.Canceled which goes
    /// against all intuition of needing to pass through InputActionPhase.Started/Performed first. This should fix that!
    /// </summary>
    [DisplayName("Hold (with fixed canceling)")]
    public class FixedHoldInteraction : IInputInteraction
    {
        public float duration;
        public float pressPoint;
        private float durationOrDefault => duration > 0.0 ? duration : InputSystem.settings.defaultHoldTime;
        private float pressPointOrDefault => pressPoint > 0.0 ? pressPoint : InputSystem.settings.defaultButtonPressPoint;
        private double m_TimePressed;
        
        internal static void Register()
        {
            InputSystem.RegisterInteraction<FixedHoldInteraction>();
        }

        public void Process(ref InputInteractionContext context)
        {
            // TODO: This doesn't make the action perform or cancel properly as expected.
            // (All it does now is prevent things from canceling when they shouldn't.)
            if (context.timerHasExpired)
            {
                if (context.ControlIsActuated())
                {
                    context.PerformedAndStayPerformed();
                }
                else
                {
                    context.Waiting();
                }
               
                return;
            }
            
            // From here the rest of the method is the same as HoldInteraction.

            switch (context.phase)
            {
                case InputActionPhase.Waiting:
                    if (context.ControlIsActuated(pressPointOrDefault))
                    {
                        m_TimePressed = context.time;

                        context.Started();
                        context.SetTimeout(durationOrDefault);
                    }
                    break;

                case InputActionPhase.Started:
                    // If we've reached our hold time threshold, perform the hold.
                    // We do this regardless of what state the control changed to.
                    if (context.time - m_TimePressed >= durationOrDefault)
                    {
                        context.PerformedAndStayPerformed();
                    }
                    if (!context.ControlIsActuated())
                    {
                        // Control is no longer actuated so we're done.
                        context.Canceled();
                    }
                    break;

                case InputActionPhase.Performed:
                    if (!context.ControlIsActuated(pressPointOrDefault))
                        context.Canceled();
                    break;
            }
        }

        public void Reset()
        {
            m_TimePressed = 0;
        }
    }

#if UNITY_EDITOR
    internal class HoldInteractionEditor : InputParameterEditor<FixedHoldInteraction>
    {
        protected override void OnEnable()
        {
            m_PressPointSetting.Initialize("Press Point",
                "Float value that an axis control has to cross for it to be considered pressed.",
                "Default Button Press Point",
                () => target.pressPoint, v => target.pressPoint = v, () => InputSystem.settings.defaultButtonPressPoint);
            m_DurationSetting.Initialize("Hold Time",
                "Time (in seconds) that a control has to be held in order for it to register as a hold.",
                "Default Hold Time",
                () => target.duration, x => target.duration = x, () => InputSystem.settings.defaultHoldTime);
        }

        public override void OnGUI()
        {
            m_PressPointSetting.OnGUI();
            m_DurationSetting.OnGUI();
        }

        private CustomOrDefaultSetting m_PressPointSetting;
        private CustomOrDefaultSetting m_DurationSetting;
    }
#endif
}
