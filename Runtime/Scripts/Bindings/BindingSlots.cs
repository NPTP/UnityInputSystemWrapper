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

            // One device's data serves every binding on it, so it is taken once for the whole resolve.
            Dictionary<string, BindingData> loadedByDevice = new();
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
                        List<CompositePart> parts = new();
                        resolved.Add(new BindingSlot(resolved.Count, i, isComposite: true, partCount + 1,
                            BindingGetter.GetBindingInfos(inputData, bindings, bindingMask, i + 1, partCount, held, loadedByDevice, parts),
                            parts));
                    }

                    i += partCount;
                }
                else if (bindingMask.Matches(binding))
                {
                    List<CompositePart> parts = new();
                    resolved.Add(new BindingSlot(resolved.Count, i, isComposite: false, 1,
                        BindingGetter.GetBindingInfos(inputData, bindings, bindingMask, i, 1, held, loadedByDevice, parts),
                        parts));
                }
            }

            return new BindingSlots(resolved, action.name, controlSchemeId.Name, held);
        }

        /// <summary>
        /// The same slots as <see cref="Resolve"/>, without stalling the frame: the assets load in the
        /// background and the callback runs once they are all in. A screen full of glyphs can open on time
        /// and fill in as each one arrives.
        /// </summary>
        internal static void ResolveAsync(InputData inputData, InputAction action, ControlSchemeId controlSchemeId,
            Action<BindingSlots> onResolved)
        {
            List<AssetReference> held = new();
            List<(int SlotIndex, string DeviceLayoutName, string PathOnDevice, CompositePart Part)> needed = new();
            List<BindingSlot> resolved = PlanSlots(action, controlSchemeId, needed);

            if (needed.Count == 0)
            {
                onResolved?.Invoke(new BindingSlots(resolved, action.name, controlSchemeId.Name, held));
                return;
            }

            LoadDeviceData(inputData, needed, held, loadedByDevice =>
            {
                List<List<BindingInfo>> infosBySlot = new();
                List<List<CompositePart>> partsBySlot = new();
                for (int i = 0; i < resolved.Count; i++)
                {
                    infosBySlot.Add(new List<BindingInfo>());
                    partsBySlot.Add(new List<CompositePart>());
                }

                foreach ((int slotIndex, string deviceLayoutName, string pathOnDevice, CompositePart part) in needed)
                {
                    loadedByDevice.TryGetValue(deviceLayoutName, out BindingData bindingData);
                    BindingInfo bindingInfo = BindingGetter.TakeLoadedBindingInfo(bindingData, pathOnDevice, held);
                    if (bindingInfo != null)
                    {
                        infosBySlot[slotIndex].Add(bindingInfo);
                        partsBySlot[slotIndex].Add(part);
                    }
                }

                for (int i = 0; i < resolved.Count; i++)
                {
                    resolved[i] = resolved[i].WithBindingInfos(infosBySlot[i], partsBySlot[i]);
                }

                onResolved?.Invoke(new BindingSlots(resolved, action.name, controlSchemeId.Name, held));
            });
        }

        /// <summary>
        /// The slots an action has on a control scheme, with no display info on them yet, and what each one
        /// will need loading. Shares its walk with <see cref="Resolve"/> through the same helpers, so the
        /// two cannot disagree about what counts as a slot.
        /// </summary>
        private static List<BindingSlot> PlanSlots(InputAction action, ControlSchemeId controlSchemeId,
            List<(int, string, string, CompositePart)> needed)
        {
            List<BindingSlot> planned = new();
            InputBinding bindingMask = controlSchemeId.ToBindingMask();
            ReadOnlyArray<InputBinding> bindings = action.bindings;

            for (int i = 0; i < bindings.Count; i++)
            {
                InputBinding binding = bindings[i];
                if (binding.isPartOfComposite)
                {
                    continue;
                }

                int startIndex = i;
                int count = 1;
                bool isComposite = binding.isComposite;

                if (isComposite)
                {
                    int partCount = CountParts(bindings, i);
                    startIndex = i + 1;
                    count = partCount;
                    i += partCount;

                    if (!AnyMatches(bindings, bindingMask, startIndex, partCount))
                    {
                        continue;
                    }
                }
                else if (!bindingMask.Matches(binding))
                {
                    continue;
                }

                int slotIndex = planned.Count;
                planned.Add(new BindingSlot(slotIndex, isComposite ? startIndex - 1 : startIndex, isComposite,
                    isComposite ? count + 1 : 1, null, null));

                foreach ((string deviceLayoutName, string pathOnDevice, CompositePart part) in
                         BindingGetter.GetNeededEntries(bindings, bindingMask, startIndex, count))
                {
                    needed.Add((slotIndex, deviceLayoutName, pathOnDevice, part));
                }
            }

            return planned;
        }

        /// <summary>
        /// Load the binding data for every device the slots touch, once each, then hand them over together.
        /// </summary>
        private static void LoadDeviceData(InputData inputData,
            List<(int SlotIndex, string DeviceLayoutName, string PathOnDevice, CompositePart Part)> needed, List<AssetReference> held,
            Action<Dictionary<string, BindingData>> onLoaded)
        {
            HashSet<string> deviceLayoutNames = new();
            foreach ((int _, string deviceLayoutName, string _, CompositePart _) in needed) deviceLayoutNames.Add(deviceLayoutName);

            Dictionary<string, BindingData> loadedByDevice = new();
            int remaining = deviceLayoutNames.Count;

            foreach (string deviceLayoutName in deviceLayoutNames)
            {
                AssetReference reference = BindingGetter.GetBindingDataReference(inputData, deviceLayoutName);
                if (reference == null)
                {
                    ISWDebug.LogWarning($"Device {deviceLayoutName} has no {nameof(BindingData)} and cannot produce display names/sprites!");
                    if (--remaining == 0) onLoaded(loadedByDevice);
                    continue;
                }

                string capturedName = deviceLayoutName;
                BindingDataCache.AcquireAsync<BindingData>(reference, bindingData =>
                {
                    if (bindingData != null)
                    {
                        loadedByDevice[capturedName] = bindingData;
                        held.Add(reference);
                    }

                    if (--remaining == 0) onLoaded(loadedByDevice);
                });
            }
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
