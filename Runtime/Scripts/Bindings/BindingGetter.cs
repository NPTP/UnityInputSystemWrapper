using System.Collections.Generic;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingGetter
    {
        internal static bool TryGetBindingInfo(RuntimeInputData runtimeInputData, InputPlayer player, ActionInfo actionInfo, ControlScheme controlScheme, out IEnumerable<BindingInfo> bindingInfos)
        {
            bindingInfos = default;

            // Get the correct action from the player asset based on the action reference wrapper's internal reference.
            if (!player.TryGetMapAndActionInPlayerAsset(actionInfo.InputAction, out InputActionMap _, out InputAction action))
            {
                return false;
            }
            
            // Get the string control paths for the used input action & composite part.
            if (!TryGetControlPaths(actionInfo, action, controlScheme, out List<string> controlPaths))
            {
                return false;
            }
            
            // Get the asset on disk containing binding data.
            if (!TryGetBindingData(runtimeInputData, controlScheme, out BindingData bindingData))
            {
                return false;
            }

            // Get the binding info (name, sprite, etc) for each control path.
            if (!TryGetAllBindingInfo(controlPaths, bindingData, out List<BindingInfo> bindingInfoList))
            {
                return false;
            }

            bindingInfos = bindingInfoList;
            return true;
        }

        private static bool TryGetActionInAssetWithMatchingName(InputActionAsset asset, string actionName, out InputAction actionFromAsset)
        {
            actionFromAsset = asset.FindAction(actionName);
            return actionFromAsset != null;
        }

        private static bool TryGetAllBindingInfo(List<string> controlPaths, BindingData bindingData, out List<BindingInfo> bindingInfoList)
        {
            bindingInfoList = new List<BindingInfo>();
            foreach (string controlPath in controlPaths)
            {
                if (bindingData.TryGetBindingInfo(controlPath, out BindingInfo bindingInfo))
                    bindingInfoList.Add(bindingInfo);
            }

            return bindingInfoList.Count > 0;
        }

        private static bool TryGetBindingData(RuntimeInputData runtimeInputData, ControlScheme controlScheme, out BindingData bindingData)
        {
            bindingData = runtimeInputData.GetControlSchemeBindingData(controlScheme);
            bool bindingDataNull = bindingData == null;
            if (bindingDataNull)
                NPTPDebug.LogWarning($"Control scheme {controlScheme} is not supported by any {nameof(BindingData)} and cannot produce display names/sprites!");
            
            return !bindingDataNull;
        }
        
        private static bool TryGetControlPaths(ActionInfo actionInfo, InputAction action, ControlScheme controlScheme, out List<string> controlPaths)
        {
            List<string> paths = new();
            InputBinding bindingMask = controlScheme.ToBindingMask();
                
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                if (bindingMask.Matches(binding) && (!actionInfo.UseCompositePart || actionInfo.CompositePart.Matches(binding)))
                {
                    string effectivePath = binding.effectivePath;
                    paths.Add(effectivePath.Remove(0, effectivePath.IndexOf('>') + 2));
                }
            }

            controlPaths = paths;
            return controlPaths.Count > 0;
        }
        
        internal static bool TryGetFirstBindingIndex(ActionReference actionReference, InputAction action, ControlScheme controlScheme, out int firstBindingIndex)
        {
            firstBindingIndex = -1;

            InputBinding bindingMask = controlScheme.ToBindingMask();
            
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                if (bindingMask.Matches(binding) && (!actionReference.UseCompositePart || actionReference.CompositePart.Matches(binding)))
                {
                    firstBindingIndex = i;
                    break;
                }
            }

            return firstBindingIndex != -1;
        }
    }
}
