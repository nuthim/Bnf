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
        private static IEnumerable<BnfPropertyMap> mappings;
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
        public void NoTimeSpanFormatTest()
        {
            //No format specified hence general ToString() format must be used

            var result = serializer.Serialize(timeSpanObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(timeSpanObj.SettingsFormatTime));
            var bnfField = mapping.Attribute.Key;

            Assert.IsTrue(result.Contains($"{bnfField}={timeNow.ToString()}"));
        }


        [TestMethod]
        public void ExplicitTimeFormatTest()
        {
            //No global format specified but format is specified explicitly on the property.

            var result = serializer.Serialize(timeSpanObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(timeSpanObj.ExplicitFormatTime));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = string.Format(mapping.Attribute.DataFormatString, timeNow);

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void SettingsTimeFormatTest()
        {
            //Global format is set but format is not set explicitly on the property

            serializer.Settings.SetFormatString(typeof(TimeSpan), "{0:ss\\:mm\\:hh}");
            var result = serializer.Serialize(timeSpanObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(timeSpanObj.SettingsFormatTime));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = string.Format(serializer.Settings.GetFormatString(typeof(TimeSpan)), timeNow);

            serializer.Settings.SetFormatString(typeof(TimeSpan), null);
            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void TimeSpanPrecedenceTest()
        {
            //Format set both globally and on the property attribute. In that case the property setting should take effect

            serializer.Settings.SetFormatString(typeof(TimeSpan), "{0:mm\\:ss\\:hh}");
            var result = serializer.Serialize(timeSpanObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(timeSpanObj.ExplicitFormatTime));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = string.Format(mapping.Attribute.DataFormatString, timeNow);

            serializer.Settings.SetFormatString(typeof(TimeSpan), null);
            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

    }

    public class TimeSpanObj
    {
        [BnfProperty(Key = "default_format_time")]
        public TimeSpan DefaultFormatTime { get; set; }

        [BnfProperty(Key = "explicit_format_time", DataFormatString = "{0:ss\\:mm\\:hh}")]
        public TimeSpan ExplicitFormatTime { get; set; }

        [BnfProperty(Key = "settings_format_time")]
        public TimeSpan SettingsFormatTime { get; set; }
    }
}
