using System;
using System.Linq;
using System.Collections.Generic;
using Bnf.Serialization.Exceptions;


namespace Bnf.Serialization.Infrastructure
{
    internal class SerializerImpl
    {
        #region Fields
        private PropertyMetaDataFactory _metadataFactory = new PropertyMetaDataFactory();
        private BnfValidator _validator = new BnfValidator();
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

        public SerializerImpl(BnfSettings settings, char fieldSeparator, char keyValueSeparator)
        {
            Settings = settings;
            FieldSeparator = fieldSeparator;
            KeyValueSeparator = keyValueSeparator;
        }

        #endregion

        public string Serialize(object data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.GetType().IsArray)
            {
                return SerializeIntl((Array)data, null);
            }
            else
            {
                return SerializeIntl(data);
            }
        }

        #region Helper Methods

        private string SerializeIntl(Array array, string elementName)
        {
            var pairs = new List<KeyValuePair<string, string>>();
            for (var index = 0; index < array.Length; index++)
            {
                var item = array.GetValue(index);
                if (item == null)
                    continue;

                var itemType = item.GetType();
                var isPrimitive = !itemType.IsClass || itemType == typeof(string);

                var value = isPrimitive ? GetFormattedValue(item, null) : item.GetType().IsArray ? SerializeIntl((Array)item, null) : SerializeIntl(item);
                var fieldName = $"{elementName ?? item.GetType().FullName}{index + 1}";
                pairs.Add(new KeyValuePair<string, string>($"{fieldName}", value));
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
                    isArray ? SerializeIntl((Array)propertyValue, bnfAttribute?.ElementName) :
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

        private void Validate(object obj)
        {
            List<Exception> errors;
            if (!_validator.Validate(obj, Settings, out errors))
                throw new BnfValidationException("Invalid data", new AggregateException(errors));
        }

        #endregion
    }
}
