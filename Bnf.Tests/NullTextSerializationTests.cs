using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization;
using System.Collections.Generic;

namespace Bnf.Tests
{
    [TestClass]
    public class NullTextSerializationTests
    {
        private static IEnumerable<PropertyMetaData> mappings;
        private static StringObj stringObj;
        private static BnfSerializer serializer;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            stringObj = new StringObj { ShortName = "Mi}th{u|n;[]" };
            mappings = new PropertyMetaDataFactory().GetPropertyMetaData(stringObj);
            serializer = new BnfSerializer();
        }


        [TestMethod]
        public void ExplicitNullTextTest()
        {
            //Null Text set both globally to null (default) but is set to some value on property attribute. In that case the property setting should take effect

            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameSet));
            var bnfField = mapping.CustomBnfPropertyAttribute.Key;
            var expectedValue = mapping.CustomBnfPropertyAttribute.NullText;

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void SettingsNullTextUnsetTest()
        {
            //Null Text neither set globally nor on the property attribute. In that case the property should be ignored for serialization

            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameUnset));
            var bnfField = mapping.CustomBnfPropertyAttribute.Key;
            Assert.IsFalse(result.Contains($"{bnfField}"));
        }

        [TestMethod]
        public void SettingsNullTextSetTest()
        {
            //Null Text set globally but not on the property attribute. In that case the global setting should take effect

            serializer.Settings.NullText = "[None]";
            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameUnset));
            var bnfField = mapping.CustomBnfPropertyAttribute.Key;
            var expectedValue = serializer.Settings.NullText;

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
            serializer.Settings.NullText = null;
        }


        [TestMethod]
        public void NullTextPrecedenceTest()
        {
            //Null Text set both globally and on the property attribute. In that case the property setting should take effect

            serializer.Settings.NullText = "[None]";
            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameSet));
            var bnfField = mapping.CustomBnfPropertyAttribute.Key;
            var expectedValue = mapping.CustomBnfPropertyAttribute.NullText;

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void ExplicitNullAllTest()
        {
            //Null Text set to null both globally and on property attribute. In that case the property should be ignored for serialization

            serializer.Settings.NullText = null;
            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameSetNull));
            var bnfField = mapping.CustomBnfPropertyAttribute.Key;

            Assert.IsFalse(result.Contains($"{bnfField}"));
        }
    }


    public class StringObj
    {
        [BnfProperty(Key = "short_name")]
        public string ShortName { get; set; }

        [BnfProperty(Key = "null_name_set", NullText = "n/a")]
        public string NullNameSet { get; set; }

        [BnfProperty(Key = "null_name_unset")]
        public string NullNameUnset { get; set; }

        [BnfProperty(Key = "null_name_set_null", NullText = null)]
        public string NullNameSetNull { get; set; }
    }
}
