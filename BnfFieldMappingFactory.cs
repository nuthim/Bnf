using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Bnf.Serialization
{
    public class BnfFieldMappingFactory
    {
        public IEnumerable<BnfPropertyMap> GetBnfFieldMappings(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var result = new List<BnfPropertyMap>(GetFields(0, obj.GetType()));
            return result.OrderByDescending(x => x.InheritanceLevel).ThenBy(x => x.Attribute.Order);
        }

        private static IEnumerable<BnfPropertyMap> GetFields(int level, Type type)
        {
            var map = new List<BnfPropertyMap>();

            if (type.BaseType != typeof(object))
                map.AddRange(GetFields(level + 1, type.BaseType));

            foreach (var property in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                var orcAttribute = property.GetCustomAttribute(typeof(BnfPropertyAttribute)) as BnfPropertyAttribute;
                if (orcAttribute == null)
                    continue;

                if (map.Any(x => x.InheritanceLevel == level && x.Attribute.Key == orcAttribute.Key))
                    throw new InvalidOperationException($"More than one property in type {type.FullName} has the same bnf key:{orcAttribute.Key}. Keys must be uniquely defined for the type");

                map.Add(new BnfPropertyMap(property, orcAttribute, level));
            }

            return map;
        }
    }
}



