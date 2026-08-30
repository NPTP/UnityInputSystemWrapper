using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Actions;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace NPTP.InputSystemWrapper
{
    public static partial class ISW
    {
        // MARKER.SinglePlayerFieldsAndProperties.Start
        public static PlayerActions Player => DefaultPlayer.Player();
        public static UIActions UI => DefaultPlayer.UI();
        public static ControlScheme CurrentControlScheme => DefaultPlayer.CurrentControlScheme;
        // MARKER.SinglePlayerFieldsAndProperties.End
        
        // MARKER.Initialize.Start
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        // MARKER.Initialize.End
        {
            InputRuntime.Initialize();
        }
    }
}
