using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NPTP.InputSystemWrapper.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static string AllWhitespaceTrimmed(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            
            return new string(s.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        public static string SpaceBetweenWords(this string s)
        {
            StringBuilder sb = new();
            
            for (int i = 0; i < s.Length; i++)
            {
                char cur = s[i];
                if (char.IsWhiteSpace(cur))
                {
                    continue;
                }
                
                if (i > 0)
                {
                    char prev = s[i - 1];
                    bool newWordCondition = char.IsLower(prev) && char.IsUpper(cur);
                    bool numberStartCondition = char.IsLetter(prev) && char.IsNumber(cur);
                    if (newWordCondition || numberStartCondition)
                    {
                        sb.Append(' ');
                    }
                }
                sb.Append(cur);
            }

            return sb.ToString();
        }

        public static string CapitalizeFirst(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            
            string upper = s.ToUpper();
            if (s.Length == 1) return upper;
            return upper[0] + s[1..];
        }

        public static string LowercaseFirst(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            
            string lower = s.ToLower();
            if (s.Length == 1) return lower;
            return lower[0] + s[1..];
        }
        
        public static string AlphaNumericCharactersOnly(this string s)
        {
            return Regex.Replace(s, "[^a-zA-Z0-9]", string.Empty);
        }

        public static string RemoveFirstCharacterIfNumber(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (!char.IsNumber(s[0])) return s;
            return s.Length > 1 ? s[1..] : string.Empty;
        }
    }
}