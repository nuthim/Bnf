using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization;
using Bnf.Serialization.Attributes;
using System.Collections.Generic;
using Bnf.Serialization.Infrastructure;
using System.Runtime.Serialization;


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
            //Null Text set globally to null (default) but is set to some value on property attribute. In that case the property setting should take effect

            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameSet));
            var bnfField = mapping.KeyName;
            var expectedValue = mapping.NullText;

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }


        [TestMethod]
        public void SettingsNullTextUnsetTest()
        {
            //Null Text neither set globally nor on the property attribute. In that case the property should be ignored for serialization

            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameUnset));
            var bnfField = mapping.KeyName;
            Assert.IsFalse(result.Contains($"{bnfField}"));
        }

        [TestMethod]
        public void SettingsNullTextSetTest()
        {
            //Null Text set globally but not on the property attribute. In that case the global setting should take effect

            serializer.Settings.NullText = "[None]";
            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameUnset));
            var bnfField = mapping.KeyName;
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
            var bnfField = mapping.KeyName;
            var expectedValue = mapping.NullText;

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void ExplicitNullAllTest()
        {
            //Null Text set to null both globally and on property attribute. In that case the property should be ignored for serialization

            serializer.Settings.NullText = null;
            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.NullNameSetNull));
            var bnfField = mapping.KeyName;

            Assert.IsFalse(result.Contains($"{bnfField}"));
        }
    }


    public class StringObj
    {
        [DataMember(Name = "short_name")]
        public string ShortName { get; set; }

        [DataMember(Name = "null_name_set")]
        [DataFormat(NullText = "n/a")]
        public string NullNameSet { get; set; }

        [DataMember(Name = "null_name_unset")]
        public string NullNameUnset { get; set; }

        [DataMember(Name = "null_name_set_null")]
        [DataFormat(NullText = null)]
        public string NullNameSetNull { get; set; }
    }
}
