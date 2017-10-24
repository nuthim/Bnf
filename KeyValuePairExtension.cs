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
                builder.Append($"{pair.Key}{keyvalueSeparator}{pair.Value} {fieldSeparator} ");
            }

            return builder.ToString().Trim().TrimEnd(fieldSeparator);
        }
    }
}
