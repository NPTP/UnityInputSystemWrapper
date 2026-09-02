#if ISW_TEXTMESHPRO
using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Text;
using TMPro;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Components
{
    /// <summary>
    /// Writes bindings into a line of TextMeshPro text wherever it names one, as
    /// &lt;isw type="sprite" player=1 action="Gameplay.Fire" composite="positive" index=2&gt;. Only the
    /// action is required, and an action name alone is enough unless two maps share it, in which case write
    /// "Map.Action". The line is written once every binding in it has loaded.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class InputGlyphText : InputDisplayBehaviour<InlineGlyphResolutions>
    {
        [Tooltip("The line to show, with an <isw ...> element wherever a binding should appear.")]
        [TextArea(2, 6)]
        [SerializeField] private string sourceText;

        /// <summary>
        /// The line to show. Setting it writes the bindings into the new line, so text that changes as a
        /// screen is used keeps its glyphs.
        /// </summary>
        public string SourceText
        {
            get => sourceText;
            set
            {
                if (sourceText == value)
                {
                    return;
                }

                sourceText = value;

                // Enabling loads anyway, so a change while disabled needs nothing more.
                if (isActiveAndEnabled)
                {
                    Refresh();
                }
            }
        }

        private TMP_Text text;

        /// <summary>The sprite assets the sprite tags in the written line point at.</summary>
        private RuntimeSpriteAssets runtimeSpriteAssets;

        /// <summary>Which sprite tag name each glyph was written as, by where the glyph sat in the line.</summary>
        private readonly Dictionary<int, string> spriteNamesByTagStart = new();

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        protected override void Load(Action<InlineGlyphResolutions> onLoaded) =>
            InlineGlyphResolver.ResolveAsync(sourceText, onLoaded);

        protected override void Display(InlineGlyphResolutions resolved)
        {
            BuildSpriteAssets(resolved);

            // In place before the line naming its sprites, so the first draw of that line can find them.
            text.spriteAsset = runtimeSpriteAssets?.Primary;
            text.text = resolved.BuildText(FormatSprite);
        }

        protected override void OnReleased()
        {
            runtimeSpriteAssets?.Dispose();
            runtimeSpriteAssets = null;
            spriteNamesByTagStart.Clear();
        }

        /// <summary>
        /// Gather the sprites the line asks for into sprite assets to draw from, and remember the name each
        /// glyph is written as.
        /// </summary>
        private void BuildSpriteAssets(InlineGlyphResolutions resolved)
        {
            // Rebuilt on every display, so the previous ones are given back first.
            OnReleased();

            List<Sprite> sprites = new();
            List<int> tagStarts = new();

            foreach (InlineGlyphResolution resolution in resolved)
            {
                if (resolution.Tag.GlyphType != InlineGlyphType.Sprite || !resolution.Resolved ||
                    resolution.BindingInfo.Sprite == null)
                {
                    continue;
                }

                sprites.Add(resolution.BindingInfo.Sprite);
                tagStarts.Add(resolution.Tag.StartIndex);
            }

            if (sprites.Count == 0)
            {
                return;
            }

            runtimeSpriteAssets = RuntimeSpriteAssets.Build(sprites, out string[] spriteNames);
            for (int i = 0; i < tagStarts.Count; i++)
            {
                if (!string.IsNullOrEmpty(spriteNames[i]))
                {
                    spriteNamesByTagStart.Add(tagStarts[i], spriteNames[i]);
                }
            }
        }

        /// <summary>
        /// The sprite tag for a glyph, or nothing when it has no sprite, leaving its display name.
        /// </summary>
        private string FormatSprite(InlineGlyphResolution resolution)
        {
            return spriteNamesByTagStart.TryGetValue(resolution.Tag.StartIndex, out string spriteName)
                ? $"<sprite name=\"{spriteName}\">"
                : string.Empty;
        }
    }
}
#endif
