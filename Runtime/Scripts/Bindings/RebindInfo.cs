using NPTP.InputSystemWrapper.Actions;

namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// Information returned in a callback when an interactive rebind is finished.
    /// </summary>
    public readonly struct RebindInfo
    {
        public enum Status
        {
            Failed = 0,
            Canceled,
            Completed
        }

        public ActionWrapper ActionWrapper { get; }
        public Status RebindStatus { get; }

        /// <summary>
        /// Every slot of the action on the control scheme that was rebound, not just the one that changed.
        /// Holds the binding data it was built from, so dispose it once the callback has read what it needs.
        /// </summary>
        public BindingSlots BindingSlots { get; }

        public RebindInfo(ActionWrapper actionWrapper, Status rebindStatus, BindingSlots bindingSlots)
        {
            ActionWrapper = actionWrapper;
            RebindStatus = rebindStatus;
            BindingSlots = bindingSlots;
        }
    }
}
