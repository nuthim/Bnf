using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization;
using System.Runtime.Serialization;


namespace Bnf.Tests
{
    [TestClass]
    public class EnumSerializationTests
    {
        private static BnfSerializer serializer;
        private static EnumObj enumObj;


        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            serializer = new BnfSerializer();
            enumObj = new EnumObj { MaleGender = Gender.Male, FemaleGender = Gender.Female, NoneGender = Gender.None };
        }

        [TestMethod]
        public void EnumTest()
        {
            var result = serializer.Serialize(enumObj);

            Assert.IsTrue(result.Contains("MaleGender=Gentleman"));
            Assert.IsTrue(result.Contains("female=Lady"));
            Assert.IsTrue(result.Contains("NoneGender=None"));

            var deserialized = serializer.Deserialize<EnumObj>(result);
            Assert.IsTrue(deserialized.Match(enumObj));
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
