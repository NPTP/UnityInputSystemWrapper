using System;
using System.Linq;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;
using RebindingOperation = UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingChanger
    {
        private const string KEYBOARD_ESCAPE = "<Keyboard>/escape";

        internal static RebindingOperation StartInteractiveRebind(InputAction action, int bindingIndex, Action callback)
        {
            bool actionWasEnabled = action.enabled;
            action.Disable();

            // Note that pointer movement (incl. touch) is already excluded in the internal Unity method from being bound in this operation. 
            RebindingOperation rebindingOperation = action.PerformInteractiveRebinding(bindingIndex);
            
            rebindingOperation
                // TODO (bindings): Allow dev to define excluded controls (.WithControlsExcluding) and cancel controls (.WithCancelingThrough) per device.
                .WithCancelingThrough(KEYBOARD_ESCAPE)
                .OnCancel(_ =>
                {
                    if (actionWasEnabled) action.Enable();
                    callback?.Invoke();
                    CleanUpRebindingOperation(ref rebindingOperation);
                })
                .OnComplete(_ =>
                {
                    if (actionWasEnabled) action.Enable();
                    CleanUpRebindingOperation(ref rebindingOperation);
                    callback?.Invoke();
                    Input.BroadcastBindingsChanged();
                });

            rebindingOperation.Start();
            return rebindingOperation;
        }

        private static void CleanUpRebindingOperation(ref RebindingOperation rebindingOperation)
        {
            rebindingOperation?.Dispose();
            rebindingOperation = null;
        }

        internal static void ResetBindingToDefaultForControlScheme(InputPlayer player, InputActionReferenceWrapper wrapper, ControlScheme controlScheme)
        {
            if (!player.TryGetMapAndActionInPlayerAsset(wrapper.InternalReference, out InputActionMap _, out InputAction action))
            {
                return;
            }
            
            bool compositeCondition(InputBinding binding) => !wrapper.UseCompositePart || wrapper.CompositePart.Matches(binding);
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
        
        // TODO (control schemes)
        private static void ResetControlSchemeToDefaultBindings(InputActionAsset asset, ControlScheme controlScheme)
        {
            foreach (InputAction action in asset)
            {
                action.RemoveBindingOverride(InputBinding.MaskByGroup(controlScheme.ToInputAssetName()));
            }
        }
    }
}