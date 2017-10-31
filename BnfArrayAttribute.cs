using System;


namespace Bnf.Serialization
{
    [AttributeUsage(AttributeTargets.Property)]
    public class BnfArrayAttribute : Attribute
    {
        public string ElementName { get; set; }
    }
}
