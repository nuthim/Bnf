using System;
using System.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace Bnf.Serialization
{
    public class BnfSerializer
    {
        #region Fields
        private const string ComplexTypePattern = "{(?<value>.*)}"; //Combination of key/value pairs enclosed in braces; {...}.
        private BnfSettings _settings;
        private const char FieldSeparator = '|';
        private const char KeyValueSeparator = '=';
        private PropertyMetaDataFactory _metadataFactory = new PropertyMetaDataFactory();
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

            if (data.GetType().IsArray)
            {
                return SerializeIntl(data as object[], null, false);
            }
            else
            {
                return SerializeIntl(data);
            }
        }

        public ExpandoObject Deserialize(string bnf)
        {
            return CreateExpandoObject(bnf);
        }


        #region Helper Methods

        private void Validate(object obj)
        {
            var validator = new BnfValidator();
            List<Exception> errors;
            if (!validator.Validate(obj, Settings, out errors))
                throw new BnfValidationException("Invalid data", new AggregateException(errors));
        }

        private string SerializeIntl(object[] array, string elementName, bool? indexed)
        {
            var pairs = new List<KeyValuePair<string, string>>();
            for (var index = 0; index < array.Length; index++)
            {
                var item = array[index];
                if (item == null)
                    continue;

                var str = item.GetType().IsArray ? SerializeIntl(item as object[], null, false) : SerializeIntl(item);
                var fieldName = elementName ?? item.GetType().Name;
                if (indexed.GetValueOrDefault(false))
                    fieldName = $"{fieldName}{index + 1}";

                pairs.Add(new KeyValuePair<string, string>($"{fieldName}", str));
            }

            return $"{{{pairs.Join(KeyValueSeparator, FieldSeparator)}}}";
        }

        private string SerializeIntl(object data)
        {
            Validate(data);

            var pairs = new List<KeyValuePair<string, string>>();
            foreach (var map in _metadataFactory.GetPropertyMetaData(data).Where(x => x.IsReadWriteProperty && x.CustomBnfIgnoreAttribute == null))
            {
                var bnfAttribute = map.CustomBnfPropertyAttribute;
                var propertyInfo = map.Property;
                var propertyValue = propertyInfo.GetValue(data);

                var nullText = bnfAttribute?.NullText ?? Settings.NullText;
                if (propertyValue == null && nullText == null)
                    continue;

                var isArray = propertyInfo.PropertyType.IsArray;
                var isPrimitive = !propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType == typeof(string);
                var fieldValue = propertyValue == null ? nullText :
                    isArray ? SerializeIntl(propertyValue as object[], bnfAttribute?.ElementName, bnfAttribute?.Indexed) :
                    !isPrimitive ? SerializeIntl(propertyValue) :
                    GetFormattedValue(propertyValue, bnfAttribute?.DataFormatString);

                pairs.Add(new KeyValuePair<string, string>(bnfAttribute?.Key ?? propertyInfo.Name, fieldValue));
            }

            return $"{{{pairs.Join(KeyValueSeparator, FieldSeparator)}}}";
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
