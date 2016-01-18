using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SInnovations.Azure.ResourceManager.Tests
{
    [TestClass]
    public class CryptoTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var keys = CryptoHelper.CryptoUtilities.GetKeys().ToArray();
            Assert.AreNotEqual(keys[0], keys[1]);
        }
    }
}
