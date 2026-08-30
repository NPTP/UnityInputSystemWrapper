using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Generated.Actions;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace NPTP.InputSystemWrapper
{
    public static partial class ISW
    {
        // MARKER.SinglePlayerFieldsAndProperties.Start
        public static PlayerActions Player => DefaultPlayer.Player;
        public static UIActions UI => DefaultPlayer.UI;
        public static ControlScheme CurrentControlScheme => DefaultPlayer.CurrentControlScheme;
        // MARKER.SinglePlayerFieldsAndProperties.End
        
        // MARKER.DefaultContextProperty.Start
        private static InputContext DefaultContext => InputContext.Default;
        // MARKER.DefaultContextProperty.End
        
        // MARKER.Initialize.Start
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        // MARKER.Initialize.End
        {
            InitializationProcess();
        }

        private static void SetUpBindings()
        {
            // MARKER.LoadAllBindingsOnInitialization.Start
            LoadBindingsForAllPlayers();
            // MARKER.LoadAllBindingsOnInitialization.End
        }
    }
}
