﻿using System;
using Bnf.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Bnf.Serialization.Attributes;
using System.Runtime.Serialization;
using System.IO;
using System.Text;

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

            Assert.AreEqual("{Bnf.Tests.ItemA1={id=1 | name=A} | Bnf.Tests.ItemA2={id=2 | name=B}}", result, false);
            var deserializedArray = serializer.Deserialize<ItemA[]>(result);

            Assert.AreEqual(new ArrayComparer().Compare(array, deserializedArray), 0);
        }

        [TestMethod]
        public void ArrayIntTest()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var itemCollection = new ItemCollection { IntArray = new IntArray (array) };
            var result = serializer.Serialize(itemCollection);

            Assert.AreEqual("{int_item_array={int1=1 | int2=2 | int3=3 | int4=4 | int5=5}}", result, false);
            var deserializedArray = serializer.Deserialize<ItemCollection>(result);

            Assert.AreEqual(itemCollection, deserializedArray);
        }

        [TestMethod]
        public void ArrayStringTest()
        {
            var array = new[] { "Mithun", "Basak", "is", "great" };
            var itemCollection = new ItemCollection { StringArray = new StringArray(array) };
            var result = serializer.Serialize(itemCollection);

            Assert.AreEqual("{string_item_array={enum1=Mithun | enum2=Basak | enum3=is | enum4=great}}", result, false);
            var deserializedArray = serializer.Deserialize<ItemCollection>(result);

            Assert.AreEqual(itemCollection, deserializedArray);
        }

        [TestMethod]
        public void ArrayAsNoAttributePropertyTest()
        {
            var array = new[] { new ItemA { Id = 1, Name = "A" }, new ItemA { Id = 2, Name = "B" } };
            var itemCollection = new ItemCollection { NoAttributeItemArray = array };
            var result = serializer.Serialize(itemCollection);

            Assert.AreEqual("{NoAttributeItemArray={Bnf.Tests.ItemA1={id=1 | name=A} | Bnf.Tests.ItemA2={id=2 | name=B}}}", result, false);

            var deserialized = serializer.Deserialize<ItemCollection>(result);
            Assert.AreEqual(itemCollection, deserialized);
        }

        [TestMethod]
        public void ArrayAsNamedPropertyTest()
        {
            var array = new[] { new ItemA { Id = 1, Name = "A" }, new ItemA { Id = 2, Name = "B" } };
            var itemCollection = new ItemCollection { NamedItemArray = new ItemACollection(array) };
            var result = serializer.Serialize(itemCollection);
            Assert.AreEqual("{named_item_array={item1={id=1 | name=A} | item2={id=2 | name=B}}}", result, false);

            var deserialized = serializer.Deserialize<ItemCollection>(result);

            Assert.AreEqual(itemCollection, deserialized);
        }

        [TestMethod]
        public void ArrayAsNonNamedPropertyTest()
        {
            var array = new[] { new ItemA { Id = 1, Name = "A" }, new ItemA { Id = 2, Name = "B" } };
            var itemCollection = new ItemCollection { NonNamedItemArray = array };
            var result = serializer.Serialize(itemCollection);
            Assert.AreEqual("{nonnamed_item_array={Bnf.Tests.ItemA1={id=1 | name=A} | Bnf.Tests.ItemA2={id=2 | name=B}}}", result, false);

            var deserialized = serializer.Deserialize<ItemCollection>(result);

            Assert.AreEqual(itemCollection, deserialized);
        }

        [TestMethod]
        public void ArrayCollectionTest()
        {
            var itemCollection = new ItemCollection
            {
                NoAttributeItemArray = new[] { new ItemA { Id = 1, Name = "A" }, new ItemA { Id = 2, Name = "B" } },
                NamedItemArray = new ItemACollection( new[] { new ItemA { Id = 3, Name = "C" }, new ItemA { Id = 4, Name = "D" } }),
                NonNamedItemArray = new[] { new ItemA { Id = 5, Name = "E" }, new ItemA { Id = 6, Name = "F" } }
            };
            var container = new ContainerObj { Collection = itemCollection };
            var result = serializer.Serialize(container);

            var sr = new DataContractSerializer(typeof(ContainerObj));
            MemoryStream memoryStream = new MemoryStream();
            sr.WriteObject(memoryStream, container);
            var text = Encoding.UTF8.GetString(memoryStream.GetBuffer());


            Assert.AreEqual("{item_collection={named_item_array={item1={id=3 | name=C} | item2={id=4 | name=D}} | nonnamed_item_array={Bnf.Tests.ItemA1={id=5 | name=E} | Bnf.Tests.ItemA2={id=6 | name=F}} | NoAttributeItemArray={Bnf.Tests.ItemA1={id=1 | name=A} | Bnf.Tests.ItemA2={id=2 | name=B}}}}",
                result, false);

            var deserialized = serializer.Deserialize<ContainerObj>(result);

            Assert.AreEqual(container, deserialized);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ArrayObjectsTest()
        {
            var array = new object[] { new ItemA { Id = 1, Name = "A" }, new ItemB { Income = 100.50m, DateOfBirth = new DateTime(1980, 7, 28) } };
            var result = serializer.Serialize(array);
            Assert.AreEqual("{Bnf.Tests.ItemA1={id=1 | name=A} | Bnf.Tests.ItemB2={dob=28/07/1980 | income=100.50}}", result, false);

            var deserialized = serializer.Deserialize<object[]>(result);

            Assert.AreEqual(result, deserialized);
        }
    }

    [DataContract(Namespace = "")]
    public class ContainerObj
    {
        [DataMember(Name = "item_collection")]
        public ItemCollection Collection { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as ContainerObj;
            if (other == null)
                return false;

            return Collection.Equals(other.Collection);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [DataContract(Namespace = "")]
    public class ItemCollection
    {
        [DataMember(Order = 1)]
        public ItemA[] NoAttributeItemArray { get; set; }

        [DataMember(Name = "named_item_array")]
        public ItemACollection NamedItemArray { get; set; }

        [DataMember(Name = "nonnamed_item_array")]
        public ItemA[] NonNamedItemArray { get; set; }

        [DataMember(Name = "int_item_array")]
        public IntArray IntArray { get; set; }

        [DataMember(Name = "string_item_array")]
        public StringArray StringArray { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as ItemCollection;
            if (other == null)
                return false;


            var comparer = new ArrayComparer();
            return 
                comparer.Compare(NoAttributeItemArray, other?.NoAttributeItemArray) == 0 &&
                NamedItemArray == null ? true : NamedItemArray.Equals(other.NamedItemArray) &&
                IntArray == other?.IntArray &&
                comparer.Compare(NonNamedItemArray, other?.NonNamedItemArray) == 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [DataContract(Namespace = "")]
    public class ItemA
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        public override int GetHashCode()
        {
            return Name == null ? Id.GetHashCode() : Id.GetHashCode() ^ Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as ItemA;
            if (other == null)
                return false;

            return Id == other.Id && Name == other.Name;
        }
    }

    [DataContract(Namespace = "")]
    public class ItemB
    {
        [DataMember(Name = "income")]
        public decimal Income { get; set; }

        [DataMember(Name = "dob")]
        [DataFormat(DataFormatString = "dd/MM/yyyy")]
        public DateTime DateOfBirth { get; set; }

        public override int GetHashCode()
        {
            return Income.GetHashCode() ^ DateOfBirth.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as ItemB;
            if (other == null)
                return false;

            return Income == other.Income && DateOfBirth == other.DateOfBirth;
        }
    }

    [CollectionDataContract(ItemName = "item")]
    public class ItemACollection : List<ItemA>
    {
        public ItemACollection()
        {

        }

        public ItemACollection(IEnumerable<ItemA> collection) : base(collection)
        {

        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as ItemACollection;
            if (other == null)
                return false;

            var comparer = new ArrayComparer();
            return comparer.Compare(ToArray(), other.ToArray()) == 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [CollectionDataContract(ItemName = "int")]
    public class IntArray : List<int>
    {
        public IntArray()
        {

        }

        public IntArray(IEnumerable<int> collection) : base(collection)
        {

        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as IntArray;
            if (other == null)
                return false;

            var comparer = new ArrayComparer();
            return comparer.Compare(ToArray(), other.ToArray()) == 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [CollectionDataContract(ItemName = "enum")]
    public class StringArray : List<string>
    {
        public StringArray()
        {

        }

        public StringArray(IEnumerable<string> collection) : base(collection)
        {

        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as StringArray;
            if (other == null)
                return false;

            var comparer = new ArrayComparer();
            return comparer.Compare(ToArray(), other.ToArray()) == 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class ArrayComparer : IComparer<Array>
    {
        public int Compare(Array x, Array y)
        {
            if (ReferenceEquals(x, y))
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            if (x.Length != y.Length)
                return x.Length > y.Length ? 1 : -1;

            for (int i = 0; i < x.Length; i++)
            {
                if (x.GetValue(i).Equals(y.GetValue(i)))
                    continue;

                return 1;
            }

            return 0;
        }
    }
}
