using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Utilities;

namespace NPTP.InputSystemWrapper.Text
{
    /// <summary>
    /// Reads "&lt;isw ...&gt;" elements out of a string. Only the action is required, so the shortest form
    /// is &lt;isw action="Fire"&gt;, and the fullest names the player, the composite part and the binding:
    /// &lt;isw type="sprite" player=1 action="Gameplay.Fire" composite="positive" index=2&gt;.
    /// </summary>
    public static class InlineGlyphTagParser
    {
        private const string TAG_OPENING = "<isw";

        private static readonly Regex attributePattern = new(
            @"(?<name>[A-Za-z][A-Za-z0-9_]*)\s*=\s*(?:""(?<value>[^""]*)""|'(?<value>[^']*)'|(?<value>[^\s>]+))",
            RegexOptions.Compiled);

        /// <summary>
        /// Whether a string is worth parsing at all, so text with no glyphs in it costs one search.
        /// </summary>
        public static bool ContainsTag(string text)
        {
            return !string.IsNullOrEmpty(text) && text.IndexOf(TAG_OPENING, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Every glyph element in the string, in the order they appear. An element naming no action is
        /// left out with a warning, so it stays in the text as written rather than showing a wrong glyph.
        /// </summary>
        public static List<InlineGlyphTag> Parse(string text)
        {
            List<InlineGlyphTag> tags = new();
            if (string.IsNullOrEmpty(text))
            {
                return tags;
            }

            int searchFrom = 0;
            while (searchFrom < text.Length)
            {
                int start = text.IndexOf(TAG_OPENING, searchFrom, StringComparison.OrdinalIgnoreCase);
                if (start < 0)
                {
                    break;
                }

                int afterName = start + TAG_OPENING.Length;

                // A longer name that merely starts with this one, like <iswitch>, is not one of these.
                if (afterName < text.Length && text[afterName] != '>' && !char.IsWhiteSpace(text[afterName]))
                {
                    searchFrom = afterName;
                    continue;
                }

                if (!TryFindClose(text, afterName, out int close))
                {
                    break;
                }

                if (TryReadTag(text.Substring(afterName, close - afterName), start, close - start + 1, out InlineGlyphTag tag))
                {
                    tags.Add(tag);
                }

                searchFrom = close + 1;
            }

            return tags;
        }

        /// <summary>The element's closing angle bracket, skipping over any inside a quoted value.</summary>
        private static bool TryFindClose(string text, int startIndex, out int closeIndex)
        {
            char quote = '\0';
            for (int i = startIndex; i < text.Length; i++)
            {
                char character = text[i];
                if (quote != '\0')
                {
                    if (character == quote)
                    {
                        quote = '\0';
                    }
                }
                else if (character == '"' || character == '\'')
                {
                    quote = character;
                }
                else if (character == '>')
                {
                    closeIndex = i;
                    return true;
                }
            }

            closeIndex = -1;
            return false;
        }

        private static bool TryReadTag(string attributes, int startIndex, int length, out InlineGlyphTag tag)
        {
            InlineGlyphType glyphType = InlineGlyphType.Sprite;
            int playerID = 0;
            string actionMapName = string.Empty;
            string actionName = string.Empty;
            CompositePart compositePart = CompositePart.DontIsolatePart;
            int uiIndex = 0;

            foreach (Match match in attributePattern.Matches(attributes))
            {
                string name = match.Groups["name"].Value;
                string value = match.Groups["value"].Value;

                switch (name.ToLowerInvariant())
                {
                    case "type":
                        if (!TryReadEnum(value, out glyphType))
                        {
                            ISWDebug.LogWarning($"Inline glyph type \"{value}\" is not a glyph type, so the sprite is shown.");
                        }

                        break;
                    case "player":
                        playerID = ReadIndex(value, "player");
                        break;
                    case "action":
                        ReadAction(value, out actionMapName, out actionName);
                        break;
                    case "composite":
                        if (!TryReadEnum(value, out compositePart))
                        {
                            ISWDebug.LogWarning($"Inline glyph composite part \"{value}\" is not a composite part, " +
                                                "so the whole binding is shown.");
                        }

                        break;
                    case "index":
                        uiIndex = ReadIndex(value, "index");
                        break;
                    default:
                        ISWDebug.LogWarning($"Inline glyph attribute \"{name}\" is not one this understands and is ignored.");
                        break;
                }
            }

            if (string.IsNullOrEmpty(actionName))
            {
                ISWDebug.LogWarning("An inline glyph names no action and is left in the text as written. " +
                                    "Write it as <isw action=\"Fire\"> or <isw action=\"Gameplay.Fire\">.");
                tag = default;
                return false;
            }

            tag = new InlineGlyphTag(startIndex, length, glyphType, playerID, actionMapName, actionName, compositePart, uiIndex);
            return true;
        }

        /// <summary>Splits "Map.Action" into its two names. A name on its own is the action.</summary>
        private static void ReadAction(string value, out string actionMapName, out string actionName)
        {
            int separator = value.IndexOf('.');
            if (separator < 0)
            {
                actionMapName = string.Empty;
                actionName = value.Trim();
                return;
            }

            actionMapName = value.Substring(0, separator).Trim();
            actionName = value.Substring(separator + 1).Trim();
        }

        private static bool TryReadEnum<T>(string value, out T parsed) where T : struct, Enum
        {
            if (Enum.TryParse(value.Trim(), ignoreCase: true, out parsed) && Enum.IsDefined(typeof(T), parsed))
            {
                return true;
            }

            parsed = default;
            return false;
        }

        private static int ReadIndex(string value, string attributeName)
        {
            if (!int.TryParse(value.Trim(), out int parsed))
            {
                ISWDebug.LogWarning($"Inline glyph {attributeName} \"{value}\" is not a number, so 0 is used.");
                return 0;
            }

            if (parsed < 0)
            {
                ISWDebug.LogWarning($"Inline glyph {attributeName} {parsed.ToString()} is below zero, so 0 is used.");
                return 0;
            }

            return parsed;
        }
    }
}
