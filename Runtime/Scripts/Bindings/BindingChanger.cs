using System;
using System.Collections.Generic;
using System.Linq;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;
using RebindingOperation = UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation;

namespace NPTP.InputSystemWrapper.Bindings
{
    public static class BindingChanger
    {
        private static string[] ExcludedPaths => new string[]
        {
            // MARKER.BindingExcludedPaths.Start
            // MARKER.BindingExcludedPaths.End
        };
        
        private static string[] CancelPaths => new string[]
        {
            // MARKER.BindingCancelPaths.Start
            "/Keyboard/escape"
            // MARKER.BindingCancelPaths.End
        };

        internal static RebindingOperation StartInteractiveRebind(ActionBindingInfo actionBindingInfo, int bindingIndex, Action<RebindInfo> callback)
        {
            ActionWrapper actionWrapper = actionBindingInfo.ActionWrapper;
            InputAction action = actionWrapper.InputAction;
            bool actionWasEnabled = action.enabled;
            action.Disable();

            RebindingOperation rebindingOperation = action.PerformInteractiveRebinding(bindingIndex);

            rebindingOperation
                // Note that pointer movement (including touch) is already excluded in the above call to PerformInteractiveRebinding. 
                .WithControlsExcludingMultiple(ExcludedPaths)
                .WithCancelingThroughMultiple(CancelPaths)
                .OnCancel(onCancel)
                .OnComplete(onComplete);
            
            rebindingOperation.Start();
            return rebindingOperation;
            
            void onCancel(RebindingOperation op)
            {
                if (actionWasEnabled) action.Enable();
                callback?.Invoke(new RebindInfo(actionWrapper, RebindInfo.Status.Canceled, Array.Empty<BindingInfo>()));
                CleanUpRebindingOperation(ref rebindingOperation);
            }

            void onComplete(RebindingOperation op)
            {
                if (actionWasEnabled) action.Enable();
                
                // TODO <optimization>: Temporary measure to return binding info with completed binding.
                // This can be cleaned up with a more direct route to the bindings given all the information the rebind operation gets!
                IEnumerable<BindingInfo> bindingInfos = Array.Empty<BindingInfo>();
                actionWrapper.TryGetBindingInfo(actionBindingInfo.ControlScheme, actionBindingInfo.CompositePart, out bindingInfos);
                
                callback?.Invoke(new RebindInfo(actionWrapper, RebindInfo.Status.Completed, bindingInfos));
                CleanUpRebindingOperation(ref rebindingOperation);
                Input.BroadcastBindingsChanged();
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
                // or else it won't get caught here. TODO: Find a better solution for this.
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

        internal static void ResetBindingToDefaultForControlScheme(ActionBindingInfo actionBindingInfo, ControlScheme controlScheme)
        {
            bool compositeCondition(InputBinding binding) => actionBindingInfo.DontUseCompositePart || actionBindingInfo.CompositePart.Matches(binding);
            if (RemoveDeviceOverridesFromAction(actionBindingInfo.ActionWrapper.InputAction, controlScheme.ToBindingMask(), compositeCondition))
            {
                Input.BroadcastBindingsChanged();
            }
        }

        internal static void ResetBindingsToDefaultForControlScheme(InputActionAsset asset, ControlScheme controlScheme)
        {
            bool changed = false;
            foreach (InputAction action in asset)
            {
                changed |= RemoveDeviceOverridesFromAction(action, controlScheme.ToBindingMask());
            }

            if (changed)
            {
                Input.BroadcastBindingsChanged();
            }
        }

        internal static void ResetBindingsToDefault(InputActionAsset asset)
        {
            bool changed = asset.Any(HasOverride);
            asset.RemoveAllBindingOverrides();

            if (changed)
            {
                Input.BroadcastBindingsChanged();
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
