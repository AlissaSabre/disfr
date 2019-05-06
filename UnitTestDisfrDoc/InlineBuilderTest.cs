using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using disfr.Doc;

namespace UnitTestDisfrDoc
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class InlineBuilderTest
    {
        [TestMethod]
        public void Ctors()
        {
            var a = new InlineBuilder();
            a.Contents().Is();
            a.IsEmpty.IsTrue();
        }

        [TestMethod]
        public void CollectionInitializers()
        {
            new InlineBuilder() { "abc" }.Contents().Is(new InlineElement[] { new InlineText("abc") });
            new InlineBuilder() { "abc", "def", "ghi" }.Contents().Is(new InlineElement[] { new InlineText("abcdefghi") });
        }

        [TestMethod]
        public void IsEmpty_1()
        {
            var s = new InlineBuilder();
            s.IsEmpty.IsTrue();
            s.Contents().Count.Is(0);
            s.Append("");
            s.IsEmpty.IsTrue();
            s.Contents().Count.Is(0);
            s.Append("a");
            s.IsEmpty.IsFalse();
            s.Contents().Count.Is(1);
            s.Append("");
            s.IsEmpty.IsFalse();
            s.Contents().Count.Is(1);
            s.Append("a");
            s.IsEmpty.IsFalse();
            s.Contents().Count.Is(1);
        }

        [TestMethod]
        public void IsEmpty_2()
        {
            var s = new InlineBuilder();
            s.IsEmpty.IsTrue();
            s.Contents().Count.Is(0);
            s.Append(new InlineTag(Tag.S, "*", "*", "t1", null, null, null));
            s.IsEmpty.IsFalse();
            s.Contents().Count.Is(1);
            s.Append("");
            s.IsEmpty.IsFalse();
            s.Contents().Count.Is(1);
            s.Append("a");
            s.IsEmpty.IsFalse();
            s.Contents().Count.Is(2);
        }

        [TestMethod]
        public void Append_1()
        {
            var t1 = new InlineTag(Tag.S, "*", "*", "t1", null, null, null);
            var t2 = new InlineTag(Tag.S, "*", "*", "t2", null, null, null);

            var s = new InlineBuilder();
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
            var s = new InlineBuilder();

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

            var s = new InlineBuilder() { "abc", t1, t2 };
            s.Is(new InlineText("abc"), t1, t2);
            s.Append(new InlineString(new InlineText("def"), t1));
            s.Is(new InlineText("abc"), t1, t2, new InlineText("def"), t1);
            s.Append(new InlineString());
            s.Is(new InlineText("abc"), t1, t2, new InlineText("def"), t1);
            s.Append(new InlineString(t1, t2, new InlineText("ghi")));
            s.Is(new InlineText("abc"), t1, t2, new InlineText("def"), t1, t1, t2, new InlineText("ghi"));
            s.Append(new InlineString(new InlineText("jkl"), t2));
            s.Is(new InlineText("abc"), t1, t2, new InlineText("def"), t1, t1, t2, new InlineText("ghijkl"), t2);
        }
    }
}
