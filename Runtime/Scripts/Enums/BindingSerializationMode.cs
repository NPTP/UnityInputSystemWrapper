using System;

namespace NPTP.InputSystemWrapper.Enums
{
    /// <summary>
    /// Where a player's saved bindings are written to and read back from. Any combination is valid,
    /// including none at all, which means bindings are never persisted.
    /// </summary>
    [Flags]
    public enum BindingSerializationMode
    {
        None = 0,

        /// <summary>A JSON file per player under the application's persistent data path.</summary>
        File = 1 << 0,

        /// <summary>
        /// The project's own storage, reached through the binding serialization events.
        /// </summary>
        Event = 1 << 1
    }

    internal static class BindingSerializationModeExtensions
    {
        internal static bool UsesFile(this BindingSerializationMode mode) => (mode & BindingSerializationMode.File) != 0;

        internal static bool UsesEvent(this BindingSerializationMode mode) => (mode & BindingSerializationMode.Event) != 0;
    }
}
