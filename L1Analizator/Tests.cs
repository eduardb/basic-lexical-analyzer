using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;

namespace L1Analizator
{
    public class Tests
    {
        [Test]
        public void TestIdentifier()
        {
            Regex i = RegExes.getIdentifierRegex();
            Assert.IsTrue(i.IsMatch("_"));
            Assert.IsTrue(i.IsMatch("a"));
            Assert.IsTrue(i.IsMatch("a_2"));
            Assert.IsTrue(i.IsMatch("_09"));
            Assert.IsTrue(i.IsMatch("_a_d2"));
            Assert.IsFalse(i.IsMatch("9_a"));
            Assert.IsFalse(i.IsMatch("9s"));
            Assert.IsFalse(i.IsMatch("s&"));
        }

        [Test]
        public void TestConstant()
        {
            Regex c = RegExes.getConstantRegex();
            Assert.IsTrue(c.IsMatch("0"));
            Assert.IsTrue(c.IsMatch("0.2"));
            Assert.IsTrue(c.IsMatch("-12"));
            Assert.IsTrue(c.IsMatch("12.02"));
            Assert.IsFalse(c.IsMatch("12.0"));
            Assert.IsFalse(c.IsMatch("02"));
            Assert.IsFalse(c.IsMatch("+0"));
        }

        [Test]
        public void TestNumericalIndex()
        {
            Regex ni = RegExes.getNumericalIndexRegex();
            Assert.IsTrue(ni.IsMatch("0"));
            Assert.IsTrue(ni.IsMatch("123"));
            Assert.IsFalse(ni.IsMatch("02"));
            Assert.IsFalse(ni.IsMatch("+2"));
        }

        [Test]
        public void TestRelations()
        {
            Assert.IsTrue(RegExes.isRelation("<"));
            Assert.IsFalse(RegExes.isRelation("^"));
        }
    }
}
