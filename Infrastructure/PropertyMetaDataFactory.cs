using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Bnf.Serialization.Attributes;
using System.Runtime.Serialization;

namespace Bnf.Serialization.Infrastructure
{
    public class PropertyMetaDataFactory
    {
        public IEnumerable<PropertyMetaData> GetPropertyMetaData(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var result = new List<PropertyMetaData>(GetProperties(0, obj.GetType()));
            return result.OrderByDescending(x => x.InheritanceLevel).ThenBy(x => x.Order).ThenBy(x => x.KeyName, StringComparer.Ordinal);
        }

        private static IEnumerable<PropertyMetaData> GetProperties(int level, Type type)
        {
            var map = new List<PropertyMetaData>();

            if (type.BaseType != typeof(object))
                map.AddRange(GetProperties(level + 1, type.BaseType));

            foreach (var propertyInfo in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                var attributes = propertyInfo.GetCustomAttributes().ToArray();
                var dataMemberAttribute = attributes.OfType<DataMemberAttribute>().SingleOrDefault();
                var dataFormatAttribute = attributes.OfType<DataFormatAttribute>().SingleOrDefault();

                var isEnumerable = TypeHelper.IsEnumerable(propertyInfo.PropertyType);

                var collectionAttribute = isEnumerable ? propertyInfo.PropertyType.GetCustomAttribute<CollectionDataContractAttribute>() : null;

                var metaData = new PropertyMetaData
                {
                    Property = propertyInfo,
                    InheritanceLevel = level,
                    IsDataMember = dataMemberAttribute != null,
                    IsEnumerable = isEnumerable,
                    IsPrimitive = TypeHelper.IsPrimitive(propertyInfo.PropertyType),
                    IsEnum = propertyInfo.PropertyType.IsEnum,
                    KeyName = dataMemberAttribute?.Name ?? propertyInfo.Name
                };

                if (isEnumerable)
                    metaData.ItemName = collectionAttribute?.ItemName ?? TypeHelper.GetElementType(propertyInfo.PropertyType).FullName;

                var isReadWriteProperty = propertyInfo.CanRead && propertyInfo.CanWrite;
                metaData.IsReadWriteProperty = isReadWriteProperty;
                metaData.IsRequired = dataMemberAttribute?.IsRequired ?? false;
                metaData.Order = dataMemberAttribute?.Order;
                metaData.NullText = dataFormatAttribute?.NullText;
                metaData.EmitDefaultValue = dataMemberAttribute?.EmitDefaultValue ?? true;
                metaData.DefaultValue = TypeHelper.GetDefaultValue(propertyInfo.PropertyType);
                metaData.DataFormatString = dataFormatAttribute?.DataFormatString;
                metaData.Exclude = !isReadWriteProperty || attributes.OfType<IgnoreDataMemberAttribute>().Any() || dataMemberAttribute == null;
                map.Add(metaData);
            }

            return map;
        }


    }
}



