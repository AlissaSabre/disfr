using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class XliffReaderCraftedTest : ReaderTestBase
    {
        [TestMethod]
        public void Read_Crafted_00()
        {
            var path = Path.Combine(IDIR, "Crafted00.xliff");
            var bundle = new XliffReader().Read(path, -1);

            bundle.Assets.Count().Is(1);
            {
                var a = bundle.Assets.ElementAt(0);
                a.Package.Is(path);
                a.Original.Is("Crafted00");
                a.SourceLang.Is("en-US");
                a.TargetLang.IsNull();
                a.TransPairs.Count().Is(0);
                a.AltPairs.Count().Is(0);
                a.Properties.Count().Is(0);
            }
        }

        [TestMethod]
        public void Read_Crafted_00a()
        {
            var path = Path.Combine(IDIR, "Crafted00a.xliff");
            var bundle = new XliffReader().Read(path, -1);

            bundle.Assets.Count().Is(1);
            {
                var a = bundle.Assets.ElementAt(0);
                a.Package.Is(path);
                a.Original.Is("Crafted00a");
                a.SourceLang.Is("en-US");
                a.TargetLang.Is("x-xyz");
                a.TransPairs.Count().Is(1);
                a.AltPairs.Count().Is(0);
                a.Properties.Count().Is(0);
                {
                    var p = a.TransPairs.ElementAt(0);
                    p.Serial.Is(1);
                    p.Id.Is("1");
                    p.Source.ToString().Is("");
                    p.Target.ToString().Is("");
                    p.SourceLang.Is("en-US");
                    p.TargetLang.Is("x-xyz");
                    p.Notes?.Count().Is(0);
                }
            }
        }

        [TestMethod]
        public void Read_Crafted_01()
        {
            var path = Path.Combine(IDIR, "Crafted01.xliff");
            var bundle = new XliffReader().Read(path, -1);

            bundle.Assets.Count().Is(1);
            {
                var a = bundle.Assets.ElementAt(0);
                a.TransPairs.Count().Is(2);
                {
                    var p = a.TransPairs.ElementAt(0);
                    p.Serial.Is(1);
                    p.Id.Is("101");
                    p.Source.ToString().Is("Simple Text");
                    p.Target.ToString().Is("Simple Text");
                    p.SourceLang.Is("en-US");
                    p.TargetLang.Is("x-xyz");
                    p.Notes?.Count().Is(0);
                }
                {
                    var p = a.TransPairs.ElementAt(1);
                    p.Serial.Is(2);
                    p.Id.Is("102");
                    p.Source.ToString().Is("abc def ghi.");
                    p.Target.ToString().Is("ABC DEF GHI.");
                    p.SourceLang.Is("en-US");
                    p.TargetLang.Is("x-xyz");
                    p.Notes?.Count().Is(0);
                }
            }
        }

        [TestMethod]
        public void Read_Crafted_02()
        {
            var path = Path.Combine(IDIR, "Crafted02.xliff");
            var bundle = new XliffReader().Read(path, -1);
            var a = bundle.Assets.ElementAt(0);
            a.TransPairs.Count().Is(7);
            {
                var p = a.TransPairs.ElementAt(0);
                p.Serial.Is(1);
                p.Id.Is("201");
                p.Source.DebuggerDisplay.Is("Various inline tags");
                p.Target.DebuggerDisplay.Is("Various inline tags");
            }
            {
                var p = a.TransPairs.ElementAt(1);
                p.Serial.Is(2);
                p.Id.Is("202");
                p.Source.DebuggerDisplay.Is("bpt/ept: abc {bpt;202.1}text{ept;202.2} def.");
                p.Target.DebuggerDisplay.Is("bpt/ept: ABC {bpt;202.1}TEXT{ept;202.2} DEF.");
                {
                    var t = p.Source.OfType<InlineTag>().ElementAt(0);
                    t.TagType.Is(Tag.B);
                    t.Id.Is("202.1");
                    t.Rid.Is("202r1");
                    t.Name.Is("bpt");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.Is("[CODE]");
                    t.Number.Is(1);
                }
                {
                    var t = p.Source.OfType<InlineTag>().ElementAt(1);
                    t.TagType.Is(Tag.E);
                    t.Id.Is("202.2");
                    t.Rid.Is("202r1");
                    t.Name.Is("ept");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.Is("[/CODE]");
                    t.Number.Is(2);
                }
                {
                    var t = p.Target.OfType<InlineTag>().ElementAt(0);
                    t.TagType.Is(Tag.B);
                    t.Id.Is("202.1");
                    t.Rid.Is("202r1");
                    t.Name.Is("bpt");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.Is("[CODE]");
                    t.Number.Is(1);
                }
                {
                    var t = p.Target.OfType<InlineTag>().ElementAt(1);
                    t.TagType.Is(Tag.E);
                    t.Id.Is("202.2");
                    t.Rid.Is("202r1");
                    t.Name.Is("ept");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.Is("[/CODE]");
                    t.Number.Is(2);
                }
            }
            {
                var p = a.TransPairs.ElementAt(2);
                p.Serial.Is(3);
                p.Id.Is("203");
                p.Source.DebuggerDisplay.Is("bx/ex: abc {bx;203.1}text{ex;203.2} def.");
                p.Target.DebuggerDisplay.Is("bx/ex: ABC {bx;203.1}TEXT{ex;203.2} DEF.");
                {
                    var t = p.Source.OfType<InlineTag>().ElementAt(0);
                    t.TagType.Is(Tag.B);
                    t.Id.Is("203.1");
                    t.Rid.Is("203r1");
                    t.Name.Is("bx");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.IsNull();
                    t.Number.Is(1);
                }
                {
                    var t = p.Source.OfType<InlineTag>().ElementAt(1);
                    t.TagType.Is(Tag.E);
                    t.Id.Is("203.2");
                    t.Rid.Is("203r1");
                    t.Name.Is("ex");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.IsNull();
                    t.Number.Is(2);
                }
                p.Target.OfType<InlineTag>().Is(p.Source.OfType<InlineTag>());
            }
            {
                var p = a.TransPairs.ElementAt(3);
                p.Serial.Is(4);
                p.Id.Is("204");
                p.Source.DebuggerDisplay.Is("g: abc {g;204.1}text{g;204.1} def.");
                p.Target.DebuggerDisplay.Is("g: ABC {g;204.1}TEXT{g;204.1} DEF.");
                {
                    var t = p.Source.OfType<InlineTag>().ElementAt(0);
                    t.TagType.Is(Tag.B);
                    t.Id.Is("204.1");
                    t.Rid.Is("204.1");
                    t.Name.Is("g");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.IsNull();
                    t.Number.Is(1);
                }
                {
                    var t = p.Source.OfType<InlineTag>().ElementAt(1);
                    t.TagType.Is(Tag.E);
                    t.Id.Is("204.1");
                    t.Rid.Is("204.1");
                    t.Name.Is("g");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.IsNull();
                    t.Number.Is(2);
                }
                p.Target.OfType<InlineTag>().Is(p.Source.OfType<InlineTag>());
            }
            {
                var p = a.TransPairs.ElementAt(4);
                p.Serial.Is(5);
                p.Id.Is("205");
                p.Source.DebuggerDisplay.Is("ph: abc {ph;205.1} def.");
                p.Target.DebuggerDisplay.Is("ph: ABC {ph;205.1} DEF.");
                {
                    var t = p.Source.OfType<InlineTag>().ElementAt(0);
                    t.TagType.Is(Tag.S);
                    t.Id.Is("205.1");
                    t.Rid.Is("205.1");
                    t.Name.Is("ph");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.Is("[CODE]");
                    t.Number.Is(1);
                }
                p.Target.OfType<InlineTag>().Is(p.Source.OfType<InlineTag>());
            }
            {
                var p = a.TransPairs.ElementAt(5);
                p.Serial.Is(6);
                p.Id.Is("206");
                p.Source.DebuggerDisplay.Is("x: abc {x;206.1} def.");
                p.Target.DebuggerDisplay.Is("x: ABC {x;206.1} DEF.");
                {
                    var t = p.Source.OfType<InlineTag>().ElementAt(0);
                    t.TagType.Is(Tag.S);
                    t.Id.Is("206.1");
                    t.Rid.Is("206.1");
                    t.Name.Is("x");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.IsNull();
                    t.Number.Is(1);
                }
                p.Target.OfType<InlineTag>().Is(p.Source.OfType<InlineTag>());
            }
            {
                var p = a.TransPairs.ElementAt(6);
                p.Serial.Is(7);
                p.Id.Is("207");
                p.Source.DebuggerDisplay.Is("it: abc {it;207.1}text{it;207.2} def.");
                p.Target.DebuggerDisplay.Is("it: ABC {it;207.1}text{it;207.2} DEF.");
                {
                    var t = p.Source.OfType<InlineTag>().ElementAt(0);
                    t.TagType.Is(Tag.B);
                    t.Id.Is("207.1");
                    t.Rid.Is("207r1");
                    t.Name.Is("it");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.Is("[CODE]");
                    t.Number.Is(1);
                }
                {
                    var t = p.Source.OfType<InlineTag>().ElementAt(1);
                    t.TagType.Is(Tag.E);
                    t.Id.Is("207.2");
                    t.Rid.Is("207r1");
                    t.Name.Is("it");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.Is("[/CODE]");
                    t.Number.Is(2);
                }
                p.Target.OfType<InlineTag>().Is(p.Source.OfType<InlineTag>());
            }
        }
    }
}
