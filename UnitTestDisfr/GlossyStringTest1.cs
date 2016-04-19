using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.UI;

namespace UnitTestDisfr
{
    [TestClass]
    public class GlossyStringTest1
    {
        [TestMethod]
        public void Ctors()
        {
            new GlossyString().Is(false);
            new GlossyString("").Is(true);
            new GlossyString("", Gloss.INS).Is(true);
            new GlossyString("abc").Is("abc", Gloss.None, true);
            new GlossyString("abc", Gloss.INS).Is("abc", Gloss.INS, true);

            new GlossyString("").ToString().Is("");
            new GlossyString("", Gloss.DEL).ToString().Is("");
        }

        [TestMethod]
        public void Append_1()
        {
            var g0 = new GlossyString();
            g0.Frozen.Is(false);
            g0.Is();
            g0.Append("");
            g0.Is();
            g0.Append("");
            g0.Is();
            g0.ToString().Is("");
            g0.Frozen.Is(false);

            var g1 = new GlossyString();
            g1.Append("abc");
            g1.Append("def");
            g1.Is("abcdef", Gloss.None);
            g1.ToString().Is("abcdef");
            g1.Frozen.Is(false);

            var g2 = new GlossyString();
            g2.Append("abc");
            g2.Append("def", Gloss.DEL);
            g2.Is("abc", Gloss.None, "def", Gloss.DEL);
            g2.ToString().Is("abcdef");
            g2.Frozen.Is(false);

            var g3 = new GlossyString();
            g3.Append("abc", Gloss.INS);
            g3.Append("def", Gloss.DEL);
            g3.Append("ghi", Gloss.DEL);
            g3.Append("jkl", Gloss.INS);
            g3.Is("abc", Gloss.INS, "defghi", Gloss.DEL, "jkl", Gloss.INS);
            g3.ToString().Is("abcdefghijkl");
            g3.Frozen.Is(false);

            var g4 = new GlossyString();
            g4.Append("abc");
            g4.Append("");
            g4.Append("def");
            g4.Is("abcdef", Gloss.None);
            g4.ToString().Is("abcdef");
            g4.Frozen.Is(false);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Append_null_1()
        {
            new GlossyString().Append(null);
        }

        [TestMethod]
        public void CollectionInitializers()
        {
            new GlossyString() { "abc" }.Is("abc", Gloss.None);

            new GlossyString() { "abc", "def", "ghi" }.Is("abcdefghi", Gloss.None);

            new GlossyString() { "abc", Gloss.TAG, "def" }.Is("abc", Gloss.TAG, "def", Gloss.None);
        }

        [TestMethod,ExpectedException(typeof(InvalidOperationException))]
        public void CollectionInitializer_Exception_1()
        {
            new GlossyString() { "abc", Gloss.TAG, Gloss.INS };
        }

        [TestMethod]
        public void Frozen_1()
        {
            var g1 = new GlossyString() { "abc", Gloss.HIT, true };
            g1.Frozen.Is(true);
            AssertEx.Catch<InvalidOperationException>(() => g1.Append("xyz"));
            AssertEx.Catch<InvalidOperationException>(() => g1.Add("xyz"));
            AssertEx.Catch<InvalidOperationException>(() => g1.Add(Gloss.INS));
            g1.Frozen = true; // This is allowed even for a frozen GlossyString.
            AssertEx.Catch<InvalidOperationException>(() => g1.Add(true));
            AssertEx.Catch<InvalidOperationException>(() => g1.Frozen = false);
            AssertEx.Catch<InvalidOperationException>(() => g1.Add(false));
            g1.Frozen.Is(true);
        }
    }

    public static class GlossyString_ExtensionMethods
    {
        public static void Is(this GlossyString g, params object[] contents)
        {
            bool frozen = (contents.Length % 2 != 0) && true.Equals(contents[contents.Length - 1]);

            var pairs = g.AsCollection().ToArray();

            try
            {
                if (pairs.Length != contents.Length / 2) goto FAILED;

                if (g.Frozen != frozen) goto FAILED;

                for (int i = 0, j = 0; i < pairs.Length; i++, j += 2)
                {
                    if (pairs[i].Text != (string)contents[j]) goto FAILED;
                    if (pairs[i].Gloss != (Gloss)contents[j + 1]) goto FAILED;
                }
            }
            catch (Exception e)
            {
                Assert.Fail(" Unexpected Exception in GlossyString.Is: " + e.ToString());
            }

            return;

          FAILED:

            var s0 = new StringBuilder();
            for (var i = 0; i < contents.Length - 1; i += 2)
            {
                s0.AppendFormat("\"{0}\",{1};", contents[i], contents[i + 1]);
            }
            s0.Append(frozen ? "FROZEN" : "unfrozen");

            var s1 = new StringBuilder();
            foreach (var p in pairs)
            {
                s1.AppendFormat("\"{0}\",{1};", p.Text, p.Gloss);
            }
            s1.Append(g.Frozen ? "FROZEN" : "unfrozen");

            Assert.Fail(" GlossyString.Is.\n Expect:\t[{0}]\n Actual:\t[{1}]", s0, s1);
        }
    }
}
