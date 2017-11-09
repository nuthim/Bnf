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

            var replacedIndex = new List<int>();
            foreach (var code in unescapeCodes)
            {
                int startIndex = 0;
                int index = -1;
                do
                {
                    index = value.IndexOf(code.Key, startIndex);
                    if (index > -1)
                    {
                        startIndex = index + 1;
                        if (!replacedIndex.Contains(index))
                        {
                            for (int i = 0; i < replacedIndex.Count; i++)
                            {
                                if (replacedIndex[i] > index)
                                    replacedIndex[i] -= code.Key.Length - 1;
                            } 
                            replacedIndex.Add(index);
                            value = value.Remove(index, code.Key.Length).Insert(index, code.Value.ToString());
                        }
                    }

                } while (index != -1 && startIndex < value.Length - 1);
                
            }

            return value;
        }
    }
}
