using System;
using System.Linq;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;
using RebindingOperation = UnityEngine.InputSystem.InputActionRebindingExtensions.RebindingOperation;

namespace NPTP.InputSystemWrapper.Bindings
{
    // TODO: localization support
    internal static class BindingChanger
    {
        private const string MOUSE = "<Mouse>";
        private const string KEYBOARD_ESCAPE = "<Keyboard>/escape";

        public static event Action OnBindingOperationEnded;

        public static RebindingOperation StartInteractiveRebind(InputAction action, int bindingIndex)
        {
            // If the binding is a composite, we need to rebind each part in turn.
            int firstPartIndex = bindingIndex + 1;
            bool allCompositeParts = action.bindings[bindingIndex].isComposite &&
                                     firstPartIndex < action.bindings.Count &&
                                     action.bindings[firstPartIndex].isPartOfComposite;

            return PerformInteractiveRebind(action, bindingIndex, allCompositeParts);
        }

        private static RebindingOperation PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            bool actionWasEnabled = action.enabled;
            action.Disable();

            RebindingOperation rebindingOperation = action.PerformInteractiveRebinding(bindingIndex);
            
            rebindingOperation
                // .WithControlsExcluding(MOUSE) // TODO: Put this back in if mouse rebinds with its own movement
                .WithCancelingThrough(KEYBOARD_ESCAPE)
                .OnCancel(operation =>
                {
                    if (actionWasEnabled) action.Enable();
                    BroadcastBindingOperationEnded();
                    CleanUpRebindOperation(ref rebindingOperation);
                })
                .OnComplete(operation =>
                {
                    if (actionWasEnabled) action.Enable();
                    BroadcastBindingOperationEnded();
                    CleanUpRebindOperation(ref rebindingOperation);

                    // If there's more composite parts we should bind, initiate a rebind for the next part.
                    if (allCompositeParts)
                    {
                        int nextBindingIndex = bindingIndex + 1;
                        if (nextBindingIndex < action.bindings.Count &&
                            action.bindings[nextBindingIndex].isPartOfComposite)
                            PerformInteractiveRebind(action, nextBindingIndex, true);
                    }

                    BroadcastBindingOperationEnded();
                });

            rebindingOperation.Start();
            return rebindingOperation;
        }

        private static void CleanUpRebindOperation(ref RebindingOperation rebindingOperation)
        {
            rebindingOperation?.Dispose();
            rebindingOperation = null;
        }

        public static void ResetBindingToDefaultForDevice(InputAction action, SupportedDevice device)
        {
            string[] devicePathStrings = BindingDeviceHelper.GetDevicePathStrings(device);
            RemoveOverridesFromAction(action, devicePathStrings);
            BroadcastBindingOperationEnded();
        }

        public static void ResetBindingsToDefaultForDevice(InputActionAsset asset, SupportedDevice device)
        {
            string[] devicePathStrings = BindingDeviceHelper.GetDevicePathStrings(device);
            foreach (InputAction action in asset)
            {
                RemoveOverridesFromAction(action, devicePathStrings);
            }
            BroadcastBindingOperationEnded();
        }

        private static void RemoveOverridesFromAction(InputAction action, string[] devices)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                string overridePath = action.bindings[i].overridePath;
                if (!string.IsNullOrEmpty(overridePath) && devices.Any(deviceString => overridePath.Contains(deviceString)))
                {
                    action.RemoveBindingOverride(i);
                }
            }
        }
        
        private static void ResetControlSchemeToDefaultBindings(InputActionAsset asset, ControlScheme controlScheme)
        {
            foreach (InputAction action in asset)
            {
                action.RemoveBindingOverride(InputBinding.MaskByGroup(controlScheme.ToInputAssetName()));
            }
        }
        
        private static void BroadcastBindingOperationEnded()
        {
            OnBindingOperationEnded?.Invoke();
        }
    }
}