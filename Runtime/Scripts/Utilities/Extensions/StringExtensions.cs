using System.Linq;
using System.Text.RegularExpressions;

namespace NPTP.InputSystemWrapper.Utilities.Extensions
{
    internal static class StringExtensions
    {
        internal static string AllWhitespaceTrimmed(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            return new string(s.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        internal static string AlphaNumericCharactersOnly(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            return Regex.Replace(s, "[^a-zA-Z0-9]", string.Empty);
        }
    }
}
