using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem.Users;

namespace NPTP.InputSystemWrapper
{
    public struct InputUserChangeInfo
    {
        public ControlScheme ControlScheme { get; }
        public InputUserChange InputUserChange { get; }
        
        // MARKER.PlayerIDProperty.Start
        // MARKER.PlayerIDProperty.End
        
        // We pass in the entire inputPlayer because this code can change when multiplayer is activated
        // in the input system wrapper package, and we get more properties from the inputPlayer.
        // Feeding it in this way just makes the code auto-generation easier and exist in less places.
        public InputUserChangeInfo(InputPlayer inputPlayer, InputUserChange inputUserChange)
        {
            ControlScheme = inputPlayer.CurrentControlScheme;
            InputUserChange = inputUserChange;
            
            // MARKER.PlayerIDConstructor.Start
            // MARKER.PlayerIDConstructor.End
        }
    }
}
