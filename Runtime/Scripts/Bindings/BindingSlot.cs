using System.Collections.Generic;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// One rebindable slot of an action, as a rebinding screen sees it. A slot is a single top-level
    /// binding: either a plain binding, or a whole composite with its parts.
    /// <para>
    /// The UI index is the slot's position among the action's slots on one control scheme, counting a
    /// composite as one. A d-pad taking four entries in the input action asset is still one slot, so the
    /// row a player saw it on last is the row it appears on next.
    /// </para>
    /// </summary>
    public readonly struct BindingSlot
    {
        /// <summary>Position among the action's slots on this control scheme, starting at 0.</summary>
        public int UIIndex { get; }

        /// <summary>Whether this slot is a composite, and so has a display entry per part.</summary>
        public bool IsComposite { get; }

        /// <summary>
        /// What to display for this slot: one entry for a plain binding, one per part for a composite,
        /// in the order the parts are declared.
        /// </summary>
        public IReadOnlyList<BindingInfo> BindingInfos { get; }

        /// <summary>
        /// The single entry to display, for the common case of a slot that is not a composite. Null for a
        /// slot with nothing to display, so a screen can fall back rather than throw.
        /// </summary>
        public BindingInfo? BindingInfo => BindingInfos is { Count: > 0 } ? BindingInfos[0] : null;

        /// <summary>
        /// Index of the top-level binding in the action's own binding list. For a composite this is the
        /// composite itself, whose parts follow it.
        /// </summary>
        internal int BindingIndex { get; }

        /// <summary>How many bindings this slot occupies in the action, including a composite's parts.</summary>
        internal int BindingCount { get; }

        internal BindingSlot(int uiIndex, int bindingIndex, bool isComposite, int bindingCount, IReadOnlyList<BindingInfo> bindingInfos)
        {
            UIIndex = uiIndex;
            BindingIndex = bindingIndex;
            IsComposite = isComposite;
            BindingCount = bindingCount;
            BindingInfos = bindingInfos;
        }

        /// <summary>
        /// The binding to rebind for a given part. A composite cannot be rebound as a whole, so a part is
        /// picked out of it; a plain binding ignores the part and rebinds itself.
        /// </summary>
        internal bool TryGetBindingIndexForPart(InputAction action, CompositePart compositePart, out int bindingIndex)
        {
            if (!IsComposite)
            {
                bindingIndex = BindingIndex;
                return true;
            }

            for (int i = BindingIndex + 1; i < BindingIndex + BindingCount; i++)
            {
                if (compositePart.Matches(action.bindings[i]))
                {
                    bindingIndex = i;
                    return true;
                }
            }

            bindingIndex = -1;
            return false;
        }
    }
}
