using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class InlineTag_LooseEqualityComparerTest
    {
        [TestMethod]
        public void Equals_AND_GetHashCode_1()
        {
            var comp = InlineTag.LooseEqualityComparer;

            var a0 = new InlineTag(Tag.S, "id", "rid", "name", "ctype", "display", "code");

            var b1 = new InlineTag(Tag.B, "id", "rid", "name", "ctype", "display", "code");
            var b2 = new InlineTag(Tag.E, "id", "rid", "name", "ctype", "display", "code");
            var b3 = new InlineTag(Tag.S, "XX", "rid", "name", "ctype", "display", "code");
            var b4 = new InlineTag(Tag.S, "id", "XXX", "name", "ctype", "display", "code");
            var b5 = new InlineTag(Tag.S, "id", "rid", "XXXX", "ctype", "display", "code");

            var c1 = new InlineTag(Tag.S, "id", "rid", "name", "XXXXX", "display", "code");
            var c2 = new InlineTag(Tag.S, "id", "rid", "name", "ctype", "XXXXXXX", "code");
            var c3 = new InlineTag(Tag.S, "id", "rid", "name", "ctype", "display", "XXXX");
            var c4 = new InlineTag(Tag.S, "id", "rid", "name", null, "display", "code");
            var c5 = new InlineTag(Tag.S, "id", "rid", "name", "ctype", null, "code");
            var c6 = new InlineTag(Tag.S, "id", "rid", "name", "ctype", "display", null);

            var ab = new InlineTag[] { a0, b1, b2, b3, b4, b5 };
            for (int i = 0; i < ab.Length; i++)
            {
                for (int j = 0; j < ab.Length; j++)
                {
                    if (i != j)
                    {
                        var msg = string.Format("i = {0}, j = {1}", i, j);
                        comp.Equals(ab[i], ab[j]).IsFalse(msg);
                        comp.GetHashCode(ab[i]).IsNot(comp.GetHashCode(ab[j]), msg); // likely though not guaranteed
                    }
                }
            }

            var ac = new InlineTag[] { a0, c1, c2, c3, c4, c5, c6 };
            for (int i = 0; i < ac.Length; i++)
            {
                for (int j = 0; j < ac.Length; j++)
                {
                    var msg = string.Format("i = {0}, j = {1}", i, j);
                    comp.Equals(ac[i], ac[j]).IsTrue(msg);
                    comp.GetHashCode(ac[i]).Is(comp.GetHashCode(ac[j]), msg); // likely though not guaranteed
                }
            }

            var bb = new InlineTag[] { b1, b2, b3, b4, b5 };
            var cc = new InlineTag[] { c1, c2, c3, c4, c5, c6 };
            for (int i = 0; i < bb.Length; i++)
            {
                for (int j = 0; j < cc.Length; j++)
                {
                    var msg = string.Format("i = {0}, j = {1}", i, j);
                    comp.Equals(bb[i], cc[j]).IsFalse(msg);
                    comp.GetHashCode(bb[i]).IsNot(comp.GetHashCode(cc[j]), msg); // likely though not guaranteed
                }
            }
        }
    }
}
