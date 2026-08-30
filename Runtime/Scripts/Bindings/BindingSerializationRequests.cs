namespace NPTP.InputSystemWrapper.Bindings
{
    /// <summary>
    /// A player's bindings, serialized and ready to store wherever the project keeps its save data.
    /// </summary>
    public class BindingsSaveRequest
    {
        public int PlayerID { get; }

        /// <summary>
        /// The bindings as JSON. Empty when the player has no overrides, which is worth storing as-is so a
        /// player who resets their bindings does not load their old ones back next session.
        /// </summary>
        public string Json { get; }

        internal BindingsSaveRequest(int playerID, string json)
        {
            PlayerID = playerID;
            Json = json;
        }
    }

    /// <summary>
    /// A request for a player's stored bindings. Handle this by setting <see cref="json"/> to whatever was
    /// last given to a <see cref="BindingsSaveRequest"/> for this player.
    /// </summary>
    public class BindingsLoadRequest
    {
        public int PlayerID { get; }

        /// <summary>
        /// Set this to the stored JSON. Left null or empty, the player is treated as having nothing stored.
        /// </summary>
        public string json;

        internal BindingsLoadRequest(int playerID)
        {
            PlayerID = playerID;
        }
    }
}
