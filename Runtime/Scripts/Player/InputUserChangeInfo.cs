using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem.Users;

namespace NPTP.InputSystemWrapper.Player
{
    public struct InputUserChangeInfo
    {
        public InputPlayer Player { get; }
        public ControlScheme ControlScheme { get; }
        public InputUserChange InputUserChange { get; }
        
        internal InputUserChangeInfo(InputPlayer inputPlayer, InputUserChange inputUserChange)
        {
            Player = inputPlayer;
            ControlScheme = inputPlayer.CurrentControlScheme;
            InputUserChange = inputUserChange;
        }
    }
}
