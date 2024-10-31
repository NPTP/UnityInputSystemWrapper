using System;
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

        internal static RebindingOperation StartInteractiveRebind(InputAction action, int bindingIndex, Action callback)
        {
            bool actionWasEnabled = action.enabled;
            action.Disable();

            RebindingOperation rebindingOperation = action.PerformInteractiveRebinding(bindingIndex);

            // Note that pointer movement (including touch) is already excluded in the above call to PerformInteractiveRebinding. 
            rebindingOperation.WithControlsExcludingMultiple(ExcludedPaths);
            rebindingOperation.WithCancelingThroughMultiple(CancelPaths);

            rebindingOperation
                .OnCancel(onCancel)
                .OnComplete(onComplete);
            
            rebindingOperation.Start();
            return rebindingOperation;
            
            void onCancel(RebindingOperation op)
            {
                if (actionWasEnabled) action.Enable();
                callback?.Invoke();
                CleanUpRebindingOperation(ref rebindingOperation);
            }

            void onComplete(RebindingOperation op)
            {
                if (actionWasEnabled) action.Enable();
                callback?.Invoke();
                CleanUpRebindingOperation(ref rebindingOperation);
                Input.BroadcastBindingsChanged();
            }
        }

        private static void WithControlsExcludingMultiple(this RebindingOperation rebindingOperation, string[] paths)
        {
            foreach (string excludedPath in paths) rebindingOperation.WithControlsExcluding(excludedPath);
            
            // Handles excluded keyboard keys coming in as "anyKey" and still completing the binding operation.
            rebindingOperation.WithControlsExcluding("<Keyboard>/anyKey");
        }
        
        private static void WithCancelingThroughMultiple(this RebindingOperation rebindingOperation, string[] paths)
        {
            rebindingOperation.WithCancelingThrough(string.Empty);
            
            // Unity's rebinding operation extension method "WithCancelingThrough" to choose a control path
            // that cancels an interactive rebind only supports ONE control path at a time.
            // This is a workaround to support multiple control paths.
            rebindingOperation.OnPotentialMatch(op =>
            {
                if (paths.Any(path => op.selectedControl.path == path))
                {
                    op.Cancel();
                }
            });
        }

        private static void CleanUpRebindingOperation(ref RebindingOperation rebindingOperation)
        {
            rebindingOperation?.Dispose();
            rebindingOperation = null;
        }

        internal static void ResetBindingToDefaultForControlScheme(InputPlayer player, ActionInfo actionInfo, ControlScheme controlScheme)
        {
            if (!player.TryGetMapAndActionInPlayerAsset(actionInfo.InputAction, out InputActionMap _, out InputAction action))
            {
                return;
            }
            
            bool compositeCondition(InputBinding binding) => !actionInfo.UseCompositePart || actionInfo.CompositePart.Matches(binding);
            if (RemoveDeviceOverridesFromAction(action, controlScheme.ToBindingMask(), compositeCondition))
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
        
        private static void ResetControlSchemeToDefaultBindings(InputActionAsset asset, ControlScheme controlScheme)
        {
            foreach (InputAction action in asset)
            {
                action.RemoveBindingOverride(InputBinding.MaskByGroup(controlScheme.ToInputAssetName()));
            }
        }
    }
}
