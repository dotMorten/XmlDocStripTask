using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XmlDocStripTaskTests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestMethod_Net6()
        {
            var txt = System.IO.File.ReadAllText(@"..\..\..\TestClassLibrary\bin\Debug\net6.0\TestClassLibrary.xml");
            Assert.IsFalse(txt.Contains("Exclude"));
        }

        [TestMethod]
        public void TestMethod_netstd()
        {
            var txt = System.IO.File.ReadAllText(@"..\..\..\TestClassLibrary\bin\Debug\netstandard2.0\TestClassLibrary.xml");
            Assert.IsFalse(txt.Contains("Exclude"));
        }
    }
}
