using NPTP.InputSystemWrapper.Enums;

namespace NPTP.InputSystemWrapper.Actions
{
    internal readonly struct ActionBindingInfo
    {
        internal ActionWrapper ActionWrapper { get; }
        internal CompositePart CompositePart { get; }
        internal ControlScheme ControlScheme { get; }
        
        internal bool DontUseCompositePart => CompositePart == CompositePart.DontIsolatePart;
        internal bool UseCompositePart => !DontUseCompositePart;
        
        internal ActionBindingInfo(ActionWrapper actionWrapper, CompositePart compositePart, ControlScheme controlScheme)
        {
            ActionWrapper = actionWrapper;
            CompositePart = compositePart;
            ControlScheme = controlScheme;
        }
    }
}
