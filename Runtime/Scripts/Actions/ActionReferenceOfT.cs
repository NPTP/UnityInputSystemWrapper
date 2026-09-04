using System;
using NPTP.InputSystemWrapper.Utilities;

namespace NPTP.InputSystemWrapper.Actions
{
    /// <summary>
    /// An ActionReference to an action whose values are read as T, giving the same ReadValue as going
    /// through the generated ISW properties. The inspector only offers actions that read as T, so a
    /// reference that compiles is a reference that reads.
    /// </summary>
    /// <example>
    /// <code>
    /// [SerializeField] private ActionReference&lt;Vector2&gt; move;
    /// private void Update() =&gt; transform.Translate(move.ReadValue());
    /// </code>
    /// </example>
    [Serializable]
    public class ActionReference<T> : ActionReference where T : struct
    {
        /// <summary>
        /// The typed wrapper for the referenced action, or null if the reference is unassigned or names an
        /// action that does not read as T.
        /// </summary>
        public ValueActionWrapper<T> ValueActionWrapper
        {
            get
            {
                ActionWrapper actionWrapper = ActionWrapper;
                if (actionWrapper == null)
                {
                    return null;
                }

                if (actionWrapper is ValueActionWrapper<T> valueActionWrapper)
                {
                    return valueActionWrapper;
                }

                ISWDebug.LogWarning($"Action {ActionName} does not read as {typeof(T).Name}, so it has no value to give.");
                return null;
            }
        }

        /// <summary>The action's current value, or default if there is nothing to read it from.</summary>
        public T ReadValue()
        {
            ValueActionWrapper<T> valueActionWrapper = ValueActionWrapper;
            return valueActionWrapper?.ReadValue() ?? default;
        }
    }
}
