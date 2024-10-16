namespace NPTP.InputSystemWrapper.Enums
{
    /// <summary>
    /// Support additional devices explicitly as needed (avoids the need to use control schemes).
    /// Wherever the actual values (SupportedDevice.MouseKeyboard, SupportedDevice.Xbox, etc) are used
    /// must be updated manually.
    /// This is the only adjustable part of this input wrapper package that must be updated manually, since it
    /// represents the actual support provided and tested in the package.
    /// </summary>
    public enum SupportedDevice
    {
        MouseKeyboard = 0,
        Xbox,
        PlayStation,
        Gamepad
    }
}