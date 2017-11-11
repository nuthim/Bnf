using System;
using System.Linq;
using System.Collections.Generic;
using Bnf.Serialization.Exceptions;
using System.Collections;
using System.Runtime.Serialization;
using System.Reflection;

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

            if (TypeHelper.IsEnumerable(data.GetType()))
            {
                return SerializeIntl((IList)data, null);
            }
            else
            {
                return SerializeIntl(data);
            }
        }

        #region Helper Methods

        private string SerializeIntl(IList array, string elementName)
        {
            var pairs = new List<KeyValuePair<string, string>>();
            for (var index = 0; index < array.Count; index++)
            {
                var item = array[index];
                if (item == null)
                    continue;

                var itemType = item.GetType();

                var value = 
                    TypeHelper.IsPrimitive(itemType) ? GetFormattedValue(item, null) : 
                    TypeHelper.IsEnumerable(itemType) ? SerializeIntl((IList)item, null) : 
                    SerializeIntl(item);

                var fieldName = $"{elementName ?? itemType.FullName}{index + 1}";
                pairs.Add(new KeyValuePair<string, string>($"{fieldName}", value));
            }

            return $"{{{pairs.Join(KeyValueSeparator, FieldSeparator)}}}";
        }

        private string SerializeIntl(object data)
        {
            Validate(data);

            var pairs = new List<KeyValuePair<string, string>>();
            foreach (var metadata in _metadataFactory.GetPropertyMetaData(data).Where(x => x.Exclude == false))
            {
                var propertyValue = metadata.Property.GetValue(data);
                var propertyType = metadata.Property.PropertyType;

                var nullText = metadata?.NullText ?? Settings.NullText;
                if (propertyValue == null && nullText == null)
                    continue;

                if (propertyType.IsValueType && propertyValue.Equals(metadata.DefaultValue) && !metadata.EmitDefaultValue)
                    continue;

                var fieldValue = 
                    propertyValue == null ? nullText :
                    metadata.IsEnum ? GetEnumValue(propertyValue) :
                    metadata.IsPrimitive ? GetFormattedValue(propertyValue, metadata.DataFormatString ?? Settings.GetFormatString(propertyType)) :
                    metadata.IsEnumerable ? SerializeIntl((IList)propertyValue, metadata.ItemName) :
                    SerializeIntl(propertyValue);

                pairs.Add(new KeyValuePair<string, string>(metadata.KeyName, fieldValue));
            }

            return $"{{{pairs.Join(KeyValueSeparator, FieldSeparator)}}}";
        }

        private string GetFormattedValue(object value, string formatString)
        {
            var type = value.GetType();
            if (type == typeof(string))
                value = value.ToString().Escape(Settings.EscapeCodes);

            if (formatString == null)
                return value.ToString();

            var method = type.GetMethod("ToString", new[] { typeof(string)});
            return (string)method.Invoke(value, new[] { formatString });
        }

        private string GetEnumValue(object propertyValue)
        {
            var enumType = propertyValue.GetType();
            var enumStr = propertyValue.ToString();
            var obj = Enum.Parse(enumType, enumStr);
            var enumAttribute = enumType.GetMember(enumStr)[0].GetCustomAttribute<EnumMemberAttribute>();

            return (enumAttribute?.Value ?? enumStr).Escape(Settings.EscapeCodes);
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
