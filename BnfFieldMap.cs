using System.Reflection;

namespace Bnf.Serialization
{
    public class BnfFieldMap
    {
        public PropertyInfo Property { get; }

        public BnfAttribute Attribute { get; }

        public int InheritanceLevel { get; }

        public BnfFieldMap(PropertyInfo property, BnfAttribute attribute, int inheritanceLevel)
        {
            Property = property;
            Attribute = attribute;
            InheritanceLevel = inheritanceLevel;
        }
    }
}