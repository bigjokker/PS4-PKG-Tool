using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PS4PKGTool
{
    public static class IllegalNameCheck
    {
        private static readonly Dictionary<char, char> _safeSubstitutes = new Dictionary<char, char>
        {
            ['<'] = '＜',   // U+FF1C Fullwidth Less-Than
            ['>'] = '＞',   // U+FF1E Fullwidth Greater-Than
            [':'] = '：',   // U+FF1A Fullwidth Colon
            ['"'] = '＂',   // U+FF02 Fullwidth Quotation Mark
            ['/'] = '／',   // U+FF0F Fullwidth Solidus
            ['\\'] = '＼',  // U+FF3C Fullwidth Reverse Solidus
            ['|'] = '｜',   // U+FF5C Fullwidth Vertical Bar
            ['?'] = '？',   // U+FF1F Fullwidth Question Mark
            ['*'] = '＊',   // U+FF0A Fullwidth Asterisk
        };

        public static bool IsValidFileName(this string expression, bool platformIndependent)
        {
            string sPattern = @"^(?!^(PRN|AUX|CLOCK\$|NUL|CON|COM\d|LPT\d|\..*)(\..+)?$)[^\x00-\x1f\\?*:\"";|/]+$";
            if (platformIndependent)
            {
                sPattern = @"^(([a-zA-Z]:|\\)\\)?(((\.)|(\.\.)|([^\\/:\*\?""\|<>\. ](([^\\/:\*\?""\|<>\. ])|([^\\/:\*\?""\|<>]*[^\\/:\*\?""\|<>\. ]))?))\\)*[^\\/:\*\?""\|<>\. ](([^\\/:\*\?""\|<>\. ])|([^\\/:\*\?""\|<>]*[^\\/:\*\?""\|<>\. ]))?$";
            }
            return (Regex.IsMatch(expression, sPattern, RegexOptions.CultureInvariant));
        }

        /// <summary>True when every character is ASCII (code point &lt; 128). orbis-pub-cmd rejects non-ASCII paths.</summary>
        public static bool IsAsciiPath(this string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] >= 128)
                    return false;
            }
            return true;
        }

        /// <summary>Replaces NTFS-invalid characters with visually-similar Unicode alternatives.</summary>
        public static string SanitizeFileName(this string name)
        {
            if (string.IsNullOrEmpty(name)) return "_";

            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var sb = new System.Text.StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (_safeSubstitutes.TryGetValue(c, out char sub))
                    sb.Append(sub);
                else if (char.IsControl(c))
                    sb.Append('_');
                else if (Array.IndexOf(invalid, c) >= 0)
                    sb.Append('_');
                else
                    sb.Append(c);
            }

            string result = sb.ToString().TrimEnd('.', ' ');

            string upper = result.ToUpperInvariant();
            if (upper == "CON" || upper == "PRN" || upper == "AUX" || upper == "NUL" ||
                (upper.Length == 4 && upper.StartsWith("COM") && upper[3] >= '1' && upper[3] <= '9') ||
                (upper.Length == 4 && upper.StartsWith("LPT") && upper[3] >= '1' && upper[3] <= '9'))
                result = "_" + result;

            return string.IsNullOrEmpty(result) ? "_" : result;
        }

        /// <summary>
        /// Builds a folder name safe for tools that choke on non-ASCII paths.
        /// Keeps ASCII letters/digits/space/dash/underscore/dot/apostrophe; maps other chars to '_'.
        /// Prefer using ASCII temp dirs for orbis I/O and only apply this when a path must be fed to orbis.
        /// </summary>
        public static string ToOrbisSafeName(this string name)
        {
            if (string.IsNullOrEmpty(name)) return "_";

            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (c < 128 && (char.IsLetterOrDigit(c) || c is ' ' or '-' or '_' or '.' or '\'' or '[' or ']' or '(' or ')'))
                    sb.Append(c);
                else if (c < 128 && c is '<' or '>' or ':' or '"' or '/' or '\\' or '|' or '?' or '*')
                    sb.Append('_');
                else if (c >= 128)
                    sb.Append('_');
                else if (char.IsControl(c))
                    sb.Append('_');
                else
                    sb.Append('_');
            }

            string result = Regex.Replace(sb.ToString(), @"_+", "_").Trim(' ', '.', '_');
            return string.IsNullOrEmpty(result) ? "_" : result.SanitizeFileName();
        }
    }
}
