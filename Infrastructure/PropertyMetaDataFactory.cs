using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Bnf.Serialization
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

            foreach (var property in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                var attributes = property.GetCustomAttributes();
                map.Add(new PropertyMetaData(type, property, level, attributes.OfType<BnfIgnoreAttribute>().SingleOrDefault(), attributes.OfType<BnfPropertyAttribute>().SingleOrDefault()));
            }

            return map;
        }


    }
}



