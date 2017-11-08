using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Bnf.Serialization.Infrastructure
{
    internal static class StringExtension
    {
        public static string Escape(this string value, IReadOnlyDictionary<char, string> escapeCodes)
        {
            var builder = new StringBuilder();

            if (escapeCodes == null || !escapeCodes.Any())
                return value;

            foreach (var chr in value)
            {
                if (!escapeCodes.ContainsKey(chr))
                {
                    builder.Append(chr);
                    continue;
                }

                var code = escapeCodes[chr];
                builder.Append(code);
            }

            return builder.ToString();
        }

        public static string Unescape(this string value, IReadOnlyDictionary<string, char> unescapeCodes)
        {
            if (unescapeCodes == null || !unescapeCodes.Any())
                return value;

            foreach (var code in unescapeCodes)
                value = value.Replace(code.Key, code.Value.ToString());

            return value;
        }
    }
}
