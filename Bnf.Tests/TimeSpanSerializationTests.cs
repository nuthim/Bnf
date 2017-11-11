using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization;
using System.Collections.Generic;
using Bnf.Serialization.Attributes;
using Bnf.Serialization.Infrastructure;
using System.Runtime.Serialization;

namespace Bnf.Tests
{
    [TestClass]
    public class TimeSpanSerializationTests
    {
        private static IEnumerable<PropertyMetaData> mappings;
        private static TimeSpanObj timeSpanObj;
        private static BnfSerializer serializer;
        private static TimeSpan timeNow = DateTime.Now.TimeOfDay;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            timeSpanObj = new TimeSpanObj { DefaultFormatTime = timeNow, ExplicitFormatTime = timeNow, SettingsFormatTime = timeNow };
            mappings = new PropertyMetaDataFactory().GetPropertyMetaData(timeSpanObj);
            serializer = new BnfSerializer();
        }

        [TestMethod]
        public void NoTimeSpanFormatTest()
        {
            //No format specified hence general ToString() format must be used

            var result = serializer.Serialize(timeSpanObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(timeSpanObj.SettingsFormatTime));
            var bnfField = mapping.KeyName;

            Assert.IsTrue(result.Contains($"{bnfField}={timeNow.ToString()}"));

            var deserialized = serializer.Deserialize<TimeSpanObj>(result);
            Assert.AreEqual(timeSpanObj, deserialized);
        }


        [TestMethod]
        public void ExplicitTimeFormatTest()
        {
            //No global format specified but format is specified explicitly on the property.

            var result = serializer.Serialize(timeSpanObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(timeSpanObj.ExplicitFormatTime));
            var bnfField = mapping.KeyName;
            var expectedValue = timeNow.ToString(mapping.DataFormatString);

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));

            var deserialized = serializer.Deserialize<TimeSpanObj>(result);
            Assert.AreEqual(timeSpanObj, deserialized);
        }

        [TestMethod]
        public void SettingsTimeFormatTest()
        {
            //Global format is set but format is not set explicitly on the property

            serializer.Settings.SetFormatString(typeof(TimeSpan), "ss\\:mm\\:hh");
            var result = serializer.Serialize(timeSpanObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(timeSpanObj.SettingsFormatTime));
            var bnfField = mapping.KeyName;
            var expectedValue = timeNow.ToString(serializer.Settings.GetFormatString(typeof(TimeSpan)));

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));

            var deserialized = serializer.Deserialize<TimeSpanObj>(result);
            Assert.AreEqual(timeSpanObj, deserialized);

            serializer.Settings.SetFormatString(typeof(TimeSpan), null);
        }

        [TestMethod]
        public void TimeSpanPrecedenceTest()
        {
            //Format set both globally and on the property attribute. In that case the property setting should take effect

            serializer.Settings.SetFormatString(typeof(TimeSpan), "mm\\:ss\\:hh");
            var result = serializer.Serialize(timeSpanObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(timeSpanObj.ExplicitFormatTime));
            var bnfField = mapping.KeyName;
            var expectedValue = timeNow.ToString(mapping.DataFormatString);

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));

            var deserialized = serializer.Deserialize<TimeSpanObj>(result);
            Assert.AreEqual(timeSpanObj, deserialized);

            serializer.Settings.SetFormatString(typeof(TimeSpan), null);
        }

    }

    public class TimeSpanObj
    {
        [DataMember(Name = "default_format_time")]
        public TimeSpan DefaultFormatTime { get; set; }

        [DataMember(Name = "explicit_format_time")]
        [DataFormat(DataFormatString = "ss\\:mm\\:hh")]
        public TimeSpan ExplicitFormatTime { get; set; }

        [DataMember(Name = "settings_format_time")]
        public TimeSpan SettingsFormatTime { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as TimeSpanObj;
            if (other == null)
                return false;

            return
                DefaultFormatTime.ToString("hh\\:mm\\:ss") == other.DefaultFormatTime.ToString("hh\\:mm\\:ss") &&
                ExplicitFormatTime.ToString("hh\\:mm\\:ss") == other.ExplicitFormatTime.ToString("hh\\:mm\\:ss") &&
                SettingsFormatTime.ToString("hh\\:mm\\:ss") == other.SettingsFormatTime.ToString("hh\\:mm\\:ss");
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
