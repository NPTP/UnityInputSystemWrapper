using NPTP.InputSystemWrapper.Bindings;

namespace NPTP.InputSystemWrapper.Text
{
    /// <summary>
    /// One glyph element and the binding it turned out to name. A tag whose player, action or binding
    /// index does not exist resolves to no binding info, and is shown as the action's name instead.
    /// </summary>
    public readonly struct InlineGlyphResolution
    {
        public InlineGlyphTag Tag { get; }

        /// <summary>What to display, or null when nothing matched the tag.</summary>
        public BindingInfo BindingInfo { get; }

        public bool Resolved => BindingInfo != null;

        internal InlineGlyphResolution(InlineGlyphTag tag, BindingInfo bindingInfo)
        {
            Tag = tag;
            BindingInfo = bindingInfo;
        }
    }
}
