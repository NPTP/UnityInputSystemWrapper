#if ISW_TEXTMESHPRO
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Player;
using NPTP.InputSystemWrapper.Text;
using TMPro;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Components
{
    /// <summary>
    /// Writes bindings into a line of text, in place, wherever it names one:
    /// "Press &lt;isw action="Fire"&gt; to shoot" shows the button the player would press.
    /// <para>
    /// The fullest form of the element names everything: &lt;isw type="sprite" player=1
    /// action="Gameplay.Fire" composite="positive" index=2&gt;. Only the action is required, and an action
    /// name on its own is enough unless two action maps share it, in which case write "Map.Action".
    /// </para>
    /// <para>
    /// The bindings load in the background and the line is written once they are all in, so it appears
    /// whole rather than filling in a glyph at a time. Their assets are released when this is disabled.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class InputGlyphText : MonoBehaviour
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

                // Enabling writes the line anyway, so a change while disabled needs nothing more.
                if (isActiveAndEnabled)
                {
                    Refresh();
                }
            }
        }

        private TMP_Text text;

        /// <summary>The bindings on screen now, held so their assets can be given back.</summary>
        private InlineGlyphResolutions resolutions;

        /// <summary>The sprite assets the sprite tags in the written line point at.</summary>
        private RuntimeSpriteAssets runtimeSpriteAssets;

        /// <summary>Which sprite tag name each glyph was written as, by where the glyph sat in the line.</summary>
        private readonly Dictionary<int, string> spriteNamesByTagStart = new();

        /// <summary>
        /// Tells a load that finishes after this was disabled or asked to load again that its result is no
        /// longer wanted, so it is released rather than shown.
        /// </summary>
        private int loadGeneration;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            InputRuntime.Current.OnAnyPlayerInputUserChange += HandleAnyPlayerInputUserChange;
            InputRuntime.Current.OnBindingsChanged += HandleBindingsChanged;
            Refresh();
        }

        private void OnDisable()
        {
            InputRuntime.Current.OnAnyPlayerInputUserChange -= HandleAnyPlayerInputUserChange;
            InputRuntime.Current.OnBindingsChanged -= HandleBindingsChanged;

            loadGeneration++;
            Release();
        }

        /// <summary>Write the line again, e.g. after changing which player a glyph in it names.</summary>
        public void Refresh()
        {
            int generation = ++loadGeneration;
            InlineGlyphResolver.ResolveAsync(sourceText, resolved =>
            {
                if (generation != loadGeneration)
                {
                    resolved.Dispose();
                    return;
                }

                Release();
                resolutions = resolved;
                Display(resolved);
            });
        }

        private void HandleAnyPlayerInputUserChange(InputUserChangeInfo inputUserChangeInfo) => Refresh();
        private void HandleBindingsChanged() => Refresh();

        private void Display(InlineGlyphResolutions resolved)
        {
            BuildSpriteAssets(resolved);

            // The sprite asset has to be in place before the line naming its sprites is set, or the first
            // draw of that line finds no sprite by those names.
            text.spriteAsset = runtimeSpriteAssets?.Primary;
            text.text = resolved.BuildText(FormatSprite);
        }

        /// <summary>
        /// Gather the sprites the line asks for into sprite assets TextMeshPro can draw from, and remember
        /// the name each glyph is written as.
        /// </summary>
        private void BuildSpriteAssets(InlineGlyphResolutions resolved)
        {
            spriteNamesByTagStart.Clear();

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
        /// The sprite tag for a glyph, or nothing when it has no sprite, which leaves its display name.
        /// </summary>
        private string FormatSprite(InlineGlyphResolution resolution)
        {
            return spriteNamesByTagStart.TryGetValue(resolution.Tag.StartIndex, out string spriteName)
                ? $"<sprite name=\"{spriteName}\">"
                : string.Empty;
        }

        private void Release()
        {
            resolutions?.Dispose();
            resolutions = null;

            runtimeSpriteAssets?.Dispose();
            runtimeSpriteAssets = null;
            spriteNamesByTagStart.Clear();
        }
    }
}
#endif
