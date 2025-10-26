namespace NPTP.InputSystemWrapper.Bindings
{
    internal static partial class BindingChanger
    {
        private static string[] GetExcludedPathsGenerated()
        {
            return new string[]
            {
                // MARKER.BindingExcludedPaths.Start
                // MARKER.BindingExcludedPaths.End
            };
        }

        private static string[] GetCancelPathsGenerated()
        {
            return new string[]
            {
                // MARKER.BindingCancelPaths.Start
                "/Keyboard/escape"
                // MARKER.BindingCancelPaths.End
            };
        }
    }
}