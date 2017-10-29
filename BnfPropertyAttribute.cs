using System;

namespace Bnf.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class BnfPropertyAttribute : Attribute
    {
        public string Key { get; set; }

        public bool Required { get; set; }

        public int Order { get; set; }

        public string DataFormatString { get; set; }

        public string NullText { get; set; }

        public string Description { get; set; }
       
    }
}
