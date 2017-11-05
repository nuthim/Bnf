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
        private static IEnumerable<PropertyMetaData> mappings;
        private static DateTimeObj dateTimeObj;
        private static BnfSerializer serializer;
        private static DateTime dateNow = DateTime.Now;

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
            var bnfField = mapping.CustomBnfPropertyAttribute.Key;

            var r = serializer.Deserialize<DateTimeObj>(result);
            Assert.IsTrue(result.Contains($"{bnfField}={dateNow.ToString()}"));
        }


        [TestMethod]
        public void ExplicitDateFormatTest()
        {
            //No global format specified but format is specified explicitly on the property.
            var result = serializer.Serialize(dateTimeObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(dateTimeObj.ExplicitFormatDate));
            var bnfField = mapping.CustomBnfPropertyAttribute.Key;
            var expectedValue = string.Format(mapping.CustomBnfPropertyAttribute.DataFormatString, dateNow);

            var r = serializer.Deserialize<DateTimeObj>(result);
            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void SettingsDateFormatTest()
        {
            //Global format is set but format is not set explicitly on the property

            serializer.Settings.SetFormatString(typeof(DateTime), "{0:dd MM yyyy}");
            var result = serializer.Serialize(dateTimeObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(dateTimeObj.SettingsFormatDate));
            var bnfField = mapping.CustomBnfPropertyAttribute.Key;
            var expectedValue = string.Format(serializer.Settings.GetFormatString(typeof(DateTime)), dateNow);

            serializer.Settings.SetFormatString(typeof(DateTime), null);
            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void DateTimePrecedenceTest()
        {
            //Format set both globally and on the property attribute. In that case the property setting should take effect

            serializer.Settings.SetFormatString(typeof(DateTime), "{0:dd MM yyyy}");
            var result = serializer.Serialize(dateTimeObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(dateTimeObj.ExplicitFormatDate));
            var bnfField = mapping.CustomBnfPropertyAttribute.Key;
            var expectedValue = string.Format(mapping.CustomBnfPropertyAttribute.DataFormatString, dateNow);

            serializer.Settings.SetFormatString(typeof(DateTime), null);
            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }
    }

    public class DateTimeObj
    {
        [BnfProperty(Key = "default_format_date")]
        public DateTime DefaultFormatDate { get; set; }

        [BnfProperty(Key = "explicit_format_date", DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime ExplicitFormatDate { get; set; }

        [BnfProperty(Key = "settings_format_date")]
        public DateTime SettingsFormatDate { get; set; }
    }
}
