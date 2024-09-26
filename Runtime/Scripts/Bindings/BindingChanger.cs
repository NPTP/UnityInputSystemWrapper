using System;
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

                    if (RemoveDuplicateBindings(action, bindingIndex, allCompositeParts))
                    {
                        action.RemoveBindingOverride(bindingIndex);
                        CleanUpRebindOperation(ref rebindingOperation);
                        PerformInteractiveRebind(action, bindingIndex, allCompositeParts);
                        return;
                    }

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

        private static bool RemoveDuplicateBindings(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            InputBinding newBinding = action.bindings[bindingIndex];

            foreach (InputAction otherAction in action.actionMap.asset)
            {
                if (action == otherAction)
                {
                    continue;
                }

                for (int i = 0; i < otherAction.bindings.Count; i++)
                {
                    if (newBinding.effectivePath == otherAction.bindings[i].effectivePath)
                    {
                        otherAction.RemoveBindingOverride(i);
                    }
                }
            }

            // TODO
            // if (allCompositeParts)
            // {
            // for (int i = 1; i < bindingIndex; i++)
            // {
            // if (action.bindings[i].effectivePath == newBinding.effectivePath)
            // return true;
            // }
            // }

            return false;
        }

        /// <summary>
        /// Remove currently applied binding overrides.
        /// </summary>
        public static void ResetToDefault()
        {
            // if (!ResolveActionAndBinding(out InputAction action, out int bindingIndex))
                // return;
            
            // ResetBinding(action, bindingIndex);

            // if (action.bindings[bindingIndex].isComposite)
            // {
            //     // It's a composite. Remove overrides from part bindings.
            //     for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
            //         action.RemoveBindingOverride(i);
            // }
            // else
            // {
            //     action.RemoveBindingOverride(bindingIndex);
            // }

            BroadcastBindingOperationEnded();
        }

        private static void ResetControlSchemeToDefaultBindings(InputActionAsset asset, ControlScheme controlScheme)
        {
            foreach (InputAction action in asset)
            {
                action.RemoveBindingOverride(InputBinding.MaskByGroup(controlScheme.ToNameInInputActionAsset()));
            }
        }

        private static void ResetBinding(InputAction action, int bindingIndex)
        {
            InputBinding newBinding = action.bindings[bindingIndex];

            action.RemoveBindingOverride(bindingIndex);

            foreach (InputAction otherAction in action.actionMap.asset)
            {
                if (otherAction == action)
                    continue;

                for (int i = 0; i < otherAction.bindings.Count; i++)
                {
                    InputBinding binding = otherAction.bindings[i];
                    if (binding.overridePath == newBinding.path)
                    {
                        otherAction.RemoveBindingOverride(i);
                    }
                }
            }
        }

        private static void BroadcastBindingOperationEnded()
        {
            OnBindingOperationEnded?.Invoke();
        }
    }
}