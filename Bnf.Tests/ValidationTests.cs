using System.Linq;
using System.Collections.Generic;
using Bnf.Serialization;
using Bnf.Serialization.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization.Exceptions;
using Bnf.Serialization.Infrastructure;

namespace Bnf.Tests
{
    [TestClass]
    public class ValidationTests
    {
        private static IEnumerable<PropertyMetaData> mappings;
        private static ValidationObj validationObj;
        private static BnfSerializer serializer;


        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            validationObj = new ValidationObj { LastName = "Basak" };
            mappings = new PropertyMetaDataFactory().GetPropertyMetaData(validationObj);
            serializer = new BnfSerializer();
        }

        [TestMethod]
        [ExpectedException(typeof(BnfValidationException))]
        public void BnfRequiredTest()
        {
            var lastName = validationObj.LastName;
            validationObj.LastName = null;
            try
            {
                var result = serializer.Serialize(validationObj);
            }
            finally
            {
                validationObj.LastName = lastName;
            }
        }


        [TestMethod]
        public void BnfNotRequiredTest()
        {
            var firstName = validationObj.FirstName;
            validationObj.FirstName = null;
            try
            {
                var result = serializer.Serialize(validationObj);
                var firstMapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(validationObj.FirstName));
                Assert.IsFalse(result.Contains($"{firstMapping.Property.Name}"));
            }
            finally
            {
                validationObj.FirstName = firstName;
            }
        }

        public class ValidationObj
        {
            public string FirstName { get; set; }

            [BnfProperty(Required = true)]
            public string LastName { get; set; }
        }
    }
}
