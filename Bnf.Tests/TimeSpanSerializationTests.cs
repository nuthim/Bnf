using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization;
using System.Collections.Generic;

namespace Bnf.Tests
{
    [TestClass]
    public class TimeSpanSerializationTests
    {
        private static IEnumerable<BnfFieldMap> mappings;
        private static TimeSpanObj timeSpanObj;
        private static BnfSerializer serializer;
        private static TimeSpan timeNow = DateTime.Now.TimeOfDay;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            timeSpanObj = new TimeSpanObj { DefaultFormatTime = timeNow, ExplicitFormatTime = timeNow, SettingsFormatTime = timeNow };
            mappings = new BnfFieldMappingFactory().GetBnfFieldMappings(timeSpanObj);
            serializer = new BnfSerializer();
        }

        [TestMethod]
        public void DefaultTimeFormatTest()
        {
            var result = serializer.Serialize(timeSpanObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(timeSpanObj.DefaultFormatTime));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = string.Format(serializer.Settings.TimeFormat, timeNow);

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void ExplicitTimeFormatTest()
        {
            var result = serializer.Serialize(timeSpanObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(timeSpanObj.ExplicitFormatTime));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = string.Format(mapping.Attribute.DataFormatString, timeNow);

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void SettingsTimeFormatTest()
        {
            serializer.Settings.TimeFormat = "{0:ss\\:mm\\:hh}";
            var result = serializer.Serialize(timeSpanObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(timeSpanObj.SettingsFormatTime));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = string.Format(serializer.Settings.TimeFormat, timeNow);

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }
    }

    public class TimeSpanObj
    {
        [Bnf(Key = "default_format_time")]
        public TimeSpan DefaultFormatTime { get; set; }

        [Bnf(Key = "explicit_format_time", DataFormatString = "{0:ss\\:mm\\:hh}")]
        public TimeSpan ExplicitFormatTime { get; set; }

        [Bnf(Key = "settings_format_time")]
        public TimeSpan SettingsFormatTime { get; set; }
    }
}
