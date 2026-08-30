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
        /// Put one slot back to its default: every part of a composite, or a single plain binding. A
        /// composite narrows to one part when the caller named one, so this undoes exactly what the
        /// matching rebind would have changed.
        /// </summary>
        internal static void ResetBindingToDefaultForSlot(InputData inputData, ActionBindingInfo actionBindingInfo)
        {
            InputAction action = actionBindingInfo.ActionWrapper.InputAction;
            BindingSlots bindingSlots = BindingSlots.Resolve(inputData, action, actionBindingInfo.ControlSchemeId);

            if (!bindingSlots.TryGetAtUIIndex(actionBindingInfo.UIIndex, out BindingSlot bindingSlot))
            {
                return;
            }

            bool changed = false;
            for (int i = bindingSlot.BindingIndex; i < bindingSlot.BindingIndex + bindingSlot.BindingCount; i++)
            {
                InputBinding binding = action.bindings[i];
                if (binding.isComposite || string.IsNullOrEmpty(binding.overridePath) ||
                    (actionBindingInfo.UseCompositePart && binding.isPartOfComposite && !actionBindingInfo.CompositePart.Matches(binding)))
                {
                    continue;
                }

                changed = true;
                action.RemoveBindingOverride(i);
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
                string overridePath = action.bindings[i].overridePath;
                if (!string.IsNullOrEmpty(overridePath))
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
                if (bindingMask.Matches(binding) && !string.IsNullOrEmpty(overridePath) &&
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
