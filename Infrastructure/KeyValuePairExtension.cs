using System.Collections.Generic;
using System.Text;

namespace Bnf.Serialization
{
    internal static class KeyValuePairExtension
    {
        public static string Join<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> pairs, char keyvalueSeparator, char fieldSeparator)
        {
            var builder = new StringBuilder();
            foreach (var pair in pairs)
            {
                if (builder.Length > 0)
                    builder.Append($" {fieldSeparator} ");

                builder.Append($"{pair.Key}{keyvalueSeparator}{pair.Value}");
            }

            return builder.ToString().Trim();
        }
    }
}
