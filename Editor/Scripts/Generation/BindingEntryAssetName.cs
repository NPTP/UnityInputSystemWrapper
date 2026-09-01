namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Names a binding entry's asset file after its control path. Paths nest with slashes, which a file
    /// name cannot contain, so those become underscores: "leftStick/x" is stored as "leftStick_x".
    /// </summary>
    internal static class BindingEntryAssetName
    {
        internal static string FromControlPath(string controlPath)
        {
            return string.IsNullOrEmpty(controlPath) ? "Unnamed" : controlPath.Replace('/', '_');
        }
    }
}
