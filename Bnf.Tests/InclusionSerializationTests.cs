using System.Linq;
using System.Collections.Generic;
using Bnf.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization.Infrastructure;
using System.Runtime.Serialization;

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

            //Properties not a data member should use the property name 
            var idMapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(inclusionObj.Id));
            Assert.IsTrue(idMapping != null);
            Assert.IsFalse(result.Contains($"{idMapping.Property.Name}={inclusionObj.Id}"));


            //Read-Write properties marked to be explicity ignored shouldn't be part of serialization
            var mapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(inclusionObj.Password));
            Assert.IsFalse(result.Contains(mapping.Property.Name));

            //Only Read-Write properties should be part of serialization
            mapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(inclusionObj.GetterOnly));
            Assert.IsFalse(result.Contains(mapping.Property.Name));

            var deserialized = serializer.Deserialize<InclusionObj>(result);
            Assert.IsTrue(deserialized.Match(inclusionObj));
        }


        [TestMethod]
        public void BnfIncludeTest()
        {
            var result = serializer.Serialize(inclusionObj);

            //Properties decorated with DataMember attribute and name should use the name 
            var firstMapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(inclusionObj.FirstName));
            Assert.IsTrue(firstMapping != null);
            Assert.IsTrue(result.Contains($"{firstMapping.KeyName}={inclusionObj.FirstName}"));

            //Properties decorated with DataMember attribute but no  name should use property name 
            var lastMapping = mappings.SingleOrDefault(x => x.Property.Name == nameof(inclusionObj.LastName));
            Assert.IsTrue(lastMapping != null);
            Assert.IsTrue(result.Contains($"{lastMapping.Property.Name}={inclusionObj.LastName}"));

            var deserialized = serializer.Deserialize<InclusionObj>(result);
            Assert.IsTrue(deserialized.Match(inclusionObj));
        }


        public class InclusionObj
        {
            public int Id { get; set; }

            [DataMember(Name = "first_name")]
            public string FirstName { get; set; }

            [DataMember]
            public string LastName { get; set; }

            public string FullName => $"{FirstName} {LastName}";

            [IgnoreDataMember]
            public string Password { get; set; }

            public string GetterOnly => "Getter only";
        }
    }
}
