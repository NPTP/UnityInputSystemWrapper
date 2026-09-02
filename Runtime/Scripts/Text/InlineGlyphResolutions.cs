using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NPTP.InputSystemWrapper.Bindings;

namespace NPTP.InputSystemWrapper.Text
{
    /// <summary>
    /// Every glyph element of one string and the binding each names, in the order they appear. Holds the
    /// binding data they were read from, so dispose it when the text showing them goes away.
    /// </summary>
    public sealed class InlineGlyphResolutions : IReadOnlyList<InlineGlyphResolution>, IDisposable
    {
        private readonly List<InlineGlyphResolution> resolutions;

        /// <summary>The slots each glyph was read from, held so their assets can be given back.</summary>
        private readonly List<BindingSlots> held;

        private bool disposed;

        /// <summary>The string the glyphs were read from.</summary>
        public string SourceText { get; }

        public int Count => resolutions.Count;

        public InlineGlyphResolution this[int index] => resolutions[index];

        internal static InlineGlyphResolutions Empty(string sourceText) =>
            new(sourceText, new List<InlineGlyphResolution>(), new List<BindingSlots>());

        internal InlineGlyphResolutions(string sourceText, List<InlineGlyphResolution> resolutions, List<BindingSlots> held)
        {
            SourceText = sourceText;
            this.resolutions = resolutions;
            this.held = held;
        }

        /// <summary>Gives back the binding data the glyphs were read from. Safe to call more than once.</summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            foreach (BindingSlots bindingSlots in held)
            {
                bindingSlots.Dispose();
            }

            held.Clear();
        }

        /// <summary>
        /// The source string with each glyph element replaced. A sprite goes to the formatter, which writes
        /// whatever the text component understands; anything else becomes a display name, and a glyph that
        /// resolved to nothing becomes the action's name.
        /// </summary>
        public string BuildText(Func<InlineGlyphResolution, string> spriteFormatter)
        {
            if (resolutions.Count == 0)
            {
                return SourceText;
            }

            StringBuilder builder = new();
            int copiedTo = 0;

            foreach (InlineGlyphResolution resolution in resolutions)
            {
                InlineGlyphTag tag = resolution.Tag;
                builder.Append(SourceText, copiedTo, tag.StartIndex - copiedTo);
                builder.Append(Replacement(resolution, spriteFormatter));
                copiedTo = tag.StartIndex + tag.Length;
            }

            builder.Append(SourceText, copiedTo, SourceText.Length - copiedTo);
            return builder.ToString();
        }

        private static string Replacement(InlineGlyphResolution resolution, Func<InlineGlyphResolution, string> spriteFormatter)
        {
            if (!resolution.Resolved)
            {
                return resolution.Tag.ActionName;
            }

            if (resolution.Tag.GlyphType == InlineGlyphType.Sprite && spriteFormatter != null)
            {
                string formatted = spriteFormatter(resolution);
                if (!string.IsNullOrEmpty(formatted))
                {
                    return formatted;
                }
            }

            return resolution.BindingInfo.DisplayName;
        }

        public IEnumerator<InlineGlyphResolution> GetEnumerator() => resolutions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
