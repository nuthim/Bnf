using System.Linq;
using System.Collections.Generic;
using Bnf.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Bnf.Tests
{
    [TestClass]
    public class InclusionSerializationTests
    {
        private static IEnumerable<PropertyMetaData> mappings;
        private static InclusionObj inclusionObj;
        private static BnfSerializer serializer;


        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            inclusionObj = new InclusionObj { Id = 100, FirstName = "Mithun", LastName = "Basak", Password = "password" };
            mappings = new PropertyMetaDataFactory().GetPropertyMetaData(inclusionObj);
            serializer = new BnfSerializer();
        }

        [TestMethod]
        public void BnfExludeTest()
        {
            var result = serializer.Serialize(inclusionObj);

            //Read-Write properties marked to be explicity ignored shouldn't be part of serialization
            var mapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(inclusionObj.Password));
            Assert.IsFalse(result.Contains(mapping.Property.Name));

            //Only Read-Write properties should be part of serialization
            mapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(inclusionObj.GetterOnly));
            Assert.IsFalse(result.Contains(mapping.Property.Name));
        }


        [TestMethod]
        public void BnfIncludeTest()
        {
            var result = serializer.Serialize(inclusionObj);

            //Properties not having a key name defined should use the property name 
            var idMapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(inclusionObj.Id));
            Assert.IsTrue(idMapping != null);
            Assert.IsTrue(result.Contains($"{idMapping.Property.Name}={inclusionObj.Id}"));

            //Properties decorated with bnf attribute and key name should use the key 
            var firstMapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(inclusionObj.FirstName));
            Assert.IsTrue(firstMapping != null);
            Assert.IsTrue(result.Contains($"{firstMapping.CustomBnfPropertyAttribute.Key}={inclusionObj.FirstName}"));

            //Properties decorated with bnf attribute but no key name should use property name 
            var lastMapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(inclusionObj.LastName));
            Assert.IsTrue(lastMapping != null);
            Assert.IsTrue(result.Contains($"{lastMapping.Property.Name}={inclusionObj.LastName}"));
        }


        public class InclusionObj
        {
            public int Id { get; set; }

            [BnfProperty(Key = "first_name")]
            public string FirstName { get; set; }

            [BnfProperty]
            public string LastName { get; set; }

            public string FullName => $"{FirstName} {LastName}";

            [BnfIgnore]
            public string Password { get; set; }

            public string GetterOnly => "Getter only";
        }
    }
}
