using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Bnf.Serialization.Infrastructure
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

            //TODO: Report multiple properties with the same bnf key on a given type

            foreach (var metadata in metaFactory.GetPropertyMetaData(obj))
            {
                if (metadata.Property.GetIndexParameters().Any())
                    continue;

                var isEnumerable = typeof(IEnumerable).IsAssignableFrom(metadata.Property.PropertyType);

                var fieldValue = metadata.Property.GetValue(obj);
                if (metadata.IsRequired && fieldValue == null)
                {
                    if (metadata.NullText == null && settings.NullText == null)
                        errors.Add(new Exception($"Detected null value for required bnf field - {metadata.Property.Name}"));
                }

                //if (bnfPropertyAttribute != null)
                //{
                //    if (!metadata.IsReadWriteProperty)
                //        errors.Add(new Exception($"{bnfPropertyAttribute.GetType().Name} is applied to non-readwrite property: {metadata.Property.Name}"));
                //    else
                //    {
                        
                //    }
                //}
            }

            return !errors.Any();
        }
    }
}
