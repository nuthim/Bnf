using System;
using System.Linq;
using System.Collections.Generic;


namespace Bnf.Serialization
{
    public class BnfValidator
    {
        /// <summary>
        /// Validates if the attributes have been applied correctly and specified object can be serialized
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="settings"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public bool Validate(object obj, BnfSettings settings, out List<Exception> errors)
        {
            if (obj == null)
                throw  new ArgumentNullException(nameof(obj));

            errors = new List<Exception>();
            var metaFactory = new PropertyMetaDataFactory();

            foreach (var metadata in metaFactory.GetPropertyMetaData(obj))
            {
                var bnfPropertyAttribute = metadata.CustomBnfPropertyAttribute;

                if (bnfPropertyAttribute != null)
                {
                    if (metadata.CustomBnfIgnoreAttribute != null)
                        errors.Add(new Exception($"{bnfPropertyAttribute.GetType().Name}, {metadata.CustomBnfIgnoreAttribute.GetType().Name} can't be applied to the same property: {metadata.Property.Name}"));

                    if (!metadata.IsReadWriteProperty)
                        errors.Add(new Exception($"{bnfPropertyAttribute.GetType().Name} is applied to non-readwrite property: {metadata.Property.Name}"));
                    else
                    {
                        var fieldValue = metadata.Property.GetValue(obj);
                        if (bnfPropertyAttribute.Required && fieldValue == null)
                        {
                            if (bnfPropertyAttribute.NullText == null && settings.NullText == null)
                                errors.Add(new Exception($"Detected null value for required bnf field - {metadata.Property.Name}"));
                        }
                    }
                }
            }

            return !errors.Any();
        }
    }
}
