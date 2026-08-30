using NPTP.InputSystemWrapper.Enums;

namespace NPTP.InputSystemWrapper.Actions
{
    internal readonly struct ActionBindingInfo
    {
        internal ActionWrapper ActionWrapper { get; }
        internal CompositePart CompositePart { get; }
        internal ControlSchemeId ControlSchemeId { get; }
        
        internal bool DontUseCompositePart => CompositePart == CompositePart.DontIsolatePart;
        internal bool UseCompositePart => !DontUseCompositePart;
        
        internal ActionBindingInfo(ActionWrapper actionWrapper, CompositePart compositePart, ControlSchemeId controlSchemeId)
        {
            ActionWrapper = actionWrapper;
            CompositePart = compositePart;
            ControlSchemeId = controlSchemeId;
        }
    }
}
