using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Utilities;

namespace NPTP.InputSystemWrapper.Text
{
    /// <summary>
    /// Turns the glyph elements of a string into the bindings they name. The binding data loads in the
    /// background, so the callback runs once every glyph in the string is ready.
    /// </summary>
    public static class InlineGlyphResolver
    {
        /// <summary>
        /// Resolve every "&lt;isw ...&gt;" element in a string. The result carries the assets the glyphs
        /// were read from, so dispose it when the text goes away.
        /// </summary>
        public static void ResolveAsync(string text, Action<InlineGlyphResolutions> onResolved)
        {
            List<InlineGlyphTag> tags = InlineGlyphTagParser.Parse(text);
            if (tags.Count == 0)
            {
                onResolved?.Invoke(InlineGlyphResolutions.Empty(text));
                return;
            }

            // Sized up front so each glyph can be written at its own place as it arrives, whatever order
            // the loads finish in, and the results stay in the order the glyphs appear in the text.
            InlineGlyphResolution[] resolved = new InlineGlyphResolution[tags.Count];
            List<BindingSlots> held = new();
            int remaining = tags.Count;

            for (int i = 0; i < tags.Count; i++)
            {
                InlineGlyphTag tag = tags[i];
                int tagIndex = i;

                if (!InputRuntime.Current.TryGetActionWrapperByName(tag.PlayerID, tag.ActionMapName, tag.ActionName,
                        out ActionWrapper actionWrapper))
                {
                    ISWDebug.LogWarning($"Inline glyph {tag.ToString()} names no action player {tag.PlayerID.ToString()} has, " +
                                        "so the action's name is shown instead.");
                    resolved[tagIndex] = new InlineGlyphResolution(tag, null);
                    if (--remaining == 0) Finish();
                    continue;
                }

                actionWrapper.GetCurrentBindingSlotsAsync(bindingSlots =>
                {
                    held.Add(bindingSlots);
                    resolved[tagIndex] = new InlineGlyphResolution(tag, FindBindingInfo(bindingSlots, tag));
                    if (--remaining == 0) Finish();
                });
            }

            void Finish()
            {
                onResolved?.Invoke(new InlineGlyphResolutions(text, new List<InlineGlyphResolution>(resolved), held));
            }
        }

        private static BindingInfo FindBindingInfo(BindingSlots bindingSlots, InlineGlyphTag tag)
        {
            return bindingSlots.TryGetAtUIIndex(tag.UIIndex, out BindingSlot bindingSlot) &&
                   bindingSlot.TryGetBindingInfo(tag.CompositePart, out BindingInfo bindingInfo)
                ? bindingInfo
                : null;
        }
    }
}
