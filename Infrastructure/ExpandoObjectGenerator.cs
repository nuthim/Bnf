using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;


namespace Bnf.Serialization.Infrastructure
{
    internal class ExpandoObjectGenerator
    {
        #region Fields
        private const string ComplexTypePattern = "{(?<value>.*)}"; //Combination of key/value pairs enclosed in braces; {...}.
        #endregion

        #region Properties

        public char FieldSeparator
        {
            get;
        }

        public char KeyValueSeparator
        {
            get;
        }

        #endregion

        #region Constructor

        public ExpandoObjectGenerator(char fieldSeparator, char keyValueSeparator)
        {
            FieldSeparator = fieldSeparator;
            KeyValueSeparator = keyValueSeparator;
        }

        #endregion

        public ExpandoObject CreateExpandoObject(string bnf)
        {
            string fieldValue;
            IsComplexType(bnf, out fieldValue);

            var fields = GetPairs(fieldValue);

            var obj = new ExpandoObject();
            foreach (var field in fields)
            {
                var items = field.Split(new[] { KeyValueSeparator }, 2);
                var propertyName = items[0].Trim();
                var propertyValue = items[1].Trim();

                AddProperty(obj, propertyName, !IsComplexType(propertyValue, out fieldValue) ? (object)propertyValue : CreateExpandoObject(propertyValue));
            }

            return obj;
        }

        #region Helper Methods

        private static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        private IEnumerable<string> GetPairs(string value)
        {
            var stack = new Stack<char>();
            var braceCount = 0;
            var word = new List<char>();
            var properties = new List<string>();
            foreach (var item in value)
            {
                switch (item)
                {
                    case '{':
                        braceCount++;
                        stack.Push(item);
                        break;

                    case '}':
                        braceCount--;
                        stack.Push(item);
                        break;

                    case '|':
                        if (braceCount != 0)
                            stack.Push(item);
                        else
                        {
                            word.Clear();
                            while (stack.Count > 0)
                                word.Add(stack.Pop());

                            word.Reverse();
                            properties.Add(string.Concat(word));
                        }
                        break;

                    default:
                        stack.Push(item);
                        break;
                }
            }

            if (stack.Count > 0)
            {
                word.Clear();
                while (stack.Count > 0)
                    word.Add(stack.Pop());

                word.Reverse();
                properties.Add(string.Concat(word));
            }

            return properties;
        }

        private static bool IsComplexType(string input, out string value)
        {
            value = input;
            if (!Regex.IsMatch(input, ComplexTypePattern))
                return false;

            var match = Regex.Match(input, ComplexTypePattern);
            value = match.Groups["value"].Value;
            return true;
        }

        #endregion
    }
}
