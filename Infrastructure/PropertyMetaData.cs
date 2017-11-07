using System;
using System.Reflection;
using Bnf.Serialization.Attributes;

namespace Bnf.Serialization.Infrastructure
{
    public class PropertyMetaData
    {
        public Type Type { get; }

        public PropertyInfo Property { get; }

        public int InheritanceLevel { get; }

        public BnfIgnoreAttribute CustomBnfIgnoreAttribute { get; }

        public BnfPropertyAttribute CustomBnfPropertyAttribute { get; }

        public bool IsReadWriteProperty
        {
            get
            {
                return Property.CanRead && Property.CanWrite;
            }
        }
        public PropertyMetaData(Type type, PropertyInfo propertyInfo, int inheritanceLevel, BnfIgnoreAttribute ignoreAttribute, BnfPropertyAttribute propertyAttribute)
        {
            Type = type;
            Property = propertyInfo;
            InheritanceLevel = inheritanceLevel;
            CustomBnfIgnoreAttribute = ignoreAttribute;
            CustomBnfPropertyAttribute = propertyAttribute;
        }
    }
}