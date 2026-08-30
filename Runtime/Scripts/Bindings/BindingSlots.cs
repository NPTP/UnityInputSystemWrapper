using System.Collections;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// The rebindable slots of one action on one control scheme, in the order a rebinding screen should
    /// lay them out. Every binding the scheme can fire is in here, so nothing can act on the player
    /// without appearing on the screen they rebind it from.
    /// </summary>
    public sealed class BindingSlots : IReadOnlyList<BindingSlot>
    {
        private readonly List<BindingSlot> slots;
        private readonly string actionName;
        private readonly string controlSchemeName;

        /// <summary>No slots at all, for a lookup that could not resolve a player or an action.</summary>
        internal static BindingSlots Empty { get; } = new(new List<BindingSlot>(), "None", "None");

        public int Count => slots.Count;

        public BindingSlot this[int uiIndex] => slots[uiIndex];

        private BindingSlots(List<BindingSlot> slots, string actionName, string controlSchemeName)
        {
            this.slots = slots;
            this.actionName = actionName;
            this.controlSchemeName = controlSchemeName;
        }

        /// <summary>
        /// The slot at a UI index, or false with a warning naming what is actually there. Use this to fill
        /// a rebinding screen, so a row the action does not have leaves the row empty rather than throwing.
        /// </summary>
        public bool TryGetAtUIIndex(int uiIndex, out BindingSlot bindingSlot)
        {
            if (uiIndex >= 0 && uiIndex < slots.Count)
            {
                bindingSlot = slots[uiIndex];
                return true;
            }

            ISWDebug.LogWarning($"No binding at UI index {uiIndex} for action {actionName} on control scheme " +
                                $"{controlSchemeName}. {DescribeIndices()}");
            bindingSlot = default;
            return false;
        }

        private string DescribeIndices()
        {
            return slots.Count == 0
                ? "It has no bindings on this control scheme at all."
                : $"It has {slots.Count} binding(s), at UI indices 0 to {slots.Count - 1}.";
        }

        public IEnumerator<BindingSlot> GetEnumerator() => slots.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Group an action's bindings into slots for one control scheme. A composite is matched by its
        /// parts, since the composite binding itself carries no control scheme group.
        /// </summary>
        internal static BindingSlots Resolve(InputData inputData, InputAction action, ControlSchemeId controlSchemeId)
        {
            List<BindingSlot> resolved = new();
            InputBinding bindingMask = controlSchemeId.ToBindingMask();
            ReadOnlyArray<InputBinding> bindings = action.bindings;

            for (int i = 0; i < bindings.Count; i++)
            {
                InputBinding binding = bindings[i];
                if (binding.isPartOfComposite)
                {
                    continue;
                }

                if (binding.isComposite)
                {
                    int partCount = CountParts(bindings, i);
                    if (AnyMatches(bindings, bindingMask, i + 1, partCount))
                    {
                        resolved.Add(new BindingSlot(resolved.Count, i, isComposite: true, partCount + 1,
                            BindingGetter.GetBindingInfos(inputData, bindings, bindingMask, i + 1, partCount)));
                    }

                    i += partCount;
                }
                else if (bindingMask.Matches(binding))
                {
                    resolved.Add(new BindingSlot(resolved.Count, i, isComposite: false, 1,
                        BindingGetter.GetBindingInfos(inputData, bindings, bindingMask, i, 1)));
                }
            }

            return new BindingSlots(resolved, action.name, controlSchemeId.Name);
        }

        private static int CountParts(ReadOnlyArray<InputBinding> bindings, int compositeIndex)
        {
            int partCount = 0;
            for (int i = compositeIndex + 1; i < bindings.Count && bindings[i].isPartOfComposite; i++)
            {
                partCount++;
            }

            return partCount;
        }

        private static bool AnyMatches(ReadOnlyArray<InputBinding> bindings, InputBinding bindingMask, int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {
                if (bindingMask.Matches(bindings[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
