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
            return result.OrderByDescending(x => x.InheritanceLevel);
        }

        private static IEnumerable<BnfPropertyMap> GetFields(int level, Type type)
        {
            var map = new List<BnfPropertyMap>();

            if (type.BaseType != typeof(object))
                map.AddRange(GetFields(level + 1, type.BaseType));

            foreach (var property in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead && x.CanWrite))
            {
                var attributes = property.GetCustomAttributes();
                if (attributes.OfType<BnfIgnoreAttribute>().Any())
                    continue;

                map.Add(new BnfPropertyMap(property, attributes.OfType<BnfPropertyAttribute>().SingleOrDefault(), level));
            }

            return map;
        }
    }
}



