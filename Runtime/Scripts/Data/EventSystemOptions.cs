using System;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// The InputSystemUIInputModule settings and default event system actions applied to every player,
    /// baked from the offline input data by the input script generator.
    /// </summary>
    [Serializable]
    internal class EventSystemOptions
    {
        [SerializeField] private float moveRepeatDelay;
        internal float MoveRepeatDelay => moveRepeatDelay;

        [SerializeField] private float moveRepeatRate;
        internal float MoveRepeatRate => moveRepeatRate;

        [SerializeField] private bool deselectOnBackgroundClick;
        internal bool DeselectOnBackgroundClick => deselectOnBackgroundClick;

        [SerializeField] private UIPointerBehavior pointerBehavior;
        internal UIPointerBehavior PointerBehavior => pointerBehavior;

        [SerializeField] private InputSystemUIInputModule.CursorLockBehavior cursorLockBehavior;
        internal InputSystemUIInputModule.CursorLockBehavior CursorLockBehavior => cursorLockBehavior;

        [SerializeField] private EventSystemActionBinding[] defaultActions;
        internal EventSystemActionBinding[] DefaultActions => defaultActions;

#if UNITY_EDITOR
        internal const string EDITOR_MoveRepeatDelayField = nameof(moveRepeatDelay);
        internal const string EDITOR_MoveRepeatRateField = nameof(moveRepeatRate);
        internal const string EDITOR_DeselectOnBackgroundClickField = nameof(deselectOnBackgroundClick);
        internal const string EDITOR_PointerBehaviorField = nameof(pointerBehavior);
        internal const string EDITOR_CursorLockBehaviorField = nameof(cursorLockBehavior);
        internal const string EDITOR_DefaultActionsField = nameof(defaultActions);
#endif
    }
}
