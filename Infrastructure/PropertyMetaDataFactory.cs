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
            return result.OrderByDescending(x => x.InheritanceLevel);
        }

        private static IEnumerable<PropertyMetaData> GetProperties(int level, Type type)
        {
            var map = new List<PropertyMetaData>();

            if (type.BaseType != typeof(object))
                map.AddRange(GetProperties(level + 1, type.BaseType));

            foreach (var propertyInfo in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                var attributes = propertyInfo.GetCustomAttributes();
                var dataMemberAttribute = attributes.OfType<DataMemberAttribute>().SingleOrDefault();
                var dataFormatAttribute = attributes.OfType<DataFormatAttribute>().SingleOrDefault();

                var isEnumerable = TypeHelper.IsEnumerable(propertyInfo.PropertyType);

                var dataContractAttribute = propertyInfo.PropertyType.GetCustomAttribute<DataContractAttribute>();
                var collectionAttribute = isEnumerable ? propertyInfo.PropertyType.GetCustomAttribute<CollectionDataContractAttribute>() : null;
                
                var metaData = new PropertyMetaData(type, propertyInfo, level);

                metaData.IsDataMember = dataMemberAttribute != null;
                metaData.IsEnumerable = isEnumerable;
                metaData.IsPrimitive = TypeHelper.IsPrimitive(propertyInfo.PropertyType);
                metaData.IsEnum = propertyInfo.PropertyType.IsEnum;

                if (isEnumerable)
                {
                    metaData.KeyName = dataMemberAttribute?.Name ?? collectionAttribute?.Name ?? propertyInfo.Name;
                    metaData.ItemName = collectionAttribute?.ItemName ?? TypeHelper.GetElementType(propertyInfo.PropertyType).FullName;
                }
                else if (propertyInfo.PropertyType.IsEnum)
                    metaData.KeyName = dataMemberAttribute?.Name ?? propertyInfo.Name;
                else
                    metaData.KeyName = dataMemberAttribute?.Name ?? dataContractAttribute?.Name ?? propertyInfo.Name;

                var isReadWriteProperty = propertyInfo.CanRead && propertyInfo.CanWrite;
                metaData.IsReadWriteProperty = isReadWriteProperty;
                metaData.IsRequired = dataMemberAttribute?.IsRequired ?? false;
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



