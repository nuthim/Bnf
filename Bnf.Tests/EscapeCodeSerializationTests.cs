using System.Linq;
using System.Collections.Generic;
using Bnf.Serialization;
using Bnf.Serialization.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Bnf.Tests
{
    [TestClass]
    public class EscapeCodeSerializationTests
    {
        private static IEnumerable<PropertyMetaData> mappings;
        private static StringObj stringObj;
        private static BnfSerializer serializer;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            stringObj = new StringObj { ShortName = @"Mi}[[|\/]]}th{=u\/||n;[]" };
            mappings = new PropertyMetaDataFactory().GetPropertyMetaData(stringObj);
            serializer = new BnfSerializer();
        }

        [TestMethod]
        public void EscapeTest()
        {
            var result = serializer.Serialize(stringObj);
            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.ShortName));
            var bnfField = mapping.KeyName;
            var expectedValue = @"Mi\][[\/\\/]]\]th\[=u\\/\/\/n;[]";
            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));

            var deserialized = serializer.Deserialize<StringObj>(result);
            Assert.AreEqual(stringObj.ShortName, deserialized.ShortName);
        }
    }
}
