using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bnf.Serialization;

namespace Bnf.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var dateTimeObj = new DateTimeObj { TradeDate = DateTime.Now, TradeTime = new TimeSpan(17, 35, 59) };
            var serializer = new BnfSerializer();
            var result = serializer.Serialize(dateTimeObj);
        }
    }
}
