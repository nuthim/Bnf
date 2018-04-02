using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;


namespace Bnf.Serialization.Infrastructure
{
    internal class DeserializerImpl
    {
        #region Fields
        private readonly PropertyMetaDataFactory _metadataFactory = new PropertyMetaDataFactory();
        private readonly ExpandoObjectGenerator _expandoGenerator;
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

        public DeserializerImpl(BnfSettings settings, char fieldSeparator, char keyValueSeparator)
        {
            Settings = settings;
            FieldSeparator = fieldSeparator;
            KeyValueSeparator = keyValueSeparator;
            _expandoGenerator = new ExpandoObjectGenerator(settings, fieldSeparator, keyValueSeparator);
        }

        #endregion

        public T Deserialize<T>(string bnf)
        {
            var expando = _expandoGenerator.CreateExpandoObject(bnf);
            return (T)DeserializeIntl(expando, typeof(T), null);
        }

        public object Deserialize(string bnf, Type type)
        {
            var expando = _expandoGenerator.CreateExpandoObject(bnf);
            return DeserializeIntl(expando, type, null);
        }


        #region Helper Methods

        private object DeserializeIntl(ExpandoObject expandoObject, Type type, string elementName)
        {
            var pairs = expandoObject as IDictionary<string, object>;

            if (TypeHelper.IsEnumerable(type))
            {
                var itemType = type.IsGenericType ? type.GetGenericArguments()?.FirstOrDefault() : type.GetElementType();
                if (itemType == null)
                    itemType = type.BaseType.IsGenericType ? type.BaseType.GetGenericArguments()?.FirstOrDefault() : type.BaseType.GetElementType();

                if (itemType == typeof(object))
                    throw new NotSupportedException("Deserializing object[] is not supported");

                MethodInfo method = GetType().GetMethod("DeserializeArrayIntl", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(itemType);
                var value = method.Invoke(this, new object[] { pairs, elementName });

                return type.IsArray ? value : Activator.CreateInstance(type, value);
            }

            var result = FormatterServices.GetUninitializedObject(type);

            foreach (var metadata in _metadataFactory.GetPropertyMetaData(result).Where(x => x.Exclude == false))
            {
                var propertyInfo = metadata.Property;

                if (!pairs.ContainsKey(metadata.KeyName))
                    continue;

                var data = pairs[metadata.KeyName];
                var expando = data as ExpandoObject;

                var propertyValue = expando != null ? DeserializeIntl(expando, propertyInfo.PropertyType, metadata.ItemName) : DeserializeValue(data.ToString(), metadata.DataFormatString ?? Settings.GetFormatString(propertyInfo.PropertyType), propertyInfo.PropertyType);
                propertyInfo.SetValue(result, propertyValue);
            }

            return result;
        }

        private T[] DeserializeArrayIntl<T>(IDictionary<string, object> pairs, string elementName)
        {
            var itemType = typeof(T);
            var list = new List<T>(pairs.Count);

            string expectedKey = elementName;

            if (string.IsNullOrWhiteSpace(expectedKey))
            {
                if (itemType != typeof(object))
                    expectedKey = itemType.FullName;
                else
                    throw new InvalidOperationException("");
            }

            foreach (var item in pairs)
            {
                var match = Regex.Match(item.Key, $"^{expectedKey}(?<index>\\d+)$");
                if (!match.Success)
                    throw new InvalidOperationException("");

                var serializedIndex = int.Parse(match.Groups["index"].Value);

                var data = pairs[item.Key];

                var isPrimitive = !itemType.IsClass || itemType == typeof(string);
                var value = isPrimitive ? TypeDescriptor.GetConverter(itemType).ConvertFrom(data) : DeserializeIntl((ExpandoObject)data, itemType, null);
                list.Insert(serializedIndex - 1, (T)value);
            }

            return list.ToArray();
        }

        private static object DeserializeValue(string strValue, string formatString, Type resultType)
        {
            if (resultType.IsEnum)
                return GetEnumValue(strValue, resultType);

            if (string.IsNullOrEmpty(formatString))
                return TypeDescriptor.GetConverter(resultType).ConvertFrom(strValue);
            
            if (resultType == typeof(TimeSpan))
            {
                TimeSpan timeSpan;
                TimeSpan.TryParseExact(strValue, formatString, null, out timeSpan);
                return timeSpan;
            }

            if (resultType == typeof(DateTime))
            {
                DateTime dateTime;
                DateTime.TryParseExact(strValue, formatString, null, System.Globalization.DateTimeStyles.None, out dateTime);
                return dateTime;
            }

            return TypeDescriptor.GetConverter(resultType).ConvertFrom(strValue);
        }

        private static object GetEnumValue(string strValue, Type enumType)
        {
            if (Enum.IsDefined(enumType, strValue))
                return Enum.Parse(enumType, strValue);

            foreach (var member in enumType.GetMembers())
            {
                var enumAttribute = member.GetCustomAttribute<EnumMemberAttribute>();
                if (enumAttribute == null)
                    continue;

                if (strValue == enumAttribute.Value)
                    return Enum.Parse(enumType, member.Name);
            }

            throw new InvalidCastException();
        }

        #endregion
    }
}
