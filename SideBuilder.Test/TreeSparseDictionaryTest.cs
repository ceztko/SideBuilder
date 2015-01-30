using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Collections.Specialized;

namespace SideBuilder.Test
{
    [TestFixture]
    public class TreeSparseDictionaryTest
    {
        private TreeSparseDictionary<string> _Dictionary;

        [TestFixtureSetUp]
        public void Setup()
        {
            _Dictionary = new TreeSparseDictionary<string>();
        }

        [Test]
        public void Foo()
        {
            _Dictionary["Test1"] = "Test1";
            Assert.AreEqual(_Dictionary["Test1"], "Test1");
        }
    }
}
