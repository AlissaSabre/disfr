using System;
using System.Linq;
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
    public class InlineBuilderTest : InlineStringTestBase
    {
        [TestMethod]
        public void Ctors()
        {
            var a = new InlineBuilder();
            a.Is();
            a.IsEmpty.IsTrue();
        }

        [TestMethod]
        public void CollectionInitializers()
        {
            new InlineBuilder() { "abc" }.Is("abc");
            new InlineBuilder() { "abc", "def", "ghi" }.Is("abcdefghi");
        }

        [TestMethod]
        public void Property_1()
        {
            new InlineBuilder().Property.Is(None);
            new InlineBuilder() { Property = None }.Property.Is(None);
            new InlineBuilder() { Property = Ins }.Property.Is(Ins);
            new InlineBuilder() { Property = Del }.Property.Is(Del);
            new InlineBuilder() { Property = Emp }.Property.Is(Emp);
        }

        [TestMethod]
        public void Property_2()
        {
            var b = new InlineBuilder();
            b.Property.Is(None);
            b.Add("abc");
            b.Property.Is(None);
            b.Property = Ins;
            b.Property.Is(Ins);
            b.Add("def");
            b.Property.Is(Ins);
            b.Property = Del;
            b.Property.Is(Del);
            b.Add("ghi");
            b.Property.Is(Del);
        }

        [TestMethod]
        public void Property_3()
        {
            var b = new InlineBuilder();
            b.Property = Ins;
            b.Add("abc");
            b.Add("def");
            b.Property = Del;
            b.Add("ghi");
            b.Property = Del;
            b.Add("jkl");
            b.Property = Emp;
            b.Add("mno");
            b.Property = Ins;
            b.Property = Emp;
            b.Add("pqr");
            b.Is(Ins, "abcdef", Del, "ghijkl", Emp, "mnopqr");
        }

        [TestMethod]
        public void IsEmpty_1()
        {
            var s = new InlineBuilder();
            s.IsEmpty.IsTrue();
            s.Count().Is(0);
            s.Append("");
            s.IsEmpty.IsTrue();
            s.Count().Is(0);
            s.Append("a");
            s.IsEmpty.IsFalse();
            s.Count().Is(1);
            s.Append("");
            s.IsEmpty.IsFalse();
            s.Count().Is(1);
            s.Append("a");
            s.IsEmpty.IsFalse();
            s.Count().Is(1);
        }

        [TestMethod]
        public void IsEmpty_2()
        {
            var s = new InlineBuilder();
            s.IsEmpty.IsTrue();
            s.Count().Is(0);
            s.Append(new InlineTag(Tag.S, "*", "*", "t1", null, null, null));
            s.IsEmpty.IsFalse();
            s.Count().Is(1);
            s.Append("");
            s.IsEmpty.IsFalse();
            s.Count().Is(1);
            s.Append("a");
            s.IsEmpty.IsFalse();
            s.Count().Is(2);
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
            s.Is("abc");
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
            AssertEx.Is(s);

            s.Append(t);
            AssertEx.Catch<ArgumentNullException>(() => s.Append((string)null));
            AssertEx.Catch<ArgumentNullException>(() => s.Append((InlineTag)null));
            s.Is(t);

            s.Append("abc");
            AssertEx.Catch<ArgumentNullException>(() => s.Append((string)null));
            AssertEx.Catch<ArgumentNullException>(() => s.Append((InlineTag)null));
            s.Is(t, new InlineText("abc"));
        }

        //[TestMethod]
        //public void Append_3()
        //{
        //    var t1 = new InlineTag(Tag.S, "*", "*", "t1", null, null, null);
        //    var t2 = new InlineTag(Tag.S, "*", "*", "t2", null, null, null);

        //    var s = new InlineBuilder() { "abc", t1, t2 };
        //    s.Is(new InlineText("abc"), t1, t2);
        //    s.Append(new InlineString(new InlineText("def"), t1));
        //    s.Is(new InlineText("abc"), t1, t2, new InlineText("def"), t1);
        //    s.Append(new InlineString());
        //    s.Is(new InlineText("abc"), t1, t2, new InlineText("def"), t1);
        //    s.Append(new InlineString(t1, t2, new InlineText("ghi")));
        //    s.Is(new InlineText("abc"), t1, t2, new InlineText("def"), t1, t1, t2, new InlineText("ghi"));
        //    s.Append(new InlineString(new InlineText("jkl"), t2));
        //    s.Is(new InlineText("abc"), t1, t2, new InlineText("def"), t1, t1, t2, new InlineText("ghijkl"), t2);
        //}

        [TestMethod]
        public void Reset_1()
        {
            var b = new InlineBuilder();
            b.IsEmpty.IsTrue();
            b.Add("abc");
            b.IsEmpty.IsFalse();
            b.Clear();
            b.IsEmpty.IsTrue();
            b.Add("def");
            b.IsEmpty.IsFalse();
            b.Clear(true);
            b.IsEmpty.IsTrue();
        }

        [TestMethod]
        public void Reset_2()
        {
            var b = new InlineBuilder();
            b.Property = Ins;
            b.Property.Is(Ins);
            b.Clear();
            b.Property.Is(None);
            b.Property = Del;
            b.Property.Is(Del);
            b.Clear(true);
            b.Property.Is(Del);
            b.Clear(false);
            b.Property.Is(None);
        }

        [TestMethod]
        public void Empty_1()
        {
            // Although undocumented, InlineBuilder should never create a new empty inline string.
            ReferenceEquals(InlineString.Empty, new InlineBuilder().ToInlineString()).IsTrue();
            ReferenceEquals(InlineString.Empty, new InlineBuilder() { Property = None }.ToInlineString()).IsTrue();
            ReferenceEquals(InlineString.Empty, new InlineBuilder() { Property = Emp }.ToInlineString()).IsTrue();
            ReferenceEquals(InlineString.Empty, new InlineBuilder() { Property = Ins }.ToInlineString()).IsTrue();
            ReferenceEquals(InlineString.Empty, new InlineBuilder() { Property = Del }.ToInlineString()).IsTrue();
            ReferenceEquals(InlineString.Empty, new InlineBuilder().Append("").ToInlineString()).IsTrue();
        }
    }
}
