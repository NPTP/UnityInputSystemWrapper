using NPTP.InputSystemWrapper.Actions;

// ReSharper disable once CheckNamespace
namespace NPTP.InputSystemWrapper.Player
{
    public sealed partial class InputPlayer
    {
        private void CreateActionMapWrappers()
        {
            // MARKER.ActionsInstantiation.Start
            actionMapWrappers.Add("Player", new PlayerActions(ID, Asset, actionWrapperTable));
            actionMapWrappers.Add("UI", new UIActions(ID, Asset, actionWrapperTable));
            // MARKER.ActionsInstantiation.End
        }
    }
}
