using NPTP.InputSystemWrapper.Enums;

namespace NPTP.InputSystemWrapper.Actions
{
    internal readonly struct ActionBindingInfo
    {
        internal ActionWrapper ActionWrapper { get; }
        internal CompositePart CompositePart { get; }
        internal ControlSchemeId ControlSchemeId { get; }

        /// <summary>Which of the action's slots on this control scheme to act on.</summary>
        internal int UIIndex { get; }

        internal bool DontUseCompositePart => CompositePart == CompositePart.DontIsolatePart;
        internal bool UseCompositePart => !DontUseCompositePart;

        internal ActionBindingInfo(ActionWrapper actionWrapper, CompositePart compositePart, ControlSchemeId controlSchemeId, int uiIndex = 0)
        {
            ActionWrapper = actionWrapper;
            CompositePart = compositePart;
            ControlSchemeId = controlSchemeId;
            UIIndex = uiIndex;
        }
    }
}
