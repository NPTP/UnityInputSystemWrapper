using NPTP.InputSystemWrapper.Enums;

namespace NPTP.InputSystemWrapper.Actions
{
    internal readonly struct ActionInfo
    {
        internal ActionWrapper ActionWrapper { get; }
        internal bool UseCompositePart { get; }
        internal CompositePart CompositePart { get; }
        
        internal ActionInfo(ActionWrapper actionWrapper, bool useCompositePart, CompositePart compositePart)
        {
            ActionWrapper = actionWrapper;
            UseCompositePart = useCompositePart;
            CompositePart = compositePart;
        }
        
        internal ActionInfo(ActionWrapper actionWrapper, bool useCompositePart) : this()
        {
            ActionWrapper = actionWrapper;
            UseCompositePart = useCompositePart;
        }
    }
}
