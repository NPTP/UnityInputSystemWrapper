using System.Collections.Generic;
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
        public IEnumerable<BindingInfo> BindingInfos { get; }

        public RebindInfo(ActionWrapper actionWrapper, Status rebindStatus, IEnumerable<BindingInfo> bindingInfos)
        {
            ActionWrapper = actionWrapper;
            RebindStatus = rebindStatus;
            BindingInfos = bindingInfos;
        }
    }
}
