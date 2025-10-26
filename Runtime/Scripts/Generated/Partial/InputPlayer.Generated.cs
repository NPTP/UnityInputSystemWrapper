using System;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Generated.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace NPTP.InputSystemWrapper
{
    public sealed partial class InputPlayer
    {
        // MARKER.ActionsProperties.Start
        // MARKER.ActionsProperties.End
        
        internal InputPlayer(InputActionAsset asset, int id, bool isMultiplayer, Transform parent)
        {
            Asset = InstantiateNewActions(asset);
            ID = id;
            
            // MARKER.ActionsInstantiation.Start
            // MARKER.ActionsInstantiation.End
            
            SetUpInputPlayerGameObject(isMultiplayer, parent);
            PopulateEventSystemActionsPool();
            
            // Input context gets set by top ISW class after this instantiation, which sets up maps & event system actions/overrides, so we don't have to handle that here.
        }
        
        private void SetEventSystemOptions()
        {
            // MARKER.EventSystemOptions.Start
            // MARKER.EventSystemOptions.End
        }
        
        /// <summary>
        /// Adds all default and override event system InputActionReferences to a shared pool to
        /// reduce duplication and lookup time.
        /// </summary>
        private void PopulateEventSystemActionsPool()
        {
            // MARKER.PopulateEventSystemActionsPool.Start
            // MARKER.PopulateEventSystemActionsPool.End
        }
        
        private void DisableAllMapsAndRemoveCallbacks()
        {
            // MARKER.DisableAllMapsAndRemoveCallbacksBody.Start
            // MARKER.DisableAllMapsAndRemoveCallbacksBody.End
        }
        
        private void EnableMapsForContext(InputContext context)
        {
            if (!Enabled)
            {
                return;
            }
            
            SetDefaultEventSystemActions();
            
            switch (context)
            {
                // MARKER.EnableContextSwitchMembers.Start
                // MARKER.EnableContextSwitchMembers.End
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            }
        }
    }
}