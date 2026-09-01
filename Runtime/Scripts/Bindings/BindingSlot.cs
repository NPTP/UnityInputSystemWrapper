using System.Collections.Generic;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// One rebindable slot of an action: a single top-level binding, either plain or a whole composite
    /// with its parts. A composite counts as one slot, so a d-pad taking four entries in the input
    /// action asset still occupies a single row of a rebinding screen.
    /// </summary>
    public readonly struct BindingSlot
    {
        /// <summary>Position among the action's slots on this control scheme, starting at 0.</summary>
        public int UIIndex { get; }

        /// <summary>Whether this slot is a composite, and so has a display entry per part.</summary>
        public bool IsComposite { get; }

        /// <summary>
        /// What to display: one entry for a plain binding, one per part for a composite, in declared order.
        /// </summary>
        public IReadOnlyList<BindingInfo> BindingInfos { get; }

        /// <summary>The single entry to display, or null if there is none.</summary>
        public BindingInfo BindingInfo => BindingInfos is { Count: > 0 } ? BindingInfos[0] : null;

        /// <summary>
        /// Index in the action's own binding list. For a composite this is the composite itself, whose
        /// parts follow it.
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
        /// The same slot carrying its display entries, for a resolve that works out its slots before the
        /// assets describing them have finished loading.
        /// </summary>
        internal BindingSlot WithBindingInfos(IReadOnlyList<BindingInfo> bindingInfos) =>
            new(UIIndex, BindingIndex, IsComposite, BindingCount, bindingInfos);

        /// <summary>
        /// The binding to rebind for a part. A composite picks out that part; a plain binding ignores it.
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
