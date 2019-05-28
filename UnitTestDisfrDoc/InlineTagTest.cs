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

            a1.Equals(a1).IsTrue();
            a1.Equals(a2).IsTrue();
            a1.Equals(b1).IsFalse();
            a2.Equals(a1).IsTrue();
            a2.Equals(a2).IsTrue();
            a2.Equals(b1).IsFalse();
            b1.Equals(a1).IsFalse();
            b1.Equals(a2).IsFalse();
            b1.Equals(b1).IsTrue();

            a1.Equals(null).IsFalse();
            a1.Equals(new Object()).IsFalse();
            a1.Equals("a").IsFalse();

            a1.Is(a1);
            a1.Is(a2);
            a1.IsNot(b1);
        }

        [TestMethod]
        public void Equals_2()
        {
            var a = new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<a/>");

            a.Equals(new InlineTag(Tag.B, "id", "rid", "a", "link", "{a}", "<a/>")).IsFalse();
            a.Equals(new InlineTag(Tag.E, "id", "rid", "a", "link", "{a}", "<a/>")).IsFalse();
            a.Equals(new InlineTag(Tag.S, "ID", "rid", "a", "link", "{a}", "<a/>")).IsFalse();
            a.Equals(new InlineTag(Tag.S, "id", "RID", "a", "link", "{a}", "<a/>")).IsFalse();
            a.Equals(new InlineTag(Tag.S, "id", "rid", "A", "link", "{a}", "<a/>")).IsFalse();
            a.Equals(new InlineTag(Tag.S, "id", "rid", "a", "LINK", "{a}", "<a/>")).IsFalse();
            a.Equals(new InlineTag(Tag.S, "id", "rid", "a", "link", "{A}", "<a/>")).IsFalse();
            a.Equals(new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<A/>")).IsFalse();

            (a == new InlineTag(Tag.B, "id", "rid", "a", "link", "{a}", "<a/>")).IsFalse();
            (a == new InlineTag(Tag.E, "id", "rid", "a", "link", "{a}", "<a/>")).IsFalse();
            (a == new InlineTag(Tag.S, "ID", "rid", "a", "link", "{a}", "<a/>")).IsFalse();
            (a == new InlineTag(Tag.S, "id", "RID", "a", "link", "{a}", "<a/>")).IsFalse();
            (a == new InlineTag(Tag.S, "id", "rid", "A", "link", "{a}", "<a/>")).IsFalse();
            (a == new InlineTag(Tag.S, "id", "rid", "a", "LINK", "{a}", "<a/>")).IsFalse();
            (a == new InlineTag(Tag.S, "id", "rid", "a", "link", "{A}", "<a/>")).IsFalse();
            (a == new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<A/>")).IsFalse();

            (a != new InlineTag(Tag.B, "id", "rid", "a", "link", "{a}", "<a/>")).IsTrue();
            (a != new InlineTag(Tag.E, "id", "rid", "a", "link", "{a}", "<a/>")).IsTrue();
            (a != new InlineTag(Tag.S, "ID", "rid", "a", "link", "{a}", "<a/>")).IsTrue();
            (a != new InlineTag(Tag.S, "id", "RID", "a", "link", "{a}", "<a/>")).IsTrue();
            (a != new InlineTag(Tag.S, "id", "rid", "A", "link", "{a}", "<a/>")).IsTrue();
            (a != new InlineTag(Tag.S, "id", "rid", "a", "LINK", "{a}", "<a/>")).IsTrue();
            (a != new InlineTag(Tag.S, "id", "rid", "a", "link", "{A}", "<a/>")).IsTrue();
            (a != new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<A/>")).IsTrue();
        }

        [TestMethod]
        public void Equals_3()
        {
            var a1 = new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<a/>");
            var a2 = new InlineTag(Tag.S, "id", "rid", "a", "link", "{a}", "<a/>");
            var b9 = new InlineTag(Tag.S, "id", "rid", "b", "link", "{b}", "<b/>");

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
