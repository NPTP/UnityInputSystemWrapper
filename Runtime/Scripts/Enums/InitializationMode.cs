namespace NPTP.InputSystemWrapper.Enums
{
    internal enum InitializationMode
    {
        /// <summary>
        /// Initialize the system via RuntimeInitializeOnLoadMethod in BeforeSceneLoad.
        /// </summary>
        BeforeSceneLoad = 0,

        /// <summary>
        /// Requires manually calling ISW.Initialize from user code, so you can manage timing/performance yourself.
        /// </summary>
        Manual
    }
}