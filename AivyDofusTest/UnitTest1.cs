using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AivyDofusTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            int[] a = new int[40];
            a[20] = 10;
            Assert.AreEqual(10, a[20]);
        }
    }
}
