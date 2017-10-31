using System;

namespace Bnf.Serialization
{
    [AttributeUsage(AttributeTargets.Property)]
    public class BnfPropertyAttribute : Attribute
    {
        public string Key { get; set; }

        public bool Required { get; set; }

        public string DataFormatString { get; set; }

        public string NullText { get; set; }

        public string Description { get; set; }
       
    }
}
