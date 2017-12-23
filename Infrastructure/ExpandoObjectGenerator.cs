using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;


namespace Bnf.Serialization.Infrastructure
{
    internal class ExpandoObjectGenerator
    {
        #region Fields
        private const string ComplexTypePattern = "{(?<value>.*)}"; //Combination of key/value pairs enclosed in braces; {...}.
        private readonly ReadOnlyDictionary<string, char> _unescapeCodes;
        #endregion

        #region Properties

        public BnfSettings Settings
        {
            get;
        }

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

        public ExpandoObjectGenerator(BnfSettings settings, char fieldSeparator, char keyValueSeparator)
        {
            Settings = settings;
            _unescapeCodes = new ReadOnlyDictionary<string, char>(GetUnescapeCodes());
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

                AddProperty(obj, propertyName, !IsComplexType(propertyValue, out fieldValue) ? (object)propertyValue.Unescape(_unescapeCodes) : CreateExpandoObject(propertyValue));
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
           
            var index = 0;
            while(index < value.Length)
            {
                var item = value[index];

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

                index ++;
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

        private IDictionary<string, char> GetUnescapeCodes()
        {
            return Settings.EscapeCodes.ToDictionary(item => item.Value, item => item.Key);
        }

        #endregion
    }
}
