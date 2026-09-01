namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// What the binding asset cache is holding, for checking that loads and disposals balance. Reading
    /// these costs nothing and changes nothing, so a profiling pass can sample them freely.
    /// </summary>
    public static class BindingDiagnostics
    {
        /// <summary>How many binding assets are loaded right now.</summary>
        public static int LoadedAssetCount => BindingDataCache.LoadedAssetCount;

        /// <summary>
        /// How many takes are outstanding across all callers. Zero means everything has been given back.
        /// </summary>
        public static int OutstandingReferenceCount => BindingDataCache.OutstandingReferenceCount;

        public static string Describe() =>
            $"{LoadedAssetCount} binding asset(s) loaded, {OutstandingReferenceCount} reference(s) outstanding";
    }
}
