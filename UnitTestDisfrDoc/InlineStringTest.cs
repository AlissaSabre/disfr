using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class InlineStringTest : InlineStringTestBase
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
            var t1 = new InlineTag(Tag.S, "*", "*", "t", null, "{t}", null);
            var t2 = new InlineTag(Tag.S, "*", "*", "t", null, "{T}", null);

            var s1 = new InlineBuilder();
            var s2 = new InlineBuilder();

            s1.Append(t1);
            s2.Append(t2);
            s1.ToInlineString().Equals(s2.ToInlineString()).IsFalse();
            s2.ToInlineString().Equals(s1.ToInlineString()).IsFalse();
        }

        [TestMethod]
        public void Equals_with_properties_1()
        {
            new InlineBuilder() { "abc", "def" }.ToInlineString().Equals(new InlineBuilder() { "abcdef" }.ToInlineString()).IsTrue();
            new InlineBuilder() { "abc", None, "def" }.ToInlineString().Equals(new InlineBuilder() { "abcdef" }.ToInlineString()).IsTrue();

            new InlineBuilder() { "abc", Ins, "def" }.ToInlineString().Equals(new InlineBuilder() { "abcdef" }.ToInlineString()).IsFalse();
            new InlineBuilder() { "abc", Del, "def" }.ToInlineString().Equals(new InlineBuilder() { "abcdef" }.ToInlineString()).IsFalse();
            new InlineBuilder() { "abc", Emp, "def" }.ToInlineString().Equals(new InlineBuilder() { "abcdef" }.ToInlineString()).IsFalse();
            new InlineBuilder() { "abc", Ins, "def" }.ToInlineString().Equals(new InlineBuilder() { "abc", Ins, "def" }.ToInlineString()).IsTrue();
            new InlineBuilder() { "abc", Del, "def" }.ToInlineString().Equals(new InlineBuilder() { "abc", Ins, "def" }.ToInlineString()).IsFalse();
            new InlineBuilder() { "abc", Emp, "def" }.ToInlineString().Equals(new InlineBuilder() { "abc", Ins, "def" }.ToInlineString()).IsFalse();
            new InlineBuilder() { "abc", Ins, "def" }.ToInlineString().Equals(new InlineBuilder() { "abc", Del, "def" }.ToInlineString()).IsFalse();
            new InlineBuilder() { "abc", Del, "def" }.ToInlineString().Equals(new InlineBuilder() { "abc", Del, "def" }.ToInlineString()).IsTrue();
            new InlineBuilder() { "abc", Emp, "def" }.ToInlineString().Equals(new InlineBuilder() { "abc", Del, "def" }.ToInlineString()).IsFalse();
            new InlineBuilder() { "abc", Ins, "def" }.ToInlineString().Equals(new InlineBuilder() { "abc", Emp, "def" }.ToInlineString()).IsFalse();
            new InlineBuilder() { "abc", Del, "def" }.ToInlineString().Equals(new InlineBuilder() { "abc", Emp, "def" }.ToInlineString()).IsFalse();
            new InlineBuilder() { "abc", Emp, "def" }.ToInlineString().Equals(new InlineBuilder() { "abc", Emp, "def" }.ToInlineString()).IsTrue();

            new InlineBuilder() { Ins, "abc" }.ToInlineString().Equals(new InlineBuilder() { Ins, new string(new[] { 'a', 'b', 'c' }) }.ToInlineString()).IsTrue();
            new InlineBuilder() { Del, "abc" }.ToInlineString().Equals(new InlineBuilder() { Del, new string(new[] { 'a', 'b', 'c' }) }.ToInlineString()).IsTrue();
            new InlineBuilder() { Emp, "abc" }.ToInlineString().Equals(new InlineBuilder() { Emp, new string(new[] { 'a', 'b', 'c' }) }.ToInlineString()).IsTrue();
            new InlineBuilder() { Ins, "abc" }.ToInlineString().Equals(new InlineBuilder() { Ins, "xyz" }.ToInlineString()).IsFalse();
            new InlineBuilder() { Del, "abc" }.ToInlineString().Equals(new InlineBuilder() { Del, "xyz" }.ToInlineString()).IsFalse();
            new InlineBuilder() { Emp, "abc" }.ToInlineString().Equals(new InlineBuilder() { Emp, "xyz" }.ToInlineString()).IsFalse();
        }

        [TestMethod]
        public void Equals_with_properties_2()
        {
            InlineString[] samples =
            {
                new InlineString(),
                new InlineString("abc"),
                new InlineBuilder() { "abc" }.ToInlineString(),
                new InlineBuilder() { Ins, "abc" }.ToInlineString(),
                new InlineBuilder() { Del, "abc" }.ToInlineString(),
                new InlineBuilder() { Emp, "abc" }.ToInlineString(),
                new InlineBuilder() { new string(new[] { 'a', 'b', 'c' }) }.ToInlineString(),
                new InlineBuilder() { Ins, new string(new[] { 'a', 'b', 'c' }) }.ToInlineString(),
                new InlineBuilder() { Del, new string(new[] { 'a', 'b', 'c' }) }.ToInlineString(),
                new InlineBuilder() { Emp, new string(new[] { 'a', 'b', 'c' }) }.ToInlineString(),
                new InlineBuilder() { "abc", None, "def" }.ToInlineString(),
                new InlineBuilder() { "abc", Ins, "def" }.ToInlineString(),
                new InlineBuilder() { "abc", Del, "def" }.ToInlineString(),
                new InlineBuilder() { "abc", Emp, "def" }.ToInlineString(),
            };

            for (int i = 0; i < samples.Length; i++)
            {
                for (int j = 0; j < samples.Length; j++)
                {
                    var msg = string.Format("i = {0}, j = {1}", i, j);
                    var x = samples[i].Equals(samples[j]);
                    (samples[i] == samples[j]).Is(x, msg);
                    (samples[i] != samples[j]).Is(!x, msg);
                }
            }
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

        [TestMethod]
        public void ToString_1()
        {
            var F = InlineString.RenderFlat;
            var N = InlineString.RenderNormal;
            var L = InlineString.RenderOlder;
            var D = InlineString.RenderDebug;
            {
                var s = new InlineBuilder().ToInlineString();
                s.ToString(F).Is("");
                s.ToString(N).Is("");
                s.ToString(L).Is("");
                s.ToString(D).Is("");
            }
            {
                var s = new InlineBuilder() { "abc" }.ToInlineString();
                s.ToString(F).Is("abc");
                s.ToString(N).Is("abc");
                s.ToString(L).Is("abc");
                s.ToString(D).Is("abc");
            }
            {
                var s = new InlineBuilder() { Ins, "abc", Del, "def" }.ToInlineString();
                s.ToString(F).Is("abc");
                s.ToString(N).Is("abc");
                s.ToString(L).Is("def");
                s.ToString(D).Is("{Ins}abc{Del}def");
            }
            {
                var s = new InlineBuilder() { { Tag.S, "id", "rid", "name" } }.ToInlineString();
                s.ToString(F).Is("");
                s.ToString(N).Is("{*}");
                s.ToString(L).Is("{*}");
                s.ToString(D).Is("{name;id}");
            }
            {
                var s = new InlineBuilder() { { Tag.S, "id", "rid", "name", "ctype", "display", "<code>" } }.ToInlineString();
                s.ToString(F).Is("");
                s.ToString(N).Is("<code>");
                s.ToString(L).Is("<code>");
                s.ToString(D).Is("{name;id}");
            }
            {
                var s = new InlineBuilder() { Ins, { Tag.S, "id", "rid", "name", "ctype", "display", "<code>" }, Del, "abc" }.ToInlineString();
                s.ToString(F).Is("");
                s.ToString(N).Is("<code>");
                s.ToString(L).Is("abc");
                s.ToString(D).Is("{Ins}{name;id}{Del}abc");
            }
            {
                var s = new InlineBuilder() { Del, { Tag.S, "id", "rid", "name", "ctype", "display", "<code>" }, Ins, "abc" }.ToInlineString();
                s.ToString(F).Is("abc");
                s.ToString(N).Is("abc");
                s.ToString(L).Is("<code>");
                s.ToString(D).Is("{Del}{name;id}{Ins}abc");
            }
        }
    }
}
