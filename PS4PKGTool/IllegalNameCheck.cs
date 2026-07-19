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
        public static bool IsValidFileName(this string expression, bool platformIndependent)
        {
            string sPattern = @"^(?!^(PRN|AUX|CLOCK\$|NUL|CON|COM\d|LPT\d|\..*)(\..+)?$)[^\x00-\x1f\\?*:\"";|/]+$";
            if (platformIndependent)
            {
                sPattern = @"^(([a-zA-Z]:|\\)\\)?(((\.)|(\.\.)|([^\\/:\*\?""\|<>\. ](([^\\/:\*\?""\|<>\. ])|([^\\/:\*\?""\|<>]*[^\\/:\*\?""\|<>\. ]))?))\\)*[^\\/:\*\?""\|<>\. ](([^\\/:\*\?""\|<>\. ])|([^\\/:\*\?""\|<>]*[^\\/:\*\?""\|<>\. ]))?$";
            }
            return (Regex.IsMatch(expression, sPattern, RegexOptions.CultureInvariant));
        }

        /// <summary>Replaces ALL characters unsafe for Windows file paths with underscores.</summary>
        public static string SanitizeFileName(this string name)
        {
            if (string.IsNullOrEmpty(name)) return "_";

            // Step 1: Normalize Unicode (smart quotes → ASCII equivalents)
            name = name.Normalize(System.Text.NormalizationForm.FormKC);

            // Step 2: Replace NTFS-invalid chars (Path.GetInvalidFileNameChars is the authoritative list)
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var sb = new System.Text.StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (Array.IndexOf(invalid, c) >= 0)
                    sb.Append('_');
                else if (char.IsControl(c))
                    sb.Append('_');
                else
                    sb.Append(c);
            }

            // Step 3: Trim trailing dots and spaces (Windows restriction)
            string result = sb.ToString().TrimEnd('.', ' ');

            // Step 4: Reserve DOS device names (CON, PRN, AUX, NUL, COM1-9, LPT1-9)
            string upper = result.ToUpperInvariant();
            if (upper == "CON" || upper == "PRN" || upper == "AUX" || upper == "NUL" ||
                (upper.Length == 4 && upper.StartsWith("COM") && upper[3] >= '1' && upper[3] <= '9') ||
                (upper.Length == 4 && upper.StartsWith("LPT") && upper[3] >= '1' && upper[3] <= '9'))
                result = "_" + result;

            return string.IsNullOrEmpty(result) ? "_" : result;
        }
    }
}
