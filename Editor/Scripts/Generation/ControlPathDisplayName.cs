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
            bool startOfWord = true;

            for (int i = 0; i < controlPath.Length; i++)
            {
                char character = controlPath[i];

                if (character == '/')
                {
                    startOfWord = true;
                    continue;
                }

                if (!startOfWord && StartsNewWord(controlPath, i))
                {
                    startOfWord = true;
                }

                if (startOfWord)
                {
                    if (displayName.Length > 0)
                    {
                        displayName.Append(' ');
                    }

                    displayName.Append(char.ToUpperInvariant(character));
                    startOfWord = false;
                    continue;
                }

                displayName.Append(character);
            }

            return displayName.ToString();
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
