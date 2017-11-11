using System;

namespace Bnf.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataFormatAttribute : Attribute
    {
        public string DataFormatString { get; set; }

        public string NullText { get; set; }
    }
}
