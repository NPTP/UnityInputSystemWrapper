using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Player;
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
    public class InputBindingDisplay : MonoBehaviour
    {
        [Tooltip("The action to display a binding for.")]
        [SerializeField] private ActionReference actionReference;

        [Tooltip("Which of the action's bindings on the current control scheme to show, as laid out on a rebinding screen.")]
        [SerializeField] private int uiIndex;

        [Tooltip("Handle to put the binding's name on a text component, TextMeshPro or otherwise.")]
        [SerializeField] private UnityEvent<string> onDisplayName;

        [Tooltip("Handle to put the binding's sprite on a sprite renderer, a UI Image, or anywhere else.")]
        [SerializeField] private UnityEvent<Sprite> onSprite;

        /// <summary>What is on screen now, held so its assets can be given back.</summary>
        private BindingSlots bindingSlots;

        /// <summary>
        /// Tells a load that finishes after this was disabled or asked to load again that its result is no
        /// longer wanted, so it is released rather than shown.
        /// </summary>
        private int loadGeneration;

        private void OnEnable()
        {
            InputRuntime.Current.OnAnyPlayerInputUserChange += HandleAnyPlayerInputUserChange;
            InputRuntime.Current.OnBindingsChanged += HandleBindingsChanged;
            Refresh();
        }

        private void OnDisable()
        {
            InputRuntime.Current.OnAnyPlayerInputUserChange -= HandleAnyPlayerInputUserChange;
            InputRuntime.Current.OnBindingsChanged -= HandleBindingsChanged;

            loadGeneration++;
            Release();
        }

        /// <summary>Load this binding again, e.g. after changing which action is shown in code.</summary>
        public void Refresh()
        {
            if (actionReference == null)
            {
                return;
            }

            int generation = ++loadGeneration;
            actionReference.GetCurrentBindingSlotsAsync(slots =>
            {
                if (generation != loadGeneration)
                {
                    slots.Dispose();
                    return;
                }

                Release();
                bindingSlots = slots;
                Display(slots);
            });
        }

        private void HandleAnyPlayerInputUserChange(InputUserChangeInfo inputUserChangeInfo) => Refresh();
        private void HandleBindingsChanged() => Refresh();

        private void Display(BindingSlots slots)
        {
            if (!slots.TryGetAtUIIndex(uiIndex, out BindingSlot slot) || slot.BindingInfo == null)
            {
                return;
            }

            BindingInfo bindingInfo = slot.BindingInfo;
            onDisplayName?.Invoke(bindingInfo.DisplayName);
            onSprite?.Invoke(bindingInfo.Sprite);
        }

        private void Release()
        {
            bindingSlots?.Dispose();
            bindingSlots = null;
        }
    }
}
