using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class InlineTextTest
    {
        [TestMethod]
        public void Ctors_1()
        {
            new InlineText("").ToString().Is("");
            new InlineText("a").ToString().Is("a");
            new InlineText("b").ToString().Is("b");
            new InlineText("abc").ToString().Is("abc");
        }

        [TestMethod]
        public void Ctors_Exception_1()
        {
            AssertEx.Catch<ArgumentNullException>(() => new InlineText(null));
        }

        [TestMethod]
        public void Equals_1()
        {
            new InlineText("").Equals(null).IsFalse();
            new InlineText("a").Equals(null).IsFalse();
            new InlineText("b").Equals(null).IsFalse();
            new InlineText("abc").Equals(null).IsFalse();
        }

        [TestMethod]
        public void Equals_2()
        {
            var samples = new[]
            {
                "", "a", "b", "abc",
                new string(new char[0]),
                new string(new char[] { 'a' }),
                new string(new char[] { 'b' }),
                new string(new char[] { 'a', 'b', 'c' }),
            };

            foreach (var s1 in samples)
            {
                foreach (var s2 in samples)
                {
                    new InlineText(s1).Equals(new InlineText(s2)).Is(s1 == s2, string.Format("s1 = \"{0}\", s2 = \"{1}\"", s1, s2));
                }
            }
        }
    }
}
