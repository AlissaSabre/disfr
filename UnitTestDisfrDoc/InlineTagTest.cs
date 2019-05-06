using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class InlineTagTest
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

            AssertEx.Catch<ArgumentNullException>(() => new InlineTag(Tag.S, null, "rid", "a", null, "{a}", "<a/>"));
            AssertEx.Catch<ArgumentNullException>(() => new InlineTag(Tag.S, "id", null, "a", null, "{a}", "<a/>"));
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
}
