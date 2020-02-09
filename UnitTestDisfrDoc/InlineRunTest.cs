using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class InlineRunTest
    {
        [TestMethod]
        public void Equals_1()
        {
            var a1 = new InlineText("a");
            var a2 = new InlineText("a");
            var b3 = new InlineText("b");

            var p1 = new InlineTag(Tag.S, "id", "rid", "name", "ctype", "display", "code");
            var p2 = new InlineTag(Tag.S, "id", "rid", "name", "ctype", "display", "code");
            var q3 = new InlineTag(Tag.B, "ID", "RID", "NAME", "CTYPE", "DISPLAY", "CODE");

            ReferenceEquals(a1, a2).IsFalse();
            ReferenceEquals(p1, p2).IsFalse();

            InlineRun c1 = a1, c2 = a2, d3 = b3;
            InlineRun r1 = p1, r2 = p2, s3 = q3;

            ReferenceEquals(c1, a1).IsTrue();
            ReferenceEquals(c1, a2).IsFalse();
            ReferenceEquals(r1, p1).IsTrue();
            ReferenceEquals(r1, p2).IsFalse();

            a1.Equals(a2).IsTrue();
            a1.Equals(b3).IsFalse();
            a1.Equals(p2).IsFalse();
            a1.Equals(q3).IsFalse();
            a1.Equals(c2).IsTrue();
            a1.Equals(d3).IsFalse();
            a1.Equals(r2).IsFalse();
            a1.Equals(s3).IsFalse();

            p1.Equals(a2).IsFalse();
            p1.Equals(b3).IsFalse();
            p1.Equals(p2).IsTrue();
            p1.Equals(q3).IsFalse();
            p1.Equals(c2).IsFalse();
            p1.Equals(d3).IsFalse();
            p1.Equals(r2).IsTrue();
            p1.Equals(s3).IsFalse();

            c1.Equals(a2).IsTrue();
            c1.Equals(b3).IsFalse();
            c1.Equals(p2).IsFalse();
            c1.Equals(q3).IsFalse();
            c1.Equals(c2).IsTrue();
            c1.Equals(d3).IsFalse();
            c1.Equals(r2).IsFalse();
            c1.Equals(s3).IsFalse();

            p1.Equals(a2).IsFalse();
            p1.Equals(b3).IsFalse();
            p1.Equals(p2).IsTrue();
            p1.Equals(q3).IsFalse();
            p1.Equals(c2).IsFalse();
            p1.Equals(d3).IsFalse();
            p1.Equals(r2).IsTrue();
            p1.Equals(s3).IsFalse();

            (a1 == a2).IsTrue();
            (a1 == b3).IsFalse();
            (a1 == p2).IsFalse();
            (a1 == q3).IsFalse();
            (a1 == c2).IsTrue();
            (a1 == d3).IsFalse();
            (a1 == r2).IsFalse();
            (a1 == s3).IsFalse();

            (p1 == a2).IsFalse();
            (p1 == b3).IsFalse();
            (p1 == p2).IsTrue();
            (p1 == q3).IsFalse();
            (p1 == c2).IsFalse();
            (p1 == d3).IsFalse();
            (p1 == r2).IsTrue();
            (p1 == s3).IsFalse();

            (c1 == a2).IsTrue();
            (c1 == b3).IsFalse();
            (c1 == p2).IsFalse();
            (c1 == q3).IsFalse();
            (c1 == c2).IsTrue();
            (c1 == d3).IsFalse();
            (c1 == r2).IsFalse();
            (c1 == s3).IsFalse();

            (p1 == a2).IsFalse();
            (p1 == b3).IsFalse();
            (p1 == p2).IsTrue();
            (p1 == q3).IsFalse();
            (p1 == c2).IsFalse();
            (p1 == d3).IsFalse();
            (p1 == r2).IsTrue();
            (p1 == s3).IsFalse();

            (a1 != a2).IsFalse();
            (a1 != b3).IsTrue();
            (a1 != p2).IsTrue();
            (a1 != q3).IsTrue();
            (a1 != c2).IsFalse();
            (a1 != d3).IsTrue();
            (a1 != r2).IsTrue();
            (a1 != s3).IsTrue();

            (p1 != a2).IsTrue();
            (p1 != b3).IsTrue();
            (p1 != p2).IsFalse();
            (p1 != q3).IsTrue();
            (p1 != c2).IsTrue();
            (p1 != d3).IsTrue();
            (p1 != r2).IsFalse();
            (p1 != s3).IsTrue();

            (c1 != a2).IsFalse();
            (c1 != b3).IsTrue();
            (c1 != p2).IsTrue();
            (c1 != q3).IsTrue();
            (c1 != c2).IsFalse();
            (c1 != d3).IsTrue();
            (c1 != r2).IsTrue();
            (c1 != s3).IsTrue();

            (p1 != a2).IsTrue();
            (p1 != b3).IsTrue();
            (p1 != p2).IsFalse();
            (p1 != q3).IsTrue();
            (p1 != c2).IsTrue();
            (p1 != d3).IsTrue();
            (p1 != r2).IsFalse();
            (p1 != s3).IsTrue();
        }
    }
}
