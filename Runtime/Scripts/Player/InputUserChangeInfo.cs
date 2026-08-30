using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem.Users;

namespace NPTP.InputSystemWrapper.Player
{
    public struct InputUserChangeInfo
    {
        public InputPlayer Player { get; }
        public InputUserChange InputUserChange { get; }

        internal ControlSchemeId ControlSchemeId { get; }

        internal InputUserChangeInfo(InputPlayer inputPlayer, InputUserChange inputUserChange)
        {
            Player = inputPlayer;
            ControlSchemeId = inputPlayer.CurrentControlSchemeId;
            InputUserChange = inputUserChange;
        }
    }
}
