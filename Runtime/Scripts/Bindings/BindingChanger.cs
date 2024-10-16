using System;
using System.Linq;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;
using RebindingOperation = UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingChanger
    {
        private const string MOUSE = "<Mouse>";
        private const string KEYBOARD_ESCAPE = "<Keyboard>/escape";

        internal static RebindingOperation StartInteractiveRebind(InputAction action, int bindingIndex, Action callback)
        {
            // If the binding is a composite, we need to rebind each part in turn.
            int firstPartIndex = bindingIndex + 1;
            bool allCompositeParts = action.bindings[bindingIndex].isComposite &&
                                     firstPartIndex < action.bindings.Count &&
                                     action.bindings[firstPartIndex].isPartOfComposite;

            return PerformInteractiveRebind(action, bindingIndex, allCompositeParts, callback);
        }

        private static RebindingOperation PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts, Action callback)
        {
            bool actionWasEnabled = action.enabled;
            action.Disable();

            RebindingOperation rebindingOperation = action.PerformInteractiveRebinding(bindingIndex);
            
            rebindingOperation
                // TODO (bindings): Put this mouse exclusion back in if mouse rebinds with its own movement, but exclude only the movement
                // .WithControlsExcluding(MOUSE)
                .WithCancelingThrough(KEYBOARD_ESCAPE)
                .OnCancel(_ =>
                {
                    if (actionWasEnabled) action.Enable();
                    callback?.Invoke();
                    CleanUpRebindOperation(ref rebindingOperation);
                })
                .OnComplete(_ =>
                {
                    if (actionWasEnabled) action.Enable();
                    CleanUpRebindOperation(ref rebindingOperation);

                    // If there's more composite parts we should bind, initiate a rebind for the next part.
                    if (allCompositeParts)
                    {
                        int nextBindingIndex = bindingIndex + 1;
                        if (nextBindingIndex < action.bindings.Count &&
                            action.bindings[nextBindingIndex].isPartOfComposite)
                            PerformInteractiveRebind(action, nextBindingIndex, true, null);
                    }

                    callback?.Invoke();
                    Input.BroadcastBindingsChanged();
                });

            rebindingOperation.Start();
            return rebindingOperation;
        }

        private static void CleanUpRebindOperation(ref RebindingOperation rebindingOperation)
        {
            rebindingOperation?.Dispose();
            rebindingOperation = null;
        }

        internal static void ResetBindingToDefaultForDevice(InputAction action, SupportedDevice device)
        {
            string[] devicePathStrings = BindingDeviceHelper.GetDevicePathStrings(device);
            if (RemoveDeviceOverridesFromAction(action, devicePathStrings))
            {
                Input.BroadcastBindingsChanged();
            }
        }

        internal static void ResetBindingsToDefaultForDevice(InputActionAsset asset, SupportedDevice device)
        {
            string[] devicePathStrings = BindingDeviceHelper.GetDevicePathStrings(device);
            bool changed = false;
            foreach (InputAction action in asset)
            {
                changed |= RemoveDeviceOverridesFromAction(action, devicePathStrings);
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
        /// Return true if a binding was changed, ie, it actually had an override that was removed/returned to default.
        /// </summary>
        private static bool RemoveDeviceOverridesFromAction(InputAction action, string[] devices)
        {
            bool changed = false;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                string overridePath = action.bindings[i].overridePath;
                if (!string.IsNullOrEmpty(overridePath) && devices.Any(deviceString => overridePath.Contains(deviceString)))
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