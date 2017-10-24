using System;
using System.Linq;
using System.Collections.Generic;


namespace Bnf.Serialization
{
    public class BnfValidator
    {
        public bool Validate(object obj, out List<Exception> errors)
        {
            if (obj == null)
                throw  new ArgumentNullException(nameof(obj));

            errors = new List<Exception>();
            var fieldFactory = new BnfFieldMappingFactory();

            foreach (var map in fieldFactory.GetBnfFieldMappings(obj))
            {
                var attribute = map.Attribute;
                var property = map.Property;

                var fieldName = attribute.Key;
                if (string.IsNullOrEmpty(fieldName))
                    errors.Add(new Exception($"Property {property.Name} has an empty bnf key. Bnf key is required to on a legit bnf property"));

                var fieldValue = property.GetValue(obj);
                if (attribute.Required && fieldValue == null)
                    errors.Add(new Exception($"Detected null value for required bnf field - {property.Name}"));
            }

            return !errors.Any();
        }
    }
}
