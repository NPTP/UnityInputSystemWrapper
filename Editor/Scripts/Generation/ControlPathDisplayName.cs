using System.Collections.Generic;
using System.Text;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Turns a control path into something readable, for a binding's default display name. The input
    /// system's own display names are not specific enough to build these from: a gamepad calls both
    /// "dpad/x" and "leftStick/x" simply "X".
    /// </summary>
    internal static class ControlPathDisplayName
    {
        /// <summary>
        /// Words the input system spells one way and players read another, matched however they are
        /// capitalized in a path.
        /// </summary>
        private static readonly Dictionary<string, string> specialCasedWords = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { "dpad", "D-Pad" }
        };

        /// <summary>
        /// A control path as words, e.g. "leftStick/up" becomes "Left Stick Up". Path separators and
        /// camel case both start a new word, and every word is capitalized.
        /// </summary>
        internal static string FromControlPath(string controlPath)
        {
            if (string.IsNullOrEmpty(controlPath))
            {
                return string.Empty;
            }

            StringBuilder displayName = new();

            foreach (string word in SplitIntoWords(controlPath))
            {
                if (displayName.Length > 0)
                {
                    displayName.Append(' ');
                }

                displayName.Append(specialCasedWords.TryGetValue(word, out string specialCased) ? specialCased : Capitalize(word));
            }

            return displayName.ToString();
        }

        private static string Capitalize(string word)
        {
            return word.Length == 1 ? word.ToUpperInvariant() : char.ToUpperInvariant(word[0]) + word.Substring(1);
        }

        private static IEnumerable<string> SplitIntoWords(string controlPath)
        {
            StringBuilder word = new();

            for (int i = 0; i < controlPath.Length; i++)
            {
                char character = controlPath[i];

                if (character == '/')
                {
                    if (word.Length > 0)
                    {
                        yield return word.ToString();
                        word.Clear();
                    }

                    continue;
                }

                if (word.Length > 0 && StartsNewWord(controlPath, i))
                {
                    yield return word.ToString();
                    word.Clear();
                }

                word.Append(character);
            }

            if (word.Length > 0)
            {
                yield return word.ToString();
            }
        }

        /// <summary>
        /// Whether a character begins a new word: an upper case letter after a lower case one, or a digit
        /// after a letter. An upper case run is left alone, so an acronym stays one word.
        /// </summary>
        private static bool StartsNewWord(string controlPath, int index)
        {
            char character = controlPath[index];
            char previous = controlPath[index - 1];

            if (char.IsUpper(character))
            {
                return char.IsLower(previous) || char.IsDigit(previous);
            }

            return char.IsDigit(character) && char.IsLetter(previous);
        }
    }
}
