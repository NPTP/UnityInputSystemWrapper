namespace NPTP.InputSystemWrapper.Actions
{
    /// <summary>
    /// Implemented by every generated actions class so that a player can enable and disable its action
    /// maps by name, without the package needing to know the generated types.
    /// </summary>
    internal interface IActionMapWrapper
    {
        void EnableAndRegisterCallbacks();
        void DisableAndUnregisterCallbacks();
    }
}
