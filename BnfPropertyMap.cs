using System.Reflection;

namespace Bnf.Serialization
{
    public class BnfPropertyMap
    {
        public PropertyInfo Property { get; }

        public BnfPropertyAttribute Attribute { get; }

        public int InheritanceLevel { get; }

        public BnfPropertyMap(PropertyInfo property, BnfPropertyAttribute attribute, int inheritanceLevel)
        {
            Property = property;
            Attribute = attribute;
            InheritanceLevel = inheritanceLevel;
        }
    }
}