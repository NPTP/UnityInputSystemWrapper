using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// Every rebindable slot of one action on one control scheme, in UI index order.
    /// <para>
    /// Holds the binding data its slots were built from, so dispose it when the screen showing them
    /// closes. A set dropped without being disposed releases its data when the garbage collector
    /// reaches it, which frees it eventually rather than promptly.
    /// </para>
    /// </summary>
    public sealed class BindingSlots : IReadOnlyList<BindingSlot>, IDisposable
    {
        private readonly List<BindingSlot> slots;
        private readonly string actionName;
        private readonly string controlSchemeName;

        /// <summary>Every asset this set took, one entry per take, so each can be given back.</summary>
        private readonly List<AssetReference> held;

        private bool disposed;

        /// <summary>No slots at all, for a lookup that resolved no player or action.</summary>
        internal static BindingSlots Empty { get; } = new(new List<BindingSlot>(), "None", "None", new List<AssetReference>());

        public int Count => slots.Count;

        public BindingSlot this[int uiIndex] => slots[uiIndex];

        private BindingSlots(List<BindingSlot> slots, string actionName, string controlSchemeName, List<AssetReference> held)
        {
            this.slots = slots;
            this.actionName = actionName;
            this.controlSchemeName = controlSchemeName;
            this.held = held;
        }

        ~BindingSlots()
        {
            if (disposed)
            {
                return;
            }

            // A finalizer runs off the main thread, so the releases are queued rather than done here.
            foreach (AssetReference reference in held)
            {
                BindingDataCache.ReleaseLater(reference);
            }
        }

        /// <summary>Gives back the binding data these slots were built from. Safe to call more than once.</summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            foreach (AssetReference reference in held)
            {
                BindingDataCache.Release(reference);
            }

            held.Clear();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The slot at a UI index, or false with a warning naming the indices that do exist.
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
        /// parts, since the composite binding itself carries no group.
        /// </summary>
        internal static BindingSlots Resolve(InputData inputData, InputAction action, ControlSchemeId controlSchemeId)
        {
            List<BindingSlot> resolved = new();
            List<AssetReference> held = new();
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
                            BindingGetter.GetBindingInfos(inputData, bindings, bindingMask, i + 1, partCount, held)));
                    }

                    i += partCount;
                }
                else if (bindingMask.Matches(binding))
                {
                    resolved.Add(new BindingSlot(resolved.Count, i, isComposite: false, 1,
                        BindingGetter.GetBindingInfos(inputData, bindings, bindingMask, i, 1, held)));
                }
            }

            return new BindingSlots(resolved, action.name, controlSchemeId.Name, held);
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
