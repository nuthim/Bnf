using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization;
using System.Collections.Generic;

namespace Bnf.Tests
{
    [TestClass]
    public class StringSerializationTests
    {
        private static IEnumerable<BnfFieldMap> mappings;
        private static StringObj stringObj;
        private static BnfSerializer serializer;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            stringObj = new StringObj { ShortName = "Mi}th{u|n;[]" };
            mappings = new BnfFieldMappingFactory().GetBnfFieldMappings(stringObj);
            serializer = new BnfSerializer();
        }

        [TestMethod]
        public void DefaultEscapeTest()
        {
            var result = serializer.Serialize(stringObj);
            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.ShortName));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = "Mi}}th{{u||n;[]";

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void AddonEscapeTest()
        {
            serializer.Settings.EscapeCodes.Add(';', "[]");
            var result = serializer.Serialize(stringObj);
            
            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.ShortName));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = "Mi}}th{{u||n[][]";

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void ExplicitNullTextTest()
        {
            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameSet));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = mapping.Attribute.NullText;

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void SettingsNullTextUnsetTest()
        {
            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameUnset));
            var bnfField = mapping.Attribute.Key;
            Assert.IsFalse(result.Contains($"{bnfField}"));
        }

        [TestMethod]
        public void SettingsNullTextSetTest()
        {
            serializer.Settings.NullText = string.Empty;
            var result = serializer.Serialize(stringObj);
            
            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameUnset));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = serializer.Settings.NullText;

            Assert.IsTrue(result.Contains($"{bnfField}= "));
            serializer.Settings.NullText = null;
        }


        [TestMethod]
        public void NullTextPrecedenceTest()
        {
            serializer.Settings.NullText = "[None]";
            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameSet));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = mapping.Attribute.NullText;

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }
    }


    public class StringObj
    {
        [Bnf(Key = "short_name")]
        public string ShortName { get; set; }

        [Bnf(Key = "null_name_set", NullText = "n/a")]
        public string NullNameSet { get; set; }

        [Bnf(Key = "null_name_unset")]
        public string NullNameUnset { get; set; }
    }
}
