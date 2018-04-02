using System.Reflection;


namespace Bnf.Serialization.Infrastructure
{
    internal class PropertyMetaData
    {
        public PropertyInfo Property { get; internal set; }

        public bool IsDataMember { get; internal set; }

        public string KeyName { get; internal set; }

        public string NullText { get; internal set; }

        public bool EmitDefaultValue { get; internal set; }

        public object DefaultValue { get; internal set; }

        public bool Exclude { get; internal set; }

        public bool IsRequired { get; internal set; }

        public int? Order { get; internal set; }

        public int InheritanceLevel { get; internal set; }

        public string DataFormatString { get; internal set; }

        public bool IsReadWriteProperty { get; internal set; }

        public string ItemName { get; internal set; }

        public bool IsEnumerable { get; internal set; }

        public bool IsPrimitive { get; internal set; }

        public bool IsEnum { get; internal set; }

    }
}