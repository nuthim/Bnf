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
    }
}
