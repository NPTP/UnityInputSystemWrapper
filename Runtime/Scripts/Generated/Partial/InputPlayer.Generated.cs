using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Generated.Actions;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace NPTP.InputSystemWrapper.Player
{
    public sealed partial class InputPlayer
    {
        // MARKER.ActionsProperties.Start
        public PlayerActions Player { get; }
        public UIActions UI { get; }
        // MARKER.ActionsProperties.End
        
        internal InputPlayer(RuntimeInputData runtimeInputData, int id, bool isMultiplayer, Transform parent)
        {
            this.runtimeInputData = runtimeInputData;
            Asset = InstantiateNewActions(runtimeInputData.InputActionAsset);
            ID = id;
            
            // MARKER.ActionsInstantiation.Start
            Player = new PlayerActions(ID, Asset, actionWrapperTable);
            actionMapWrappers.Add("Player", Player);
            UI = new UIActions(ID, Asset, actionWrapperTable);
            actionMapWrappers.Add("UI", UI);
            // MARKER.ActionsInstantiation.End
            
            SetUpInputPlayerGameObject(isMultiplayer, parent);
            PopulateEventSystemActionsPool();
            
            // Input context gets set by top ISW class after this instantiation, which sets up maps & event system actions/overrides, so we don't have to handle that here.
        }
    }
}
