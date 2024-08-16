using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace NPTP.InputSystemWrapper
{
    public sealed record DeviceControlInfo
    {
        public InputDevice InputDevice { get; }
        public ControlScheme ControlScheme { get; }
        public InputUserChange InputUserChange { get; }
        
        // MARKER.PlayerIDProperty.Start
        // MARKER.PlayerIDProperty.End
        
        private DeviceControlInfo() { }
        
        public DeviceControlInfo(InputPlayer inputPlayer, InputUserChange inputUserChange)
        {
            InputDevice = inputPlayer.LastUsedDevice;
            ControlScheme = inputPlayer.CurrentControlScheme;
            InputUserChange = inputUserChange;
            
            // MARKER.PlayerIDConstructor.Start
            // MARKER.PlayerIDConstructor.End
        }
    }
}
