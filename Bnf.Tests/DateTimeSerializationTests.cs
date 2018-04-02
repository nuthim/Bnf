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
    public class DateTimeSerializationTests
    {
        private static IEnumerable<PropertyMetaData> mappings;
        private static DateTimeObj dateTimeObj;
        private static BnfSerializer serializer;
        private static DateTime dateNow = DateTime.Today;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            dateTimeObj = new DateTimeObj { DefaultFormatDate = dateNow, ExplicitFormatDate = dateNow, SettingsFormatDate = dateNow };
            mappings = new PropertyMetaDataFactory().GetPropertyMetaData(dateTimeObj);
            serializer = new BnfSerializer();
        }

        [TestMethod]
        public void NoDateTimeFormatTest()
        {
            //No format specified hence general ToString() format must be used

            var result = serializer.Serialize(dateTimeObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(dateTimeObj.SettingsFormatDate));
            var bnfField = mapping.KeyName;

            Assert.IsTrue(result.Contains($"{bnfField}={dateNow.ToString()}"));

            var deserialized = serializer.Deserialize<DateTimeObj>(result);
            Assert.IsTrue(deserialized.Match(dateTimeObj));
        }

        [TestMethod]
        public void ExplicitDateFormatTest()
        {
            //No global format specified but format is specified explicitly on the property.
            var result = serializer.Serialize(dateTimeObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(dateTimeObj.ExplicitFormatDate));
            var bnfField = mapping.KeyName;
            var expectedValue = dateNow.ToString(mapping.DataFormatString);

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));

            var deserialized = serializer.Deserialize<DateTimeObj>(result);
            Assert.IsTrue(deserialized.Match(dateTimeObj));
        }

        [TestMethod]
        public void SettingsDateFormatTest()
        {
            //Global format is set but format is not set explicitly on the property

            serializer.Settings.SetFormatString(typeof(DateTime), "dd MM yyyy");
            var result = serializer.Serialize(dateTimeObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(dateTimeObj.SettingsFormatDate));
            var bnfField = mapping.KeyName;
            var expectedValue = dateNow.ToString(serializer.Settings.GetFormatString(typeof(DateTime)));

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));

            var deserialized = serializer.Deserialize<DateTimeObj>(result);
            Assert.IsTrue(deserialized.Match(dateTimeObj));

            serializer.Settings.SetFormatString(typeof(DateTime), null);
        }

        [TestMethod]
        public void DateTimePrecedenceTest()
        {
            //Format set both globally and on the property attribute. In that case the property setting should take effect

            serializer.Settings.SetFormatString(typeof(DateTime), "dd MM yyyy");
            var result = serializer.Serialize(dateTimeObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(dateTimeObj.ExplicitFormatDate));
            var bnfField = mapping.KeyName;
            var expectedValue = dateNow.ToString(mapping.DataFormatString);

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));

            var deserialized = serializer.Deserialize<DateTimeObj>(result);
            Assert.IsTrue(deserialized.Match(dateTimeObj));

            serializer.Settings.SetFormatString(typeof(DateTime), null);
        }

        [TestMethod]
        public void DontEmitDefaultTest()
        {
            //Defaults for Value types will not be serialized if EmitDefaultValue = false

            var result = serializer.Serialize(dateTimeObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(dateTimeObj.DontEmitDefault));
            var bnfField = mapping.KeyName;

            Assert.IsFalse(result.Contains($"{bnfField}"));

            var deserialized = serializer.Deserialize<DateTimeObj>(result);
            Assert.IsTrue(deserialized.Match(dateTimeObj));
        }
    }

    public class DateTimeObj
    {
        [DataMember(Name = "default_format_date")]
        public DateTime DefaultFormatDate { get; set; }

        [DataMember(Name = "explicit_format_date")]
        [DataFormat(DataFormatString = "dd/MM/yyyy")]
        public DateTime ExplicitFormatDate { get; set; }

        [DataMember(Name = "settings_format_date")]
        public DateTime SettingsFormatDate { get; set; }

        [DataMember(Name = "dont_emit_default", EmitDefaultValue = false)]
        public DateTime DontEmitDefault { get; set; }
    }
}
