using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Bnf.Serialization.Attributes;

namespace Bnf.Serialization.Infrastructure
{
    internal class DeserializerImpl
    {
        #region Fields
        private PropertyMetaDataFactory _metadataFactory = new PropertyMetaDataFactory();
        private ExpandoObjectGenerator _expandoGenerator;
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
            _expandoGenerator = new ExpandoObjectGenerator(fieldSeparator, keyValueSeparator);
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

        private object DeserializeIntl(ExpandoObject expandoObject, Type type, BnfPropertyAttribute propertyAttribute)
        {
            var pairs = expandoObject as IDictionary<string, object>;

            if (type.IsArray)
            {
                var itemType = type.GetElementType();
                if (itemType == typeof(object))
                    throw new NotSupportedException("Deserializing object[] is not supported");

                MethodInfo method = GetType().GetMethod("DeserializeArrayIntl", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(itemType);
                var value = method.Invoke(this, new object[] { pairs, propertyAttribute });
                return value;
            }

            var result = FormatterServices.GetUninitializedObject(type);

            foreach (var map in _metadataFactory.GetPropertyMetaData(result).Where(x => x.IsReadWriteProperty && x.CustomBnfIgnoreAttribute == null))
            {
                var bnfAttribute = map.CustomBnfPropertyAttribute;
                var propertyInfo = map.Property;

                var key = bnfAttribute?.Key ?? propertyInfo.Name;
                if (!pairs.ContainsKey(key))
                    continue;

                var data = pairs[key];
                var expando = data as ExpandoObject;

                MethodBase method = MethodBase.GetCurrentMethod();
                var propertyValue = expando != null ? method.Invoke(this, new object[] { expando, propertyInfo.PropertyType, bnfAttribute }) : TypeDescriptor.GetConverter(propertyInfo.PropertyType).ConvertFrom(data);
                propertyInfo.SetValue(result, propertyValue);
            }

            return result;
        }

        private T[] DeserializeArrayIntl<T>(IDictionary<string, object> pairs, BnfPropertyAttribute bnfAttribute)
        {
            var itemType = typeof(T);
            var list = new List<T>(pairs.Count);

            string expectedKey = bnfAttribute?.ElementName;

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
                var expando = data as ExpandoObject;

                var isPrimitive = !itemType.IsClass || itemType == typeof(string);
                var value = isPrimitive ? TypeDescriptor.GetConverter(itemType).ConvertFrom(data) : DeserializeIntl((ExpandoObject)data, itemType, null);
                list.Insert(serializedIndex - 1, (T)value);
            }

            return list.ToArray();
        }

        #endregion
    }
}
