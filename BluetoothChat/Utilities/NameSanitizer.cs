using System.Linq;
using System.Text.RegularExpressions;

namespace BluetoothChat.Utilities
{
    public static class NameSanitizer
    {
        private const int MaxNameLength = 32;

        public static string Sanitize(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Anonymous";
            }

            string sanitized = new string(name.Where(c => !char.IsControl(c)).ToArray()).Trim();
            sanitized = Regex.Replace(sanitized, @"\[host\]", string.Empty, RegexOptions.IgnoreCase).Trim();
            sanitized = Regex.Replace(sanitized, @"[^\p{L}\p{M}\p{Nd} _.\-]", string.Empty).Trim();

            if (sanitized.Length > MaxNameLength)
            {
                int limit = MaxNameLength;
                if (char.IsHighSurrogate(sanitized[limit - 1]))
                {
                    limit--;
                }
                sanitized = sanitized.Substring(0, limit);
            }

            return sanitized.Length == 0 ? "Anonymous" : sanitized;
        }
    }
}
