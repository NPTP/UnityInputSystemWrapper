using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Generated.Actions;
using UnityEngine;

namespace NPTP.InputSystemWrapper
{
    public static partial class ISW
    {
        // MARKER.SinglePlayerFieldsAndProperties.Start
        // MARKER.SinglePlayerFieldsAndProperties.End
        
        // MARKER.DefaultContextProperty.Start
        private static InputContext DefaultContext => 0;
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
