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
                if (attribute == null)
                    continue;

                var property = map.Property;

                var fieldValue = property.GetValue(obj);
                if (attribute.Required && fieldValue == null)
                    errors.Add(new Exception($"Detected null value for required bnf field - {property.Name}"));
            }

            return !errors.Any();
        }
    }
}
