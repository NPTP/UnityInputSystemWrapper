using System.Collections.Generic;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingGetter
    {
        internal static bool TryGetBindingInfo(RuntimeInputData runtimeInputData, ActionBindingInfo actionBindingInfo, out IEnumerable<BindingInfo> bindingInfos)
        {
            bindingInfos = default;
            
            // Get the string control paths for the used input action & composite part.
            if (!TryGetControlPaths(actionBindingInfo, actionBindingInfo.ControlScheme, out List<string> controlPaths))
            {
                return false;
            }
            
            // Get the asset on disk containing binding data.
            if (!TryGetBindingData(runtimeInputData, actionBindingInfo.ControlScheme, out BindingData bindingData))
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
                ISWDebug.LogWarning($"Control scheme {controlScheme} is not supported by any {nameof(BindingData)} and cannot produce display names/sprites!");
            
            return !bindingDataNull;
        }
        
        private static bool TryGetControlPaths(ActionBindingInfo actionBindingInfo, ControlScheme controlScheme, out List<string> controlPaths)
        {
            List<string> paths = new();
            InputBinding bindingMask = controlScheme.ToBindingMask();
                
            for (int i = 0; i < actionBindingInfo.ActionWrapper.InputAction.bindings.Count; i++)
            {
                InputBinding binding = actionBindingInfo.ActionWrapper.InputAction.bindings[i];
                if (bindingMask.Matches(binding) && (actionBindingInfo.DontUseCompositePart || actionBindingInfo.CompositePart.Matches(binding)))
                {
                    string effectivePath = binding.effectivePath;
                    paths.Add(effectivePath.Remove(0, effectivePath.IndexOf('>') + 2));
                }
            }

            controlPaths = paths;
            return controlPaths.Count > 0;
        }
        
        internal static bool TryGetFirstBindingIndex(ActionBindingInfo actionBindingInfo, out int firstBindingIndex)
        {
            firstBindingIndex = -1;

            InputBinding bindingMask = actionBindingInfo.ControlScheme.ToBindingMask();
            
            for (int i = 0; i < actionBindingInfo.ActionWrapper.InputAction.bindings.Count; i++)
            {
                InputBinding binding = actionBindingInfo.ActionWrapper.InputAction.bindings[i];
                if (bindingMask.Matches(binding) && (actionBindingInfo.DontUseCompositePart || actionBindingInfo.CompositePart.Matches(binding)))
                {
                    firstBindingIndex = i;
                    break;
                }
            }

            return firstBindingIndex != -1;
        }
    }
}
