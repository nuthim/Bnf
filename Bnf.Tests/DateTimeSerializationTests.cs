using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization;
using System.Collections.Generic;

namespace Bnf.Tests
{
    [TestClass]
    public class DateTimeSerializationTests
    {
        private static IEnumerable<BnfFieldMap> mappings;
        private static DateTimeObj dateTimeObj;
        private static BnfSerializer serializer;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            dateTimeObj = new DateTimeObj { DefaultFormatDate = DateTime.Now, ExplicitFormatDate = DateTime.Now, SettingsFormatDate = DateTime.Now };
            mappings = new BnfFieldMappingFactory().GetBnfFieldMappings(dateTimeObj);
            serializer = new BnfSerializer();
        }

        [TestMethod]
        public void DefaultDateFormatTest()
        {
            var result = serializer.Serialize(dateTimeObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(dateTimeObj.DefaultFormatDate));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = string.Format(serializer.Settings.DateFormat, DateTime.Now);

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void ExplicitDateFormatTest()
        {
            var result = serializer.Serialize(dateTimeObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(dateTimeObj.ExplicitFormatDate));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = string.Format(mapping.Attribute.DataFormatString, DateTime.Now);

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void SettingsDateFormatTest()
        {
            serializer.Settings.DateFormat = "{0:dd MM yyyy}";
            var result = serializer.Serialize(dateTimeObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(dateTimeObj.SettingsFormatDate));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = string.Format(serializer.Settings.DateFormat, DateTime.Now);

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }
    }

    public class DateTimeObj
    {
        [Bnf(Key = "default_format_date")]
        public DateTime DefaultFormatDate { get; set; }

        [Bnf(Key = "explicit_format_date", DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime ExplicitFormatDate { get; set; }

        [Bnf(Key = "settings_format_date")]
        public DateTime SettingsFormatDate { get; set; }
    }
}
