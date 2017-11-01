using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization;
using System.Collections.Generic;

namespace Bnf.Tests
{
    [TestClass]
    public class ArraySerializationTests
    {
        private static IEnumerable<PropertyMetaData> mappings;
        private static BnfSerializer serializer;


        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            
        }

        [TestMethod]
        public void TestMethod1()
        {
            var array = new[] { new Item { Id = 1, Name = "A" }, new Item { Id = 2, Name = "B" } };

            serializer = new BnfSerializer();
            var result = serializer.Serialize(array);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var array = new[] { new Item { Id = 1, Name = "A" }, new Item { Id = 2, Name = "B" } };
            var itemCollection = new ItemCollection { Items = array };
            serializer = new BnfSerializer();
            var result = serializer.Serialize(itemCollection);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var array = new[] { new Item { Id = 1, Name = "A" }, new Item { Id = 2, Name = "B" } };
            var itemCollection = new ItemCollection { Items = array };
            var container = new ContainerObj { Collection = itemCollection };
            serializer = new BnfSerializer();
            var result = serializer.Serialize(container);
        }
    }

    public class ContainerObj
    {
        [BnfProperty(Key = "item_collection")]
        public ItemCollection Collection { get; set; }
    }

    public class ItemCollection
    {
        [BnfArray(Key = "items_array", ElementName = "item#")]
        public Item[] Items { get; set; }
    }

    public class Item
    {
        [BnfProperty(Key = "id")]
        public int Id { get; set; }

        [BnfProperty(Key = "name")]
        public string Name { get; set; }
    }
}
