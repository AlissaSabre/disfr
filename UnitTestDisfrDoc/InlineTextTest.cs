using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class InlineTextTest : InlineStringTestBase
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

            (new InlineText("") == null).IsFalse();
            (new InlineText("a") == null).IsFalse();
            (new InlineText("b") == null).IsFalse();
            (new InlineText("abc") == null).IsFalse();

            (new InlineText("") != null).IsTrue();
            (new InlineText("a") != null).IsTrue();
            (new InlineText("b") != null).IsTrue();
            (new InlineText("abc") != null).IsTrue();
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
                    (new InlineText(s1) == new InlineText(s2)).Is(s1 == s2, string.Format("s1 = \"{0}\", s2 = \"{1}\"", s1, s2));
                    (new InlineText(s1) != new InlineText(s2)).Is(s1 != s2, string.Format("s1 = \"{0}\", s2 = \"{1}\"", s1, s2));
                }
            }
        }

        [TestMethod]
        public void Equals_3()
        {
            InlineText a1 = new InlineText("a");
            InlineText a2 = new InlineText("a");
            InlineText b9 = new InlineText("b");

            object x1 = a1;
            object x2 = a2;
            object y9 = b9;

            ReferenceEquals(a1, a2).IsFalse();
            a1.Equals(a1).IsTrue();
            a1.Equals(a2).IsTrue();
            a1.Equals(b9).IsFalse();
#pragma warning disable CS1718
            (a1 == a1).IsTrue();
            (a1 == a2).IsTrue();
            (a1 == b9).IsFalse();
            (a1 != a1).IsFalse();
            (a1 != a2).IsFalse();
            (a2 != b9).IsTrue();
#pragma warning restore CS1718

            x1.Equals(x1).IsTrue();
            x1.Equals(x2).IsTrue();
            x1.Equals(b9).IsFalse();
#pragma warning disable CS1718
            (x1 == x1).IsTrue();
            (x1 == x2).IsFalse();
            (x1 == y9).IsFalse();
            (x1 != x1).IsFalse();
            (x1 != x2).IsTrue();
            (x1 != y9).IsTrue();
#pragma warning restore CS1718

            a1.Equals(x1).IsTrue();
            a1.Equals(x2).IsTrue();
            a1.Equals(y9).IsFalse();
#pragma warning disable CS0253
            (a1 == x1).IsTrue();
            (a1 == x2).IsFalse();
            (a1 == y9).IsFalse();
            (a1 != x1).IsFalse();
            (a1 != x2).IsTrue();
            (a1 != y9).IsTrue();
#pragma warning restore CS0253

            x1.Equals(a1).IsTrue();
            x1.Equals(a2).IsTrue();
            x1.Equals(b9).IsFalse();
#pragma warning disable CS0252
            (x1 == a1).IsTrue();
            (x1 == a2).IsFalse();
            (x1 == b9).IsFalse();
            (x1 != a1).IsFalse();
            (x1 != a2).IsTrue();
            (x2 != b9).IsTrue();
#pragma warning restore CS0252
        }

        [TestMethod]
        public void Equals_4()
        {
            new InlineText("").Equals("").IsFalse();
            new InlineText("a").Equals("a").IsFalse();
        }

        [TestMethod]
        public void ToString_1()
        {
            foreach (var s in new[] { "", "a", "b", "ab" })
            {
                new InlineText(s).ToString().Is(s, string.Format("s = \"{0}\"", s));
                foreach (InlineToString t in Enum.GetValues(typeof(InlineToString)))
                {
                    new InlineText(s).ToString(t).Is(s, string.Format("s = \"{0}\", t = \"{1}\"", s, t));
                }
            }
        }
    }
}
