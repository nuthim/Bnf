using System;
using System.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace Bnf.Serialization
{
    public class BnfSerializer
    {
        private const string DictionaryPattern = "{(?<value>.*)}"; //Combination of key/value pairs enclosed in braces; {...}.
        private readonly BnfSettings _settings;
        private const char FieldSeparator = '|';
        private const char KeyValueSeparator = '=';

        #region Constructor

        public BnfSerializer() : this(null)
        {
            
        }

        public BnfSerializer(BnfSettings settings)
        {
            _settings = settings ?? new BnfSettings();
        }

        #endregion

        public string Serialize(object data)
        {
            var validator = new BnfValidator();
            List<Exception> errors;
            if (!validator.Validate(data, out errors))
                throw new InvalidOperationException("Invalid data", new AggregateException(errors));

            var pairs = GetKeyValuePairs(data);
            return $"{{{pairs.Join(KeyValueSeparator, FieldSeparator)}}}";
        }

        public ExpandoObject Deserialize(string bnf)
        {
            return CreateExpandoObject(bnf);
        }


        #region Helper Methods

        private IEnumerable<KeyValuePair<string, string>> GetKeyValuePairs(object data)
        {
            var fields = new List<KeyValuePair<string, string>>();
            var fieldFactory = new BnfFieldMappingFactory();
            foreach (var map in fieldFactory.GetBnfFieldMappings(data))
            {
                var bnfAttribute = map.Attribute;
                var propertyInfo = map.Property;
                var propertyValue = propertyInfo.GetValue(data);

                if (propertyValue == null && _settings.IgnoreNull)
                    continue;

                var isPrimitive = !propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType == typeof(string);
                var fieldValue = propertyValue == null ? bnfAttribute.NullText ?? _settings.NullText :
                    !isPrimitive ? Serialize(propertyValue) :
                    GetFormattedValue(propertyValue, bnfAttribute.DataFormatString);

                fields.Add(new KeyValuePair<string, string>(bnfAttribute.Key, fieldValue));
            }

            return fields.ToArray();
        }

        private string GetFormattedValue(object value, string formatString)
        {
            switch(value.GetType().Name)
            {
                case "DateTime":
                    return string.Format(formatString ?? _settings.DateFormat, value);

                case "TimeSpan":
                    return string.Format(formatString ?? _settings.TimeFormat, value);

                case "String":
                    return value.ToString().Escape(_settings.EscapeCodes);

                default:
                    return string.Format("{0}", value);
            }
        }

        private ExpandoObject CreateExpandoObject(string bnf)
        {
            string fieldValue;
            IsDictionary(bnf, out fieldValue);

            var fields = GetPairs(fieldValue);

            var obj = new ExpandoObject();
            foreach (var field in fields)
            {
                var items = field.Split(new[] {KeyValueSeparator}, 2);
                var propertyName = items[0].Trim();
                var propertyValue = items[1].Trim();

                AddProperty(obj, propertyName, !IsDictionary(propertyValue, out fieldValue) ? (object) propertyValue : CreateExpandoObject(propertyValue));
            }

            return obj;
        }

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

        private static bool IsDictionary(string input, out string value)
        {
            value = input;
            if (!Regex.IsMatch(input, DictionaryPattern))
                return false;

            var match = Regex.Match(input, DictionaryPattern);
            value = match.Groups["value"].Value;
            return true;
        }

        #endregion
    }
}
