using System;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Bindings;
using UnityEngine;
using UnityEngine.Events;

namespace NPTP.InputSystemWrapper.Components
{
    /// <summary>
    /// Shows one binding of an action: its name and its sprite. Place it, choose an action, and handle
    /// whichever of the events you need - anything left unhandled is simply not shown.
    /// <para>
    /// The binding's assets load in the background, so a screen full of these opens on time and each glyph
    /// appears as it arrives rather than every one of them stalling the frame. They are released when this
    /// is disabled, so a rebinding screen costs nothing once closed.
    /// </para>
    /// </summary>
    public class InputBindingDisplay : InputDisplayBehaviour<BindingSlots>
    {
        [Tooltip("The action to display a binding for.")]
        [SerializeField] private ActionReference actionReference;

        [Tooltip("Which of the action's bindings on the current control scheme to show, as laid out on a rebinding screen.")]
        [Min(0)]
        [SerializeField] private int uiIndex;

        [Tooltip("Handle to put the binding's name on a text component, TextMeshPro or otherwise.")]
        [SerializeField] private UnityEvent<string> onDisplayName;

        [Tooltip("Handle to put the binding's sprite on a sprite renderer, a UI Image, or anywhere else.")]
        [SerializeField] private UnityEvent<Sprite> onSprite;

        /// <summary>
        /// Which player's bindings are shown. Setting it points the reference at that player and loads
        /// again, so one screen can be walked through the players in turn.
        /// </summary>
        public int PlayerID
        {
            get => actionReference?.PlayerID ?? 0;
            set
            {
                if (actionReference == null || actionReference.PlayerID == value)
                {
                    return;
                }

                actionReference.PlayerID = value;

                // Enabling loads anyway, so a change while disabled needs nothing more than the new value.
                if (isActiveAndEnabled)
                {
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Which of the action's bindings is shown. Setting it repaints from what is already loaded
        /// rather than loading again, since the slots hold every binding the action has.
        /// </summary>
        public int UIIndex
        {
            get => uiIndex;
            set
            {
                int clamped = Mathf.Max(0, value);
                if (uiIndex == clamped)
                {
                    return;
                }

                uiIndex = clamped;
                Redisplay();
            }
        }

        protected override bool CanLoad => actionReference != null;

        protected override void Load(Action<BindingSlots> onLoaded) => actionReference.GetCurrentBindingSlotsAsync(onLoaded);

        protected override void Display(BindingSlots slots)
        {
            // The reference says which part of a composite it means, so a display wired to the up part of
            // a movement composite shows that part rather than the whole binding's first control.
            if (!slots.TryGetAtUIIndex(uiIndex, out BindingSlot slot) ||
                !slot.TryGetBindingInfo(actionReference.CompositePart, out BindingInfo bindingInfo))
            {
                // Cleared rather than left as it was, so moving to a binding the action does not have
                // does not leave the previous one on screen.
                onDisplayName?.Invoke(string.Empty);
                onSprite?.Invoke(null);
                return;
            }

            onDisplayName?.Invoke(bindingInfo.DisplayName);
            onSprite?.Invoke(bindingInfo.Sprite);
        }
    }
}
