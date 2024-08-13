using UnityEngine.InputSystem;

namespace UnityInputSystemWrapper
{
    public sealed record DeviceControlInfo
    {
        public InputDevice InputDevice { get; }
        public ControlScheme ControlScheme { get; }
        
        // MARKER.PlayerIDProperty.Start
        public PlayerID PlayerID { get; }
        // MARKER.PlayerIDProperty.End
        
        private DeviceControlInfo() { }
        
        public DeviceControlInfo(InputPlayer inputPlayer)
        {
            InputDevice = inputPlayer.LastUsedDevice;
            ControlScheme = inputPlayer.CurrentControlScheme;
            
            // MARKER.PlayerIDConstructor.Start
            PlayerID = inputPlayer.ID;
            // MARKER.PlayerIDConstructor.End
        }
    }
}
