using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class InlineStringTest1
    {
        [TestMethod]
        public void Ctors()
        {
            var a = new InlineString();
            a.Contents.Is(new InlineElement[0]);
            a.IsEmpty.IsTrue();
        }

        [TestMethod]
        public void CollectionInitializer()
        {
            new InlineString() { "abc" }.Is(new InlineElement[] { new InlineText("abc") });
            new InlineString() { "abc", "def", "ghi" }.Is(new InlineElement[] { new InlineText("abcdefghi") });
        }

        [TestMethod]
        public void IsEmpty_1()
        {
            var s = new InlineString();
            s.IsEmpty.IsTrue();
            s.Contents.Count().Is(0);
            s.Append("");
            s.IsEmpty.IsTrue();
            s.Contents.Count().Is(0);
            s.Append("a");
            s.IsEmpty.IsFalse();
            s.Contents.Count().Is(1);
        }

        [TestMethod]
        public void IsEmpty_2()
        {
            var s = new InlineString();
            s.IsEmpty.IsTrue();
            s.Append(new InlineTag(Tag.S, "*", "*", "t", null, null, null));
            s.IsEmpty.IsFalse();
        }

        [TestMethod]
        public void Append_1()
        {
            var t1 = new InlineTag(Tag.S, "*", "*", "t1", null, null, null);
            var t2 = new InlineTag(Tag.S, "*", "*", "t2", null, null, null);
            var s = new InlineString();
            s.Is();
            s.Append("");
            s.Is();
            s.Append("abc");
            s.Is(new InlineText("abc"));
            s.Append("def");
            s.Is(new InlineText("abcdef"));
            s.Append("");
            s.Is(new InlineText("abcdef"));
            s.Append(t1);
            s.Is(new InlineText("abcdef"), t1);
            s.Append(t2);
            s.Is(new InlineText("abcdef"), t1, t2);
            s.Append("");
            s.Is(new InlineText("abcdef"), t1, t2);
            s.Append("xyz");
            s.Is(new InlineText("abcdef"), t1, t2, new InlineText("xyz"));
        }

        [TestMethod]
        public void Append_2()
        {
            var t = new InlineTag(Tag.S, "*", "*", "t", null, null, null);
            var s = new InlineString();

            AssertEx.Catch<ArgumentNullException>(() => s.Append((string)null));
            AssertEx.Catch<ArgumentNullException>(() => s.Append((InlineTag)null));
            s.Is();

            s.Append(t);
            AssertEx.Catch<ArgumentNullException>(() => s.Append((string)null));
            AssertEx.Catch<ArgumentNullException>(() => s.Append((InlineTag)null));
            s.Is(t);

            s.Append("abc");
            AssertEx.Catch<ArgumentNullException>(() => s.Append((string)null));
            AssertEx.Catch<ArgumentNullException>(() => s.Append((InlineTag)null));
            s.Is(t, new InlineText("abc"));
        }

        [TestMethod]
        public void Append_3()
        {
            var t1 = new InlineTag(Tag.S, "*", "*", "t1", null, null, null);
            var t2 = new InlineTag(Tag.S, "*", "*", "t2", null, null, null);

            var s = new InlineString() { "abc", t1, t2 };
            s.Is(new InlineText("abc"), t1, t2);
            s.Append(new InlineString() { "def", t1 });
            s.Is(new InlineText("abc"), t1, t2, new InlineText("def"), t1);
            s.Append(new InlineString());
            s.Is(new InlineText("abc"), t1, t2, new InlineText("def"), t1);
            s.Append(new InlineString() { t1, t2, "ghi" });
            s.Is(new InlineText("abc"), t1, t2, new InlineText("def"), t1, t1, t2, new InlineText("ghi"));
            s.Append(new InlineString() { "jkl", t2 });
            s.Is(new InlineText("abc"), t1, t2, new InlineText("def"), t1, t1, t2, new InlineText("ghijkl"), t2);
        }

        [TestMethod]
        public void Append_4()
        {
            var s = new InlineString() { "ab cd" };
            var t = new InlineString() { "ef gh" };
            s.Append(t);
            s.Is(new InlineString() { "ab cdef gh" });
            s.Contents.Count().Is(1);
            AssertEx.Catch<ArgumentException>(() => s.Append(s));
        }

        [TestMethod]
        public void Equals_1()
        {
            var t1 = new InlineTag(Tag.S, "*", "*", "t", null, null, null);
            var t2 = new InlineTag(Tag.S, "*", "*", "t", null, null, null);
            var s1 = new InlineString();
            var s2 = new InlineString();

            s1.Equals(s2).Is(true);
            s2.Equals(s1).Is(true);
            s1.Append("abc");
            s1.Equals(s2).Is(false);
            s2.Equals(s1).Is(false);
            s2.Append("abc");
            s1.Equals(s2).Is(true);
            s2.Equals(s1).Is(true);
            s1.Append("def");
            s1.Equals(s2).Is(false);
            s2.Equals(s1).Is(false);
            s2.Append("def");
            s1.Equals(s2).Is(true);
            s2.Equals(s1).Is(true);
            s1.Append(t1);
            s1.Equals(s2).Is(false);
            s2.Equals(s1).Is(false);
            s2.Append(t2);
            s1.Equals(s2).Is(true);
            s2.Equals(s1).Is(true);

            s1.Append("xxx");
            s2.Append("yyy");
            s1.Equals(s2).Is(false);
            s2.Equals(s1).Is(false);
        }

        [TestMethod]
        public void Equals_2()
        {
            var s1 = new InlineString();
            var s2 = new InlineString();

            s1.Append("abc").Append("def");
            s2.Append("abcdef");
            s1.Equals(s2).Is(true);
            s2.Equals(s1).Is(true);
        }

        [TestMethod]
        public void Equals_3()
        {
            var t = new InlineTag(Tag.S, "*", "*", "t", null, null, null);
            var s1 = new InlineString();
            var s2 = new InlineString();

            s1.Append("abc");
            s2.Append(t);
            s1.Equals(s2).Is(false);
            s2.Equals(s1).Is(false);
        }

        [TestMethod]
        public void Equals_4()
        {
            // InlineTag.Equals doesn't see Display string anymore, 
            // hence InlineString.Equals either.

            var t1 = new InlineTag(Tag.S, "*", "*", "t", null, "{t}", null);
            var t2 = new InlineTag(Tag.S, "*", "*", "t", null, "{T}", null);

            var s1 = new InlineString();
            var s2 = new InlineString();

            s1.Append(t1);
            s2.Append(t2);
            s1.Equals(s2).Is(true);
            s2.Equals(s1).Is(true);
        }

        [TestMethod]
        public void GetHashCode_1()
        {
            var t1 = new InlineTag(Tag.S, "*", "*", "t", null, null, null);
            var t2 = new InlineTag(Tag.S, "*", "*", "t", null, null, null);

            var s1 = new InlineString();
            var s2 = new InlineString();
            s1.GetHashCode().Is(s2.GetHashCode());

            s1.Append("abc");
            s1.GetHashCode().IsNot(s2.GetHashCode()); // very likely but not guaranteed
            s2.Append("abc");
            s1.GetHashCode().Is(s2.GetHashCode());
            s1.Append("def");
            s1.GetHashCode().IsNot(s2.GetHashCode()); // very likely but not guaranteed
            s2.Append("def");
            s1.GetHashCode().Is(s2.GetHashCode());
            s1.Append(t1);
            s1.GetHashCode().IsNot(s2.GetHashCode()); // very likely but not guaranteed
            s2.Append(t2);
            s1.GetHashCode().Is(s2.GetHashCode());

            s1.Append("xxx");
            s1.GetHashCode().IsNot(s2.GetHashCode()); // very likely but not guaranteed
            s2.Append("yyy");
            s1.GetHashCode().IsNot(s2.GetHashCode()); // very likely but not guaranteed
        }

        [TestMethod]
        public void GetHashCode_4()
        {
            // InlineTag.GetHashCode doesn't see Display string anymore,
            // Hence InlineString.GetHashCode, either.

            var t1 = new InlineTag(Tag.S, "*", "", "t", null, "{t}", null);
            var t2 = new InlineTag(Tag.S, "*", "", "t", null, "{T}", null);

            var s1 = new InlineString();
            var s2 = new InlineString();

            s1.Append(t1);
            s2.Append(t2);
            s1.GetHashCode().Is(s2.GetHashCode()); // very likely but not guaranteed
        }
    }

    [TestClass]
    public class InlineTagTest1
    {
        [TestMethod]
        public void Ctors_1()
        {
            var a = new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<a/>");
            a.TagType.Is(Tag.S);
            a.Id.Is("id");
            a.Rid.Is("rid");
            a.Name.Is("a");
            a.Ctype.Is("link");
            a.Display.Is("{a}");
            a.Code.Is("<a/>");

            AssertEx.Catch<ArgumentNullException>(() => new InlineTag(Tag.S, null, "rid", "a",  null, "{a}", "<a/>"));
            AssertEx.Catch<ArgumentNullException>(() => new InlineTag(Tag.S, "id", null,  "a",  null, "{a}", "<a/>"));
            AssertEx.Catch<ArgumentNullException>(() => new InlineTag(Tag.S, "id", "rid", null, null, "{a}", "<a/>"));
            new InlineTag(Tag.S, "id", "rid", "a", null, null, null); // Throws nothing.

            var a2 = new InlineTag(Tag.B, "id", "rid", "a", "link", "{a}", "<a/>");
            a2.TagType.Is(Tag.B);
            a2.TagType.IsNot(Tag.E);

            var a3 = new InlineTag(Tag.E, "id", "rid", "a", "link", "{a}", "<a/>");
            a3.TagType.Is(Tag.E);
            a3.TagType.IsNot(Tag.B);
        }

        [TestMethod]
        public void Ctors_2()
        {
            var a = new InlineTag(Tag.S, "id", "rid", "a", null, null, "<a/>");
            a.TagType.Is(Tag.S);
            a.Id.Is("id");
            a.Rid.Is("rid");
            a.Name.Is("a");
            a.Ctype.Is(null as string);
            a.Display.Is(null as string);
            a.Code.Is("<a/>");
        }

        [TestMethod]
        public void Ctors_3()
        {
            var a = new InlineTag(Tag.S, "id", "rid", "a", null, "{a}", null);
            a.TagType.Is(Tag.S);
            a.Id.Is("id");
            a.Rid.Is("rid");
            a.Name.Is("a");
            a.Ctype.Is(null as string);
            a.Display.Is("{a}");
            a.Code.Is(null as string);
        }

        [TestMethod]
        public void Ctors_4()
        {
            var a = new InlineTag(Tag.S, "id", "rid", "a", "link", null, null);
            a.TagType.Is(Tag.S);
            a.Id.Is("id");
            a.Rid.Is("rid");
            a.Name.Is("a");
            a.Ctype.Is("link");
            a.Display.Is(null as string);
            a.Code.Is(null as string);
        }

        [TestMethod]
        public void Equals_1()
        {
            var a1 = new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<a/>");
            var a2 = new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<a/>");
            var b1 = new InlineTag(Tag.S, "id", "rid", "b", "link", "{b}", "<b/>");

            a1.Equals(a1).Is(true);
            a1.Equals(a2).Is(true);
            a1.Equals(b1).Is(false);
            a2.Equals(a1).Is(true);
            a2.Equals(a2).Is(true);
            a2.Equals(b1).Is(false);
            b1.Equals(a1).Is(false);
            b1.Equals(a2).Is(false);
            b1.Equals(b1).Is(true);

            a1.Equals(null).Is(false);
            a1.Equals(new Object()).Is(false);
            a1.Equals("abc").Is(false);

            a1.Is(a1);
            a1.Is(a2);
            a1.IsNot(b1);
        }

        [TestMethod]
        public void Equals_2()
        {
            var a = new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<a/>");
            a.IsNot(new InlineTag(Tag.B, "id", "rid", "a", "link", "{a}", "<a/>"));
            a.IsNot(new InlineTag(Tag.E, "id", "rid", "a", "link", "{a}", "<a/>"));
            a.IsNot(new InlineTag(Tag.S, "Id", "rid", "a", "link", "{a}", "<a/>"));
            a.IsNot(new InlineTag(Tag.S, "id", "rId", "a", "link", "{a}", "<a/>"));
            a.IsNot(new InlineTag(Tag.S, "id", "rid", "A", "link", "{a}", "<a/>"));

            // InlineTag.Equals doesn't see Ctype, Display string nor Code.
            a.Is(new InlineTag(Tag.S, "id", "rid", "a", "Link", "{a}", "<a/>"));
            a.Is(new InlineTag(Tag.S, "id", "rid", "a", "link", "{A}", "<a/>"));
            a.Is(new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<A/>"));
        }

        [TestMethod]
        public void InlineTag_Is_1()
        {
            var a1 = new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<a/>");
            var b1 = new InlineTag(Tag.S, "id", "rid", "b", "link", "{b}", "<b/>");

            a1.Is(Tag.S, "id", "rid", "a", "link", "{a}", "<a/>");
            b1.Is(Tag.S, "id", "rid", "b", "link", "{b}", "<b/>");
        }

        [TestMethod]
        public void GetHashCode_1()
        {
            var a1 = new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<a/>");
            var a2 = new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<a/>");
            var b1 = new InlineTag(Tag.S, "id", "rid", "b", "link", "{b}", "<b/>");

            a1.GetHashCode().Is(a2.GetHashCode());
            a1.GetHashCode().IsNot(b1.GetHashCode()); // very likely but not guaranteed
        }
    }

    //[TestClass]
    //public class InlineCharTest1
    //{
    //    [TestMethod]
    //    public void Ctor_1()
    //    {
    //        new InlineChar('\u0020');
    //        new InlineChar('\u0009');
    //    }

    //    [TestMethod]
    //    public void Equals_1()
    //    {
    //        new InlineChar('\u0020').Equals(new InlineChar('\u0020')).Is(true);
    //        new InlineChar('\u0009').Equals(new InlineChar('\u0009')).Is(true);
    //        new InlineChar('\u0020').Equals(new InlineChar('\u0009')).Is(false);
    //        new InlineChar('\u0009').Equals(new InlineChar('\u0020')).Is(false);
    //        new InlineChar('\u0020').Equals("\u0020").Is(false);
    //        new InlineChar('\u0020').Equals('\u0020').Is(false);
    //    }

    //    private static readonly char[] GetHashCode_TestData_1 =
    //    {
    //        '\u0009', '\u000A', '\u000D', '\u0020', '\u00A0',
    //        '\u1680', '\u180E',
    //        '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007',
    //        '\u2008', '\u2009', '\u200A', '\u200B', '\u200C', '\u200D',
    //        '\u2028', '\u2029', '\u202F', '\u205F',
    //        '\u2060', '\u2061', '\u2062', '\u2063',
    //        '\u3000', '\u3064',
    //        '\uFFA0', '\uFEFF',
    //    };

    //    [TestMethod]
    //    public void GetHashCode_1()
    //    {
    //        foreach (char c in GetHashCode_TestData_1)
    //        {
    //            foreach (char d in GetHashCode_TestData_1)
    //            {
    //                (new InlineChar(c).GetHashCode() == new InlineChar(d).GetHashCode())
    //                    .Is(c == d, string.Format("{0:X} vs {1:X}", (int)c, (int)d)); 
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public void Char_1()
    //    {
    //        new InlineChar('\u0020').Char.Is('\u0020');
    //        new InlineChar('\u0009').Char.Is('\u0009');
    //    }
    //}

    public static class InlineString_ExtensionMethods
    {
        public static void Is(this InlineTag tag, Tag type, string id, string rid, string name, string ctype, string display, string code)
        {
            tag.Is(new InlineTag(type, id, rid, name, ctype, display, code));
        }
    }
}
