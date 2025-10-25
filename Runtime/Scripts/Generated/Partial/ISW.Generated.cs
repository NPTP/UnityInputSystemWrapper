using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Generated.Actions;
using UnityEngine;

namespace NPTP.InputSystemWrapper
{
    public static partial class ISW
    {
        // MARKER.SinglePlayerFieldsAndProperties.Start
        public static GameplayActions Gameplay => DefaultPlayer.Gameplay;
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
