using System;
using System.Linq;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;
using RebindingOperation = UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation;

using NPTP.InputSystemWrapper;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingChanger
    {
        internal static RebindingOperation StartInteractiveRebind(InputData inputData, ActionBindingInfo actionBindingInfo, int bindingIndex, Action<RebindInfo> callback)
        {
            string[] excludedPaths = inputData.BindingExcludedPaths ?? Array.Empty<string>();
            string[] cancelPaths = inputData.BindingCancelPaths ?? Array.Empty<string>();

            ActionWrapper actionWrapper = actionBindingInfo.ActionWrapper;
            InputAction action = actionWrapper.InputAction;
            bool actionWasEnabled = action.enabled;
            action.Disable();

            RebindingOperation rebindingOperation = action.PerformInteractiveRebinding(bindingIndex);

            rebindingOperation
                // Note that pointer movement (including touch) is already excluded in the above call to PerformInteractiveRebinding.
                .WithControlsExcludingMultiple(excludedPaths)
                .WithCancelingThroughMultiple(cancelPaths)
                .OnCancel(onCancel)
                .OnComplete(onComplete);

            // Zero or less is the input system's own way of saying "wait indefinitely".
            if (inputData.RebindTimeoutSeconds > 0)
            {
                rebindingOperation.WithTimeout(inputData.RebindTimeoutSeconds);
            }

            rebindingOperation.Start();
            return rebindingOperation;

            void onCancel(RebindingOperation op)
            {
                if (actionWasEnabled) action.Enable();
                callback?.Invoke(new RebindInfo(actionWrapper, RebindInfo.Status.Canceled,
                    InputRuntime.Current.GetBindingSlots(actionWrapper, actionBindingInfo.ControlSchemeId)));
                CleanUpRebindingOperation(ref rebindingOperation);
            }

            void onComplete(RebindingOperation op)
            {
                if (actionWasEnabled) action.Enable();

                callback?.Invoke(new RebindInfo(actionWrapper, RebindInfo.Status.Completed,
                    InputRuntime.Current.GetBindingSlots(actionWrapper, actionBindingInfo.ControlSchemeId)));
                CleanUpRebindingOperation(ref rebindingOperation);
                InputRuntime.Current.BroadcastBindingsChanged();
            }
        }

        private static RebindingOperation WithControlsExcludingMultiple(this RebindingOperation rebindingOperation, string[] paths)
        {
            foreach (string excludedPath in paths) rebindingOperation.WithControlsExcluding(excludedPath);

            // Handles excluded keyboard keys coming in as "anyKey" and still completing the binding operation.
            rebindingOperation.WithControlsExcluding("<Keyboard>/anyKey");

            return rebindingOperation;
        }

        private static RebindingOperation WithCancelingThroughMultiple(this RebindingOperation rebindingOperation, string[] paths)
        {
            if (paths.Length == 0)
            {
                return rebindingOperation;
            }

            string primaryCancelPath = paths[0];
            rebindingOperation.WithCancelingThrough(primaryCancelPath);

            // Unity's rebinding operation extension method "WithCancelingThrough" to choose a control path
            // that cancels an interactive rebind only supports ONE control path at a time (strange oversight).
            // The below is a workaround to support multiple control paths if required.
            if (paths.Length > 1)
            {
                // >>> NOTE: OnPotentialMatch will not read inputs outside of your current control scheme. So if you're
                // rebinding on gamepad and hit Escape to cancel, Escape had better be your primaryCancelPath (above)
                // or else it won't get caught here. TODO: Find a better solution for this, perhaps an AnyButtonPress listener that catches cancel paths.
                rebindingOperation.OnPotentialMatch(operation =>
                {
                    if (paths.Any(path => operation.selectedControl.path == path))
                    {
                        operation.Cancel();
                    }
                });
            }

            return rebindingOperation;
        }

        private static void CleanUpRebindingOperation(ref RebindingOperation rebindingOperation)
        {
            rebindingOperation?.Dispose();
            rebindingOperation = null;
        }

        /// <summary>
        /// Put one slot back to its default: every part of a composite, narrowed to a single part when
        /// the caller names one, or a single plain binding.
        /// </summary>
        internal static void ResetBindingToDefaultForSlot(InputData inputData, ActionBindingInfo actionBindingInfo)
        {
            ChangeSlot(inputData, actionBindingInfo,
                (action, bindingIndex) => action.RemoveBindingOverride(bindingIndex),
                binding => binding.overridePath != null);
        }

        /// <summary>
        /// Empty one slot, so the action has nothing bound there until it is rebound or reset. An empty
        /// override path is how the input system disables a binding, and an empty override still counts
        /// as one, so a cleared slot is saved and loaded like any other.
        /// </summary>
        internal static void ClearBindingForSlot(InputData inputData, ActionBindingInfo actionBindingInfo)
        {
            ChangeSlot(inputData, actionBindingInfo,
                (action, bindingIndex) => action.ApplyBindingOverride(bindingIndex, string.Empty),
                binding => binding.overridePath != string.Empty);
        }

        /// <summary>
        /// Apply a change to one slot's bindings: every part of a composite, narrowed to a single part
        /// when the caller names one, or a single plain binding. Only the bindings the change would
        /// actually alter are touched, so nothing is broadcast when there was nothing to do.
        /// </summary>
        private static void ChangeSlot(InputData inputData, ActionBindingInfo actionBindingInfo,
            Action<InputAction, int> change, Func<InputBinding, bool> wouldChange)
        {
            InputAction action = actionBindingInfo.ActionWrapper.InputAction;
            bool changed = false;

            using (BindingSlots bindingSlots = BindingSlots.Resolve(inputData, action, actionBindingInfo.ControlSchemeId))
            {
                if (!bindingSlots.TryGetAtUIIndex(actionBindingInfo.UIIndex, out BindingSlot bindingSlot))
                {
                    return;
                }

                for (int i = bindingSlot.BindingIndex; i < bindingSlot.BindingIndex + bindingSlot.BindingCount; i++)
                {
                    InputBinding binding = action.bindings[i];
                    if (binding.isComposite || !wouldChange(binding) ||
                        (actionBindingInfo.UseCompositePart && binding.isPartOfComposite && !actionBindingInfo.CompositePart.Matches(binding)))
                    {
                        continue;
                    }

                    changed = true;
                    change(action, i);
                }
            }

            if (changed)
            {
                InputRuntime.Current.BroadcastBindingsChanged();
            }
        }

        internal static void ResetBindingToDefaultForControlScheme(ActionBindingInfo actionBindingInfo, ControlSchemeId controlSchemeId)
        {
            bool compositeCondition(InputBinding binding) => actionBindingInfo.DontUseCompositePart || actionBindingInfo.CompositePart.Matches(binding);
            if (RemoveDeviceOverridesFromAction(actionBindingInfo.ActionWrapper.InputAction, controlSchemeId.ToBindingMask(), compositeCondition))
            {
                InputRuntime.Current.BroadcastBindingsChanged();
            }
        }

        internal static void ResetBindingsToDefaultForControlScheme(InputActionAsset asset, ControlSchemeId controlSchemeId)
        {
            bool changed = false;
            foreach (InputAction action in asset)
            {
                changed |= RemoveDeviceOverridesFromAction(action, controlSchemeId.ToBindingMask());
            }

            if (changed)
            {
                InputRuntime.Current.BroadcastBindingsChanged();
            }
        }

        internal static void ResetBindingsToDefault(InputActionAsset asset)
        {
            bool changed = asset.Any(HasOverride);
            asset.RemoveAllBindingOverrides();

            if (changed)
            {
                InputRuntime.Current.BroadcastBindingsChanged();
            }
        }

        private static bool HasOverride(InputAction action)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                // An empty override is a deliberately cleared binding, not the absence of one.
                string overridePath = action.bindings[i].overridePath;
                if (overridePath != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Return true only if a binding was changed, ie, it actually had an override that was removed/returned to default.
        /// </summary>
        private static bool RemoveDeviceOverridesFromAction(InputAction action, InputBinding bindingMask, Func<InputBinding, bool> additionalRemoveCondition = null)
        {
            bool changed = false;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                string overridePath = binding.overridePath;
                if (bindingMask.Matches(binding) && overridePath != null &&
                    (additionalRemoveCondition == null || additionalRemoveCondition.Invoke(binding)))
                {
                    changed = true;
                    action.RemoveBindingOverride(i);
                }
            }

            return changed;
        }
    }
}
