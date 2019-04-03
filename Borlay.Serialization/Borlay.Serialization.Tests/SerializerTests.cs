using Borlay.Arrays;
using Borlay.Serialization.Converters;
using Borlay.Serialization.Notations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Borlay.Serialization.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ConvertTest()
        {
            var testItems = Enumerable.Range(0, 10).Select(i => CreateTestDataItem($"item-{i}", i)).ToArray();
            var testData = CreateTestData(Guid.NewGuid().ToString(), 1, testItems);

            var serializer = new Serializer();
            serializer.LoadFromReference<UnitTest1>();

            var bytes = new byte[1024];
            var index = 0;

            serializer.AddBytes(testData, bytes, ref index);

            index = 0;
            var back = (TestData)serializer.GetObject(bytes, ref index);

            Assert.AreEqual(testData.Id, back.Id);
            Assert.IsNotNull(back.Items);
            Assert.AreEqual(testData.Items.Length, back.Items.Length);
            Assert.AreEqual(testData.Items[testItems.Length - 1].Name, back.Items[testItems.Length - 1].Name);
        }

        [TestMethod]
        public void ConvertArrayTest()
        {
            var testItems = Enumerable.Range(0, 10).Select(i => CreateTestDataItem($"item-{i}", i)).ToArray();
            //var testData = CreateTestData(Guid.NewGuid().ToString(), 1, testItems);

            var serializer = new Serializer();
            serializer.LoadFromReference<UnitTest1>();

            var bytes = new byte[1024];
            var index = 0;

            serializer.AddBytes(testItems, bytes, ref index);

            index = 0;
            var back = (TestDataItem[])serializer.GetObject(bytes, ref index);

            Assert.IsNotNull(back);
            Assert.AreEqual(testItems.Length, back.Length);
            Assert.AreEqual(testItems[0].Id, back[0].Id);
            Assert.AreEqual(testItems[0].Name, back[0].Name);
            Assert.AreEqual(testItems[testItems.Length - 1].Id, back[testItems.Length - 1].Id);
            Assert.AreEqual(testItems[testItems.Length - 1].Name, back[testItems.Length - 1].Name);
        }

        [TestMethod]
        public void DateTimeTest()
        {

        }

        [TestMethod]
        public void TimeSpanTest()
        {

        }

        [TestMethod]
        public void EnumArrayTest()
        {
        }

        [TestMethod]
        public void IArrayConverterTest()
        {
            // byte[] IArrayConverter
        }

        [TestMethod]
        public void DoubleArrayConverterTest()
        {
            // byte[][]
        }

        [TestMethod]
        public void ConvertManyTest()
        {
            var count = 100000;
            var testItems = Enumerable.Range(0, 10).Select(i => CreateTestDataItem($"item-{i}", i)).ToArray();
            var testData = CreateTestData(Guid.NewGuid().ToString(), 1, testItems);

            var serializer = new Serializer();
            serializer.LoadFromReference<UnitTest1>();

            var bytes = new byte[1024];
            var index = 0;

            var watch = Stopwatch.StartNew();

            for(int i = 0; i < count; i++)
            {
                index = 0;
                serializer.AddBytes(testData, bytes, ref index);
            }

            watch.Stop();

            // 100k 2.3s
            // po perdarymo
            // 100k 2.0s
        }

        public TestData CreateTestData(string name, int index, params TestDataItem[] items)
        {
            return new TestData()
            {
                Id = ByteArray.New(32),
                Ids = new ByteArray[] { ByteArray.New(9), ByteArray.New(5) },
                Name = name,
                Index = index,
                Indexes = new int[] { 1, 3 },
                Value = 0,
                Values = new byte[] { 4, 7 },
                //Number = index,
                Items = items,
            };
        }

        public TestDataItem CreateTestDataItem(string name, int value)
        {
            return new TestDataItem()
            {
                Id = ByteArray.New(32),
                Name = name,
                Value = value
            };
        }
    }

    [Data(1)]
    public class TestData
    {
        [Include(0)]
        public ByteArray Id { get; set; }

        [Include(1)]
        public ByteArray[] Ids { get; set; }

        [Include(2)]
        public string Name { get; set; }

        [Include(3)]
        public int Index { get; set; }

        [Include(4)]
        public int[] Indexes { get; set; }

        //[Include(5)]
        //public decimal Number { get; set; }

        [Include(6)]
        public byte Value { get; set; }

        [Include(7)]
        public byte[] Values { get; set; }

        [Include(8)]
        public TestType Type { get; set; }

        [Include(9)]
        public TestType[] Types { get; set; }

        [Include(10)]
        public bool Is { get; set; }

        public DateTime Date { get; set; }

        public DateTime[] Dates { get; set; }

        [Include(13)]
        public TestDataItem Item { get; set; }

        //[Array(0, 100)]
        [Include(14)]
        public TestDataItem[] Items { get; set; }
    }

    public enum TestType
    {
        Data = 1,
        Item,
        InhItem
    }

    [Data(2)]
    public class TestDataItem
    {
        [Include(0)]
        public ByteArray Id { get; set; }

        [Include(1)]
        public string Name { get; set; }

        [Include(2)]
        public int Value { get; set; }
    }

    [Data(3)]
    public class InheritTestDataItem : TestDataItem
    {
        [Include(3)]
        public Type Type { get; set; }

        [Include(4)]
        public decimal Number { get; set; }
    }
}
