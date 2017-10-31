using System;
using System.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;

namespace Bnf.Serialization
{
    public class BnfSerializer
    {
        #region Fields
        private const string ComplexTypePattern = "{(?<value>.*)}"; //Combination of key/value pairs enclosed in braces; {...}.
        private BnfSettings _settings;
        private const char FieldSeparator = '|';
        private const char KeyValueSeparator = '=';
        #endregion

        public BnfSettings Settings
        {
            get { return _settings ?? (_settings = new BnfSettings()); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _settings = value;
            }
        }

        #region Constructor

        public BnfSerializer() : this(null)
        {

        }

        public BnfSerializer(BnfSettings settings)
        {
            _settings = settings;
        }

        #endregion

        public string Serialize(object data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            IEnumerable<KeyValuePair<string, string>> pairs;

            if (data.GetType().IsArray)
            {
                var array = data as object[];
                pairs = Serialize(array);
            }
            else
            {

                var validator = new BnfValidator();
                List<Exception> errors;
                if (!validator.Validate(data, out errors))
                    throw new InvalidOperationException("Invalid data", new AggregateException(errors));

                pairs = GetKeyValuePairs(data);
            }

            return $"{{{pairs.Join(KeyValueSeparator, FieldSeparator)}}}";
        }

        public ExpandoObject Deserialize(string bnf)
        {
            return CreateExpandoObject(bnf);
        }


        #region Helper Methods

        private IEnumerable<KeyValuePair<string, string>> Serialize(object[] array)
        {
            var fields = new List<KeyValuePair<string, string>>();
            for (var index = 0; index < array.Length; index++)
            {
                var item = array[index];
                if (item == null)
                    continue;

                var str = Serialize(item);
                fields.Add(new KeyValuePair<string, string>($"{item.GetType().Name}{index+1}", str));
            }

            return fields.ToArray();
        }

        private IEnumerable<KeyValuePair<string, string>> GetKeyValuePairs(object data)
        {
            var fields = new List<KeyValuePair<string, string>>();
            var fieldFactory = new BnfFieldMappingFactory();
            foreach (var map in fieldFactory.GetBnfFieldMappings(data))
            {
                var bnfAttribute = map.Attribute;
                var propertyInfo = map.Property;
                var propertyValue = propertyInfo.GetValue(data);

                var nullText = bnfAttribute?.NullText ?? Settings.NullText;
                if (propertyValue == null && nullText == null)
                    continue;

                var isPrimitive = !propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType == typeof(string);
                var fieldValue = propertyValue == null ? nullText :
                    !isPrimitive ? Serialize(propertyValue) :
                    GetFormattedValue(propertyValue, bnfAttribute?.DataFormatString);

                fields.Add(new KeyValuePair<string, string>(bnfAttribute?.Key ?? propertyInfo.Name, fieldValue));
            }

            return fields.ToArray();
        }

        private string GetFormattedValue(object value, string formatString)
        {
            var type = value.GetType();
            if (type == typeof(string))
                value = value.ToString().Escape(Settings.EscapeCodes);

            var format = formatString ?? Settings.GetFormatString(type);
            if (format == null)
                return string.Format("{0}", value);

            return string.Format(format, value);
        }

        private ExpandoObject CreateExpandoObject(string bnf)
        {
            string fieldValue;
            IsComplexType(bnf, out fieldValue);

            var fields = GetPairs(fieldValue);

            var obj = new ExpandoObject();
            foreach (var field in fields)
            {
                var items = field.Split(new[] {KeyValueSeparator}, 2);
                var propertyName = items[0].Trim();
                var propertyValue = items[1].Trim();

                AddProperty(obj, propertyName, !IsComplexType(propertyValue, out fieldValue) ? (object) propertyValue : CreateExpandoObject(propertyValue));
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
