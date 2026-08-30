using NPTP.InputSystemWrapper.Actions;

// ReSharper disable once CheckNamespace
namespace NPTP.InputSystemWrapper.Player
{
    public sealed partial class InputPlayer
    {
        private void CreateActionMapWrappers()
        {
            // MARKER.ActionsInstantiation.Start
            Player = new PlayerActions(ID, Asset, actionWrapperTable);
            actionMapWrappers.Add("Player", Player);
            UI = new UIActions(ID, Asset, actionWrapperTable);
            actionMapWrappers.Add("UI", UI);
            // MARKER.ActionsInstantiation.End
        }
    }
}
