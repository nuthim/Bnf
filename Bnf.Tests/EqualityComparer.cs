using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;


namespace Bnf.Tests
{
    public class EqualityComparer : IEqualityComparer<object>
    {
        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x == null || y == null)
                return false;

            if (x.GetType() != y.GetType())
                return false;

            if (x is string || x.GetType().IsEnum || x.GetType().IsValueType)
            {
                if (!x.Equals(y))
                    return false;
            }

            foreach (var property in x.GetType().GetProperties())
            {
                var dataMember = property.GetCustomAttribute<DataMemberAttribute>();
                if (dataMember == null)
                    continue;

                var thisValue = property.GetValue(x);
                var otherValue = property.GetValue(y);

                if (ReferenceEquals(thisValue, otherValue))
                    continue;

                if (thisValue == null || otherValue == null)
                    return false;

                var isEnumerable = IsEnumerable(property.PropertyType);
                if (isEnumerable)
                {
                    var arrayEquals = new CollectionComparer().Compare((ICollection)thisValue, (ICollection)otherValue) == 0;
                    if (!arrayEquals)
                        return false;
                }
                else if (property.PropertyType == typeof(string) || property.PropertyType.IsValueType)
                {
                    if (!thisValue.Equals(otherValue))
                        return false;
                }
                else if (!Equals(thisValue, otherValue))
                    return false;
            }

            return true;
        }

        public int GetHashCode(object obj)
        {
            throw new NotImplementedException();
        }

        private static bool IsEnumerable(Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }
    }
}
