using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class InlineStringTest
    {
        [TestMethod]
        public void Ctors_1()
        {
            var a = new InlineString();
            a.Is();
            a.IsEmpty.IsTrue();
        }

        [TestMethod]
        public void Ctors_2()
        {
            var a = new InlineText("a");
            var b = new InlineText("b");
            var t = new InlineTag(Tag.S, "*", "*", "t", null, null, null);

            new InlineString().Is();
            new InlineString("a").Is(new InlineText("a"));
            new InlineString(a).Is(new InlineText("a"));
            new InlineString(a, b).Is(new InlineText("ab"));
            new InlineString(t).Is(t);
            new InlineString(a, t).Is(new InlineText("a"), t);
            new InlineString(t, a).Is(t, new InlineText("a"));
            new InlineString(a, a).Is(new InlineText("aa"));
            new InlineString(a, t, a).Is(new InlineText("a"), t, new InlineText("a"));
            new InlineString(a, t, a, a).Is(new InlineText("a"), t, new InlineText("aa"));
            new InlineString(a, a, t, a).Is(new InlineText("aa"), t, new InlineText("a"));
        }

        [TestMethod]
        public void IsEmpty_1()
        {
            new InlineString().IsEmpty.IsTrue();
            new InlineString("").IsEmpty.IsTrue();
            new InlineString("a").IsEmpty.IsFalse();
            new InlineString(new InlineText("a")).IsEmpty.IsFalse();
            new InlineString(new InlineText("a"), new InlineText("b")).IsEmpty.IsFalse();
            new InlineString(new InlineTag(Tag.S, "*", "*", "t", null, null, null)).IsEmpty.IsFalse();
        }

        [TestMethod]
        public void Equals_1()
        {
            var t1 = new InlineTag(Tag.S, "*", "*", "t", null, null, null);
            var t2 = new InlineTag(Tag.S, "*", "*", "t", null, null, null);
            var s1 = new InlineBuilder();
            var s2 = new InlineBuilder();

            s1.ToInlineString().Equals(s2.ToInlineString()).Is(true);
            s2.ToInlineString().Equals(s1.ToInlineString()).Is(true);
            s1.Append("abc");
            s1.ToInlineString().Equals(s2.ToInlineString()).Is(false);
            s2.ToInlineString().Equals(s1.ToInlineString()).Is(false);
            s2.Append("abc");
            s1.ToInlineString().Equals(s2.ToInlineString()).Is(true);
            s2.ToInlineString().Equals(s1.ToInlineString()).Is(true);
            s1.Append("def");
            s1.ToInlineString().Equals(s2.ToInlineString()).Is(false);
            s2.ToInlineString().Equals(s1.ToInlineString()).Is(false);
            s2.Append("def");
            s1.ToInlineString().Equals(s2.ToInlineString()).Is(true);
            s2.ToInlineString().Equals(s1.ToInlineString()).Is(true);
            s1.Append(t1);
            s1.ToInlineString().Equals(s2.ToInlineString()).Is(false);
            s2.ToInlineString().Equals(s1.ToInlineString()).Is(false);
            s2.Append(t2);
            s1.ToInlineString().Equals(s2.ToInlineString()).Is(true);
            s2.ToInlineString().Equals(s1.ToInlineString()).Is(true);

            s1.Append("xxx");
            s2.Append("yyy");
            s1.ToInlineString().Equals(s2.ToInlineString()).Is(false);
            s2.ToInlineString().Equals(s1.ToInlineString()).Is(false);
        }

        [TestMethod]
        public void Equals_2()
        {
            var s1 = new InlineBuilder();
            var s2 = new InlineBuilder();

            s1.Append("abc").Append("def");
            s2.Append("abcdef");
            s1.ToInlineString().Equals(s2.ToInlineString()).Is(true);
            s2.ToInlineString().Equals(s1.ToInlineString()).Is(true);
        }

        [TestMethod]
        public void Equals_3()
        {
            var t = new InlineTag(Tag.S, "*", "*", "t", null, null, null);
            var s1 = new InlineBuilder();
            var s2 = new InlineBuilder();

            s1.Append("abc");
            s2.Append(t);
            s1.ToInlineString().Equals(s2.ToInlineString()).Is(false);
            s2.ToInlineString().Equals(s1.ToInlineString()).Is(false);
        }

        [TestMethod]
        public void Equals_4()
        {
            // InlineTag.Equals doesn't see Display string anymore, 
            // hence InlineString.Equals either.

            var t1 = new InlineTag(Tag.S, "*", "*", "t", null, "{t}", null);
            var t2 = new InlineTag(Tag.S, "*", "*", "t", null, "{T}", null);

            var s1 = new InlineBuilder();
            var s2 = new InlineBuilder();

            s1.Append(t1);
            s2.Append(t2);
            s1.ToInlineString().Equals(s2.ToInlineString()).Is(true);
            s2.ToInlineString().Equals(s1.ToInlineString()).Is(true);
        }

        [TestMethod]
        public void GetHashCode_1()
        {
            var t1 = new InlineTag(Tag.S, "*", "*", "t", null, null, null);
            var t2 = new InlineTag(Tag.S, "*", "*", "t", null, null, null);

            var s1 = new InlineBuilder();
            var s2 = new InlineBuilder();
            s1.ToInlineString().GetHashCode().Is(s2.ToInlineString().GetHashCode());

            s1.Append("abc");
            s1.ToInlineString().GetHashCode().IsNot(s2.ToInlineString().GetHashCode()); // very likely but not guaranteed
            s2.Append("abc");
            s1.ToInlineString().GetHashCode().Is(s2.ToInlineString().GetHashCode());
            s1.Append("def");
            s1.ToInlineString().GetHashCode().IsNot(s2.ToInlineString().GetHashCode()); // very likely but not guaranteed
            s2.Append("def");
            s1.ToInlineString().GetHashCode().Is(s2.ToInlineString().GetHashCode());
            s1.Append(t1);
            s1.ToInlineString().GetHashCode().IsNot(s2.ToInlineString().GetHashCode()); // very likely but not guaranteed
            s2.Append(t2);
            s1.ToInlineString().GetHashCode().Is(s2.ToInlineString().GetHashCode());

            s1.Append("xxx");
            s1.ToInlineString().GetHashCode().IsNot(s2.ToInlineString().GetHashCode()); // very likely but not guaranteed
            s2.Append("yyy");
            s1.ToInlineString().GetHashCode().IsNot(s2.ToInlineString().GetHashCode()); // very likely but not guaranteed
        }

        [TestMethod]
        public void GetHashCode_4()
        {
            // InlineTag.GetHashCode doesn't see Display string anymore,
            // Hence InlineString.GetHashCode, either.

            var t1 = new InlineTag(Tag.S, "*", "", "t", null, "{t}", null);
            var t2 = new InlineTag(Tag.S, "*", "", "t", null, "{T}", null);

            var s1 = new InlineBuilder();
            var s2 = new InlineBuilder();

            s1.Append(t1);
            s2.Append(t2);
            s1.ToInlineString().GetHashCode().Is(s2.ToInlineString().GetHashCode());
        }
    }
}
