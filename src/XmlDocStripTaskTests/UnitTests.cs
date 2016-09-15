using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XmlDocStripTaskTests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestMethod_Net45()
        {
            var txt = System.IO.File.ReadAllText(@"..\..\..\TestClassLibrary.net45\bin\TestClassLibrary.net45.intellisense.XML");
            Assert.IsFalse(txt.Contains("Exclude"));
        }

        [TestMethod]
        public void TestMethod_UWP()
        {
            var txt = System.IO.File.ReadAllText(@"..\..\..\TestClassLibrary.UWP\bin\TestClassLibrary.UWP.intellisense.XML");
            Assert.IsFalse(txt.Contains("Exclude"));
        }
    }
}
