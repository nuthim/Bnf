using System.Linq;
using System.Collections.Generic;
using Bnf.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Bnf.Tests
{
    [TestClass]
    public class MappingSerializationTests
    {
        private static IEnumerable<BnfPropertyMap> mappings;
        private static PartialObj partialObj;
        private static BnfSerializer serializer;


        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            partialObj = new PartialObj { Id = 100, FirstName = "Mithun", LastName = "Basak", Password = "password" };
            mappings = new BnfFieldMappingFactory().GetBnfFieldMappings(partialObj);
            serializer = new BnfSerializer();
        }

        [TestMethod]
        public void BnfIgnoreTest()
        {
            var result = serializer.Serialize(partialObj);

            //Read-Write properties marked to be explicity ignored shouldn't be part of serialization
            var mapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(partialObj.Password));
            Assert.IsTrue(mapping == null);

            //Properties having both getter and setter should be part of serialization
            mapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(partialObj.FullName));
            Assert.IsTrue(mapping == null);
        }

        [TestMethod]
        public void BnfIncludeTest()
        {
            var result = serializer.Serialize(partialObj);

            //In this case only 3 properties are valid for serialization
            Assert.IsTrue(mappings.Count() == 3);

            //Properties not having a key name defined should use the property name 
            var idMapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(partialObj.Id));
            Assert.IsTrue(idMapping != null);
            Assert.IsTrue(result.Contains($"{idMapping.Property.Name}={partialObj.Id}"));

            //Properties decorated with bnf attribute and key name should use the key 
            var firstMapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(partialObj.FirstName));
            Assert.IsTrue(firstMapping != null);
            Assert.IsTrue(result.Contains($"{firstMapping.Attribute.Key}={partialObj.FirstName}"));

            //Properties decorated with bnf attribute but no key name should use property name 
            var lastMapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(partialObj.LastName));
            Assert.IsTrue(lastMapping != null);
            Assert.IsTrue(result.Contains($"{lastMapping.Property.Name}={partialObj.LastName}"));
        }

        public class PartialObj
        {
            public int Id { get; set; }

            [BnfProperty(Key = "first_name")]
            public string FirstName { get; set; }

            [BnfProperty]
            public string LastName { get; set; }

            [BnfProperty(Key = "full_name")]
            public string FullName => $"{FirstName} {LastName}";

            [BnfIgnore]
            public string Password { get; set; }
        }
    }
}
