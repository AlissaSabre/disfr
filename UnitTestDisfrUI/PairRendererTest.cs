using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;
using disfr.UI;

namespace UnitTestDisfrUI
{
    [TestClass]
    public class PairRendererTest
    {
        private static InlineString Inline(string s)
        {
            return new InlineString() { s };
        }

        [TestMethod]
        public void GlossyFromInline_FlatString_1a()
        {
            var pr = new PairRenderer() { ShowSpecials = false };
            var glossy = pr.GlossyFromInline(Inline(""));
            var c = glossy.AsCollection();
            c.Count.Is(0);
        }

        [TestMethod]
        public void GlossyFromInline_FlatString_1b()
        {
            var pr = new PairRenderer() { ShowSpecials = true };
            var glossy = pr.GlossyFromInline(Inline(""));
            var c = glossy.AsCollection();
            c.Count.Is(0);
        }

        [TestMethod]
        public void GlossyFromInline_FlatString_2a()
        {
            var pr = new PairRenderer() { ShowSpecials = false };
            var glossy = pr.GlossyFromInline(Inline("abc"));
            var c = glossy.AsCollection();
            c.Count.Is(1);
            c.ElementAt(0).Gloss.Is(Gloss.None);
            c.ElementAt(0).Text.Is("abc");
        }

        [TestMethod]
        public void GlossyFromInline_FlatString_2b()
        {
            var pr = new PairRenderer() { ShowSpecials = true };
            var glossy = pr.GlossyFromInline(Inline("abc"));
            var c = glossy.AsCollection();
            c.Count.Is(1);
            c.ElementAt(0).Gloss.Is(Gloss.None);
            c.ElementAt(0).Text.Is("abc");
        }

        [TestMethod]
        public void GlossyFromInline_FlatString_3a()
        {
            var pr = new PairRenderer() { ShowSpecials = false };
            var glossy = pr.GlossyFromInline(Inline(" "));
            var c = glossy.AsCollection();
            c.Count.Is(1);
            c.ElementAt(0).Gloss.Is(Gloss.None);
            c.ElementAt(0).Text.Is("\u0020");
        }

        [TestMethod]
        public void GlossyFromInline_FlatString_3b()
        {
            var pr = new PairRenderer() { ShowSpecials = true };
            var glossy = pr.GlossyFromInline(Inline(" "));
            var c = glossy.AsCollection();
            c.Count.Is(2);
            c.ElementAt(0).Gloss.Is(Gloss.SYM);
            c.ElementAt(0).Text.Is("\u0020");
            c.ElementAt(1).Gloss.Is(Gloss.ALT);
            c.ElementAt(1).Text.Is("\u22C5\u200B");
        }

        [TestMethod]
        public void GlossyFromInline_FlatString_4a()
        {
            var pr = new PairRenderer() { ShowSpecials = false };
            var glossy = pr.GlossyFromInline(Inline("a b"));
            var c = glossy.AsCollection();
            c.Count.Is(1);
            c.ElementAt(0).Gloss.Is(Gloss.None);
            c.ElementAt(0).Text.Is("a\u0020b");
        }

        [TestMethod]
        public void GlossyFromInline_FlatString_4b()
        {
            var pr = new PairRenderer() { ShowSpecials = true };
            var glossy = pr.GlossyFromInline(Inline("a b"));
            var c = glossy.AsCollection();
            c.Count.Is(4);
            c.ElementAt(0).Gloss.Is(Gloss.None);
            c.ElementAt(0).Text.Is("a");
            c.ElementAt(1).Gloss.Is(Gloss.SYM);
            c.ElementAt(1).Text.Is("\u0020");
            c.ElementAt(2).Gloss.Is(Gloss.ALT);
            c.ElementAt(2).Text.Is("\u22C5\u200B");
            c.ElementAt(3).Gloss.Is(Gloss.None);
            c.ElementAt(3).Text.Is("b");
        }

        [TestMethod]
        public void GlossyFromInline_FlatString_5a()
        {
            var pr = new PairRenderer() { ShowSpecials = false };
            var glossy = pr.GlossyFromInline(Inline("\u0000\u0009\u000A\u000D\u001F\u00A0\u3000\uFEFF"));
            var c = glossy.AsCollection();
            c.Count.Is(1);
            c.ElementAt(0).Gloss.Is(Gloss.None);
            c.ElementAt(0).Text.Is("\u0000\u0009\u000A\u000D\u001F\u00A0\u3000\uFEFF");
        }

        [TestMethod]
        public void GlossyFromInline_FlatString_5b()
        {
            var pr = new PairRenderer() { ShowSpecials = true };
            var glossy = pr.GlossyFromInline(Inline("\u0000\u0009\u000A\u000D\u001F\u00A0\u3000\uFEFF"));
            var c = glossy.AsCollection();
            c.Count.Is(16);
            c.ElementAt(0).Gloss.Is(Gloss.SYM);
            c.ElementAt(0).Text.Is("\u0000");
            c.ElementAt(1).Gloss.Is(Gloss.ALT);
            c.ElementAt(1).Text.Is("(U+0000)");
            c.ElementAt(2).Gloss.Is(Gloss.SYM);
            c.ElementAt(2).Text.Is("\u0009");
            c.ElementAt(3).Gloss.Is(Gloss.ALT);
            c.ElementAt(3).Text.Is("\u2192\u0009");
            c.ElementAt(4).Gloss.Is(Gloss.SYM);
            c.ElementAt(4).Text.Is("\u000A");
            c.ElementAt(5).Gloss.Is(Gloss.ALT);
            c.ElementAt(5).Text.Is("\u21B5\u000A");
            c.ElementAt(6).Gloss.Is(Gloss.SYM);
            c.ElementAt(6).Text.Is("\u000D");
            c.ElementAt(7).Gloss.Is(Gloss.ALT);
            c.ElementAt(7).Text.Is("\u2190");
            c.ElementAt(8).Gloss.Is(Gloss.SYM);
            c.ElementAt(8).Text.Is("\u001F");
            c.ElementAt(9).Gloss.Is(Gloss.ALT);
            c.ElementAt(9).Text.Is("(U+001F)");
            c.ElementAt(10).Gloss.Is(Gloss.SYM);
            c.ElementAt(10).Text.Is("\u00A0");
            c.ElementAt(11).Gloss.Is(Gloss.ALT);
            c.ElementAt(11).Text.Is("\u00AC");
            c.ElementAt(12).Gloss.Is(Gloss.SYM);
            c.ElementAt(12).Text.Is("\u3000");
            c.ElementAt(13).Gloss.Is(Gloss.ALT);
            c.ElementAt(13).Text.Is("\u2610\u200B");
            c.ElementAt(14).Gloss.Is(Gloss.SYM);
            c.ElementAt(14).Text.Is("\uFEFF");
            c.ElementAt(15).Gloss.Is(Gloss.ALT);
            c.ElementAt(15).Text.Is("(U+FEFF)");
        }
    }
}
