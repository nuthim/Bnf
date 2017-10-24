using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Bnf.Serialization
{
    public class BnfFieldMappingFactory
    {
        public IEnumerable<BnfFieldMap> GetBnfFieldMappings(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var result = new List<BnfFieldMap>(GetFields(0, obj.GetType()));
            return result.OrderByDescending(x => x.InheritanceLevel).ThenBy(x => x.Attribute.Order);
        }

        private static IEnumerable<BnfFieldMap> GetFields(int level, Type type)
        {
            var map = new List<BnfFieldMap>();

            if (type.BaseType != typeof(object))
                map.AddRange(GetFields(level + 1, type.BaseType));

            foreach (var property in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                var orcAttribute = property.GetCustomAttribute(typeof(BnfAttribute)) as BnfAttribute;
                if (orcAttribute == null)
                    continue;

                map.Add(new BnfFieldMap(property, orcAttribute, level));
            }

            return map;
        }
    }
}



