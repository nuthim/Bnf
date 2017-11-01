using System;


namespace Bnf.Serialization
{
    [AttributeUsage(AttributeTargets.Property)]
    public class BnfArrayAttribute : Attribute
    {
        public string Key { get; set; }

        public string ElementName { get; set; }
    }
}
