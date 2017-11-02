using System;
using Bnf.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bnf.Tests
{
    [TestClass]
    public class ArraySerializationTests
    {
        private static BnfSerializer serializer;


        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            serializer = new BnfSerializer();
        }

        [TestMethod]
        public void ArrayNoContainerTest()
        {
            var array = new[] { new ItemA { Id = 1, Name = "A" }, new ItemA { Id = 2, Name = "B" } };
            var result = serializer.Serialize(array);
            Assert.AreEqual("{ItemA={id=1 | name=A} | ItemA={id=2 | name=B}}", result, false);
        }

        [TestMethod]
        public void ArrayAsNoAttributePropertyTest()
        {
            var array = new[] { new ItemA { Id = 1, Name = "A" }, new ItemA { Id = 2, Name = "B" } };
            var itemCollection = new ItemCollection { NoAttributeItemArray = array };
            var result = serializer.Serialize(itemCollection);
            Assert.AreEqual("{NoAttributeItemArray={ItemA={id=1 | name=A} | ItemA={id=2 | name=B}}}", result, false);
        }

        [TestMethod]
        public void ArrayAsNamedIndexedPropertyTest()
        {
            var array = new[] { new ItemA { Id = 1, Name = "A" }, new ItemA { Id = 2, Name = "B" } };
            var itemCollection = new ItemCollection { NamedIndexedItemArray = array };
            var result = serializer.Serialize(itemCollection);
            Assert.AreEqual("{named_indexed_item_array={item1={id=1 | name=A} | item2={id=2 | name=B}}}", result, false);
        }

        [TestMethod]
        public void ArrayAsNamedNonIndexedPropertyTest()
        {
            var array = new[] { new ItemA { Id = 1, Name = "A" }, new ItemA { Id = 2, Name = "B" } };
            var itemCollection = new ItemCollection { NamedNonIndexedItemArray = array };
            var result = serializer.Serialize(itemCollection);
            Assert.AreEqual("{named_nonindexed_item_array={item={id=1 | name=A} | item={id=2 | name=B}}}", result, false);
        }

        [TestMethod]
        public void ArrayAsNonNamedNonIndexedPropertyTest()
        {
            var array = new[] { new ItemA { Id = 1, Name = "A" }, new ItemA { Id = 2, Name = "B" } };
            var itemCollection = new ItemCollection { NonNamedNonIndexedItemArray = array };
            var result = serializer.Serialize(itemCollection);
            Assert.AreEqual("{nonnamed_nonindexed_item_array={ItemA={id=1 | name=A} | ItemA={id=2 | name=B}}}", result, false);
        }

        [TestMethod]
        public void ArrayCollectionTest()
        {
            var array = new[] { new ItemA { Id = 1, Name = "A" }, new ItemA { Id = 2, Name = "B" } };
            var itemCollection = new ItemCollection
            {
                NoAttributeItemArray = array,
                NamedIndexedItemArray = array,
                NamedNonIndexedItemArray = array,
                NonNamedNonIndexedItemArray = array
            };
            var container = new ContainerObj { Collection = itemCollection };
            var result = serializer.Serialize(container);
            Assert.AreEqual("{item_collection={NoAttributeItemArray={ItemA={id=1 | name=A} | ItemA={id=2 | name=B}} | named_indexed_item_array={item1={id=1 | name=A} | item2={id=2 | name=B}} | named_nonindexed_item_array={item={id=1 | name=A} | item={id=2 | name=B}} | nonnamed_nonindexed_item_array={ItemA={id=1 | name=A} | ItemA={id=2 | name=B}}}}",
                result, false);
        }

        [TestMethod]
        public void ArrayHeterogeneousTest()
        {
            var array = new object[] { new ItemA { Id = 1, Name = "A" }, new ItemB { Income = 100.50m, DateOfBirth = new DateTime(1980, 7, 28) } };
            var result = serializer.Serialize(array);
            Assert.AreEqual("{ItemA={id=1 | name=A} | ItemB={income=100.50 | dob=28/07/1980}}", result, false);
        }
    }

    public class ContainerObj
    {
        [BnfProperty(Key = "item_collection")]
        public ItemCollection Collection { get; set; }
    }

    public class ItemCollection
    {
        public ItemA[] NoAttributeItemArray { get; set; }

        [BnfProperty(Key = "named_indexed_item_array", ElementName = "item", Indexed = true)]
        public ItemA[] NamedIndexedItemArray { get; set; }

        [BnfProperty(Key = "named_nonindexed_item_array", ElementName = "item")]
        public ItemA[] NamedNonIndexedItemArray { get; set; }

        [BnfProperty(Key = "nonnamed_nonindexed_item_array")]
        public ItemA[] NonNamedNonIndexedItemArray { get; set; }
    }

    public class ItemA
    {
        [BnfProperty(Key = "id")]
        public int Id { get; set; }

        [BnfProperty(Key = "name")]
        public string Name { get; set; }
    }

    public class ItemB
    {
        [BnfProperty(Key = "income")]
        public decimal Income { get; set; }

        [BnfProperty(Key = "dob", DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DateOfBirth { get; set; }
    }
}
