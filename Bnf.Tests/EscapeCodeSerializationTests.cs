using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Bnf.Tests
{
    [TestClass]
    public class EscapeCodeSerializationTests
    {
        private static IEnumerable<BnfPropertyMap> mappings;
        private static StringObj stringObj;
        private static BnfSerializer serializer;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            stringObj = new StringObj { ShortName = "Mi}th{u|n;[]" };
            mappings = new BnfFieldMappingFactory().GetBnfFieldMappings(stringObj);
            serializer = new BnfSerializer();
        }

        [TestMethod]
        public void DefaultEscapeTest()
        {
            //Test default escape codes for }, {, |

            var result = serializer.Serialize(stringObj);
            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.ShortName));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = "Mi}}th{{u||n;[]";

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }

        [TestMethod]
        public void AddonEscapeTest()
        {
            //Test default escape codes for }, {, | along with ;

            serializer.Settings.SetEscapeCode(';', "[]");
            var result = serializer.Serialize(stringObj);

            var mapping = mappings.Single(x => x.Property.Name == nameof(stringObj.ShortName));
            var bnfField = mapping.Attribute.Key;
            var expectedValue = "Mi}}th{{u||n[][]";

            Assert.IsTrue(result.Contains($"{bnfField}={expectedValue}"));
        }
    }
}
