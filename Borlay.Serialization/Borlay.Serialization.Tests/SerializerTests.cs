using Borlay.Arrays;
using Borlay.Serialization.Notations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Borlay.Serialization.Tests
{
    [TestClass]
    public class SerializerTests
    {
        [TestMethod]
        public void ConvertTest()
        {
            var testItems = Enumerable.Range(0, 10).Select(i => CreateTestDataItem($"item-{i}", i)).ToList();
            var inhItem = new InheritTestDataItem()
            {
                Id = ByteArray.New(32),
                Name = "Inh",
                Number = 197452374654985980.596417987646462946M,
                Value = 5,
                Type = TestType.InhItem,
                NulValue = 19
            };
            testItems.Add(inhItem);
            var testData = CreateTestData(Guid.NewGuid().ToString(), 1, testItems.ToArray());

            var serializer = new Serializer();
            serializer.LoadFromReference<SerializerTests>();

            var bytes = new byte[1024];
            var index = 0;

            serializer.AddBytes(testData, bytes, ref index);

            index = 0;
            var back = (TestData)serializer.GetObject(bytes, ref index);

            Assert.AreEqual(testData.Id, back.Id);
            Assert.IsNotNull(back.Items);
            Assert.AreEqual(testData.Values[0], back.Values[0]);
            Assert.AreEqual(testData.Values[1], back.Values[1]);
            Assert.AreEqual(testData.Date.Year, back.Date.Year);
            Assert.AreEqual(testData.Date.Day, back.Date.Day);
            Assert.AreEqual(testData.Date.Millisecond, back.Date.Millisecond);
            Assert.AreEqual(testData.Dates[0].Year, back.Dates[0].Year);
            Assert.AreEqual(testData.Dates[0].Day, back.Dates[0].Day);
            Assert.AreEqual(testData.Dates[0].Millisecond, back.Dates[0].Millisecond);
            Assert.AreEqual(testData.Items.Length, back.Items.Length);
            Assert.AreEqual(testData.Items[testItems.Count - 1].Name, back.Items[testItems.Count - 1].Name);
            Assert.IsInstanceOfType(back.Items[0], typeof(TestDataItem));
            Assert.AreEqual(testData.Items[0].NulValue, back.Items[0].NulValue);
            Assert.AreEqual(19, back.Items[testItems.Count - 1].NulValue);
            Assert.IsInstanceOfType(back.Items[testItems.Count - 1], typeof(InheritTestDataItem));

            var backInhItem = (InheritTestDataItem)back.Items[testItems.Count - 1];
            Assert.AreEqual(inhItem.Number, backInhItem.Number);
        }

        [TestMethod]
        public void ConvertArrayTest()
        {
            var testItems = Enumerable.Range(0, 10).Select(i => CreateTestDataItem($"item-{i}", i)).ToArray();
            //var testData = CreateTestData(Guid.NewGuid().ToString(), 1, testItems);

            var serializer = new Serializer();
            serializer.LoadFromReference<SerializerTests>();

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
            var serializer = new Serializer();
            serializer.LoadFromReference<SerializerTests>();

            var obj = DateTime.Now.AddYears(-3).AddDays(-2).AddMilliseconds(-100);

            var bytes = new byte[64];
            var index = 0;

            serializer.AddBytes(obj, bytes, ref index);

            index = 0;
            var back = (DateTime)serializer.GetObject(bytes, ref index);

            Assert.AreEqual(obj.Date.Year, back.Date.Year);
            Assert.AreEqual(obj.Date.Day, back.Date.Day);
            Assert.AreEqual(obj.Date.Millisecond, back.Date.Millisecond);
        }

        [TestMethod]
        public void GuidTest()
        {
            var serializer = new Serializer();
            serializer.LoadFromReference<SerializerTests>();

            var obj = Guid.NewGuid();

            var bytes = new byte[64];
            var index = 0;

            serializer.AddBytes(obj, bytes, ref index);

            index = 0;
            var back = (Guid)serializer.GetObject(bytes, ref index);

            Assert.AreEqual(obj, back);
        }

        [TestMethod]
        public void TimeSpanTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void EnumArrayTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void IArrayConverterTest()
        {
            var serializer = new Serializer();
            serializer.LoadFromReference<SerializerTests>();

            var obj = new byte[] { 5, 7, 20 };

            var bytes = new byte[64];
            var index = 0;

            serializer.AddBytes(obj, bytes, ref index);

            index = 0;
            var back = serializer.GetObject(bytes, ref index) as byte[];

            Assert.IsNotNull(back);
            Assert.AreEqual(obj.Length, back.Length);
            Assert.IsTrue(obj.ContainsSequence32(back));

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
            serializer.LoadFromReference<SerializerTests>();

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
                Date = DateTime.Now.AddYears(-3).AddDays(-2).AddMilliseconds(-100),
                Dates = new DateTime[] { DateTime.Now.AddYears(-4).AddDays(-1).AddMilliseconds(-150) },
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

        [Include(11)]
        public DateTime Date { get; set; }

        [Include(12)]
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

        [Include(3)]
        public int? NulValue { get; set; }
    }

    [Data(3)]
    public class InheritTestDataItem : TestDataItem
    {
        [Include(100)]
        public TestType Type { get; set; }

        [Include(101)]
        public decimal Number { get; set; }
    }
}
