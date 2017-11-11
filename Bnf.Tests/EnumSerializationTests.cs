using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization;
using System.Runtime.Serialization;
using Bnf.Serialization.Infrastructure;
using System.Collections.Generic;


namespace Bnf.Tests
{
    [TestClass]
    public class EnumSerializationTests
    {
        private static BnfSerializer serializer;
        private static EnumObj enumObj;
        private static IEnumerable<PropertyMetaData> mappings;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            serializer = new BnfSerializer();
            enumObj = new EnumObj { MaleGender = Gender.Male, FemaleGender = Gender.Female, NoneGender = Gender.None };
            mappings = new PropertyMetaDataFactory().GetPropertyMetaData(enumObj);
        }

        [TestMethod]
        public void EnumTest()
        {
            var result = serializer.Serialize(enumObj);

            Assert.IsTrue(result.Contains("MaleGender=Gentleman"));
            Assert.IsTrue(result.Contains("female=Lady"));
            Assert.IsTrue(result.Contains("NoneGender=None"));

            var deserialized = serializer.Deserialize<EnumObj>(result);
            Assert.AreEqual(enumObj, deserialized);
        }
    }

    [DataContract(Namespace = "")]
    public class EnumObj
    {
        [DataMember]
        public Gender MaleGender { get; set; }

        [DataMember(Name = "female")]
        public Gender FemaleGender { get; set; }

        [DataMember]
        public Gender NoneGender { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as EnumObj;
            if (other == null)
                return false;

            return
                MaleGender == other.MaleGender &&
                FemaleGender == other.FemaleGender &&
                NoneGender == other.NoneGender;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [DataContract]
    public enum Gender
    {
        [EnumMember(Value = "Gentleman")]
        Male,

        [EnumMember(Value = "Lady")]
        Female,

        [EnumMember]
        None
    }
}
