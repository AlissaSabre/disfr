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
            var bundle = new XliffReader().Read(path);

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
            var bundle = new XliffReader().Read(path);

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
            var bundle = new XliffReader().Read(path);

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
            var bundle = new XliffReader().Read(path);
            var a = bundle.Assets.ElementAt(0);
            a.TransPairs.Count().Is(7);
            {
                var p = a.TransPairs.ElementAt(0);
                p.Serial.Is(1);
                p.Id.Is("201");
                p.Source.DebuggerDisplay.Is("Various inline tags for: abc [CODE]text[/CODE] def.");
                p.Target.DebuggerDisplay.Is("Various inline tags for: ABC [CODE]TEXT[/CODE] DEF.");
            }
            {
                var p = a.TransPairs.ElementAt(1);
                p.Serial.Is(2);
                p.Id.Is("202");
                p.Source.DebuggerDisplay.Is("bpt/ept: abc {bpt;202.1}text{ept;202.2} def.");
                p.Target.DebuggerDisplay.Is("bpt/ept: ABC {bpt;202.1}TEXT{ept;202.2} DEF.");
                {
                    var t = p.Source.Tags.ElementAt(0);
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
                    var t = p.Source.Tags.ElementAt(1);
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
                    var t = p.Target.Tags.ElementAt(0);
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
                    var t = p.Target.Tags.ElementAt(1);
                    t.TagType.Is(Tag.E);
                    t.Id.Is("202.2");
                    t.Rid.Is("202r1");
                    t.Name.Is("ept");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.Is("[/CODE]");
                    t.Number.Is(2);
                }
                p.Target.Tags.Is(p.Source.Tags);
            }
            {
                var p = a.TransPairs.ElementAt(2);
                p.Serial.Is(3);
                p.Id.Is("203");
                p.Source.DebuggerDisplay.Is("bx/ex: abc {bx;203.1}text{ex;203.2} def.");
                p.Target.DebuggerDisplay.Is("bx/ex: ABC {bx;203.1}TEXT{ex;203.2} DEF.");
                {
                    var t = p.Source.Tags.ElementAt(0);
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
                    var t = p.Source.Tags.ElementAt(1);
                    t.TagType.Is(Tag.E);
                    t.Id.Is("203.2");
                    t.Rid.Is("203r1");
                    t.Name.Is("ex");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.IsNull();
                    t.Number.Is(2);
                }
                p.Target.Tags.Is(p.Source.Tags);
            }
            {
                var p = a.TransPairs.ElementAt(3);
                p.Serial.Is(4);
                p.Id.Is("204");
                p.Source.DebuggerDisplay.Is("g: abc {g;204.1}text{g;204.1} def.");
                p.Target.DebuggerDisplay.Is("g: ABC {g;204.1}TEXT{g;204.1} DEF.");
                {
                    var t = p.Source.Tags.ElementAt(0);
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
                    var t = p.Source.Tags.ElementAt(1);
                    t.TagType.Is(Tag.E);
                    t.Id.Is("204.1");
                    t.Rid.Is("204.1");
                    t.Name.Is("g");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.IsNull();
                    t.Number.Is(2);
                }
                p.Target.Tags.Is(p.Source.Tags);
            }
            {
                var p = a.TransPairs.ElementAt(4);
                p.Serial.Is(5);
                p.Id.Is("205");
                p.Source.DebuggerDisplay.Is("ph: abc {ph;205.1}text{ph;205.2} def.");
                p.Target.DebuggerDisplay.Is("ph: ABC {ph;205.1}TEXT{ph;205.2} DEF.");
                {
                    var t = p.Source.Tags.ElementAt(0);
                    t.TagType.Is(Tag.S);
                    t.Id.Is("205.1");
                    t.Rid.Is("205.1");
                    t.Name.Is("ph");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.Is("[CODE]");
                    t.Number.Is(1);
                }
                {
                    var t = p.Source.Tags.ElementAt(1);
                    t.TagType.Is(Tag.S);
                    t.Id.Is("205.2");
                    t.Rid.Is("205.2");
                    t.Name.Is("ph");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.Is("[/CODE]");
                    t.Number.Is(2);
                }
                p.Target.Tags.Is(p.Source.Tags);
            }
            {
                var p = a.TransPairs.ElementAt(5);
                p.Serial.Is(6);
                p.Id.Is("206");
                p.Source.DebuggerDisplay.Is("x: abc {x;206.1}text{x;206.2} def.");
                p.Target.DebuggerDisplay.Is("x: ABC {x;206.1}TEXT{x;206.2} DEF.");
                {
                    var t = p.Source.Tags.ElementAt(0);
                    t.TagType.Is(Tag.S);
                    t.Id.Is("206.1");
                    t.Rid.Is("206.1");
                    t.Name.Is("x");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.IsNull();
                    t.Number.Is(1);
                }
                {
                    var t = p.Source.Tags.ElementAt(1);
                    t.TagType.Is(Tag.S);
                    t.Id.Is("206.2");
                    t.Rid.Is("206.2");
                    t.Name.Is("x");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.IsNull();
                    t.Number.Is(2);
                }
                p.Target.Tags.Is(p.Source.Tags);
            }
            {
                var p = a.TransPairs.ElementAt(6);
                p.Serial.Is(7);
                p.Id.Is("207");
                p.Source.DebuggerDisplay.Is("it: abc {it;207.1}text{it;207.2} def.");
                p.Target.DebuggerDisplay.Is("it: ABC {it;207.1}TEXT{it;207.2} DEF.");
                {
                    var t = p.Source.Tags.ElementAt(0);
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
                    var t = p.Source.Tags.ElementAt(1);
                    t.TagType.Is(Tag.E);
                    t.Id.Is("207.2");
                    t.Rid.Is("207r1");
                    t.Name.Is("it");
                    t.Ctype.IsNull();
                    t.Display.IsNull();
                    t.Code.Is("[/CODE]");
                    t.Number.Is(2);
                }
                p.Target.Tags.Is(p.Source.Tags);
            }
        }

        [TestMethod]
        public void Read_Crafted_03()
        {
            var path = Path.Combine(IDIR, "Crafted03.xliff");
            var bundle = new XliffReader().Read(path);
            var a = bundle.Assets.ElementAt(0);
            a.TransPairs.Count().Is(7);
            {
                var p = a.TransPairs.ElementAt(0);
                p.Id.Is("301");
                p.Source.DebuggerDisplay.Is("Reordering of tags: abc [CODE1]text1[/CODE1] def [CODE2]text2[/CODE2] ghi.");
                p.Target.DebuggerDisplay.Is("Reordering of tags: ABC [CODE2]TEXT2[/CODE2] DEF [CODE1]TEXT1[/CODE1] GHI.");
            }
            {
                var p = a.TransPairs.ElementAt(1);
                p.Id.Is("302");
                p.Source.DebuggerDisplay.Is("bpt/ept: abc {bpt;302.1}text1{ept;302.2} def {bpt;302.3}text2{ept;302.4} ghi.");
                p.Target.DebuggerDisplay.Is("bpt/ept: ABC {bpt;302.3}TEXT2{ept;302.4} DEF {bpt;302.1}TEXT1{ept;302.2} GHI.");
                VerifyTagOrderChanged(p);
            }
            {
                var p = a.TransPairs.ElementAt(2);
                p.Id.Is("303");
                p.Source.DebuggerDisplay.Is("bx/ex: abc {bx;303.1}text1{ex;303.2} def {bx;303.3}text1{ex;303.4} ghi.");
                p.Target.DebuggerDisplay.Is("bx/ex: ABC {bx;303.3}TEXT2{ex;303.4} DEF {bx;303.1}TEXT1{ex;303.2} GHI.");
                VerifyTagOrderChanged(p);
            }
            {
                var p = a.TransPairs.ElementAt(3);
                p.Serial.Is(4);
                p.Id.Is("304");
                p.Source.DebuggerDisplay.Is("g: abc {g;304.1}text1{g;304.1} def {g;304.2}text2{g;304.2} ghi.");
                p.Target.DebuggerDisplay.Is("g: ABC {g;304.2}TEXT2{g;304.2} DEF {g;304.1}TEXT1{g;304.1} GHI.");
                VerifyTagOrderChanged(p);
            }
            {
                var p = a.TransPairs.ElementAt(4);
                p.Serial.Is(5);
                p.Id.Is("305");
                p.Source.DebuggerDisplay.Is("ph: abc {ph;305.1}text1{ph;305.2} def {ph;305.3}text2{ph;305.4} ghi.");
                p.Target.DebuggerDisplay.Is("ph: ABC {ph;305.3}TEXT2{ph;305.4} DEF {ph;305.1}TEXT1{ph;305.2} GHI.");
                VerifyTagOrderChanged(p);
            }
            {
                var p = a.TransPairs.ElementAt(5);
                p.Serial.Is(6);
                p.Id.Is("306");
                p.Source.DebuggerDisplay.Is("x: abc {x;306.1}text1{x;306.2} def {x;306.3}text2{x;306.4} ghi.");
                p.Target.DebuggerDisplay.Is("x: ABC {x;306.3}TEXT2{x;306.4} DEF {x;306.1}TEXT1{x;306.2} GHI.");
                var s = p.Source.Tags.ToArray();
                var t = p.Target.Tags.ToArray();
                VerifyTagOrderChanged(p);
            }
            {
                var p = a.TransPairs.ElementAt(6);
                p.Serial.Is(7);
                p.Id.Is("307");
                p.Source.DebuggerDisplay.Is("it: abc {it;307.1}text1{it;307.2} def {it;307.3}text2{it;307.4} ghi.");
                p.Target.DebuggerDisplay.Is("it: ABC {it;307.3}TEXT2{it;307.4} DEF {it;307.1}TEXT1{it;307.2} GHI.");
                VerifyTagOrderChanged(p);
            }
        }

        private static void VerifyTagOrderChanged(ITransPair p)
        {
            var s = p.Source.Tags.ToArray();
            var t = p.Target.Tags.ToArray();
            s[0].IsNot(t[0]);
            s[0].IsNot(t[1]);
            s[0].Is(t[2]);
            s[0].IsNot(t[3]);
            s[1].IsNot(t[0]);
            s[1].IsNot(t[1]);
            s[1].IsNot(t[2]);
            s[1].Is(t[3]);
            s[2].Is(t[0]);
            s[2].IsNot(t[1]);
            s[2].IsNot(t[2]);
            s[2].IsNot(t[3]);
            s[3].IsNot(t[0]);
            s[3].Is(t[1]);
            s[3].IsNot(t[2]);
            s[3].IsNot(t[3]);
        }

        [TestMethod]
        public void Read_Crafted_04()
        {
            var path = Path.Combine(IDIR, "Crafted04.xliff");
            var bundle = new XliffReader().Read(path);
            var a = bundle.Assets.ElementAt(0);
            var pairs = a.TransPairs.ToArray();
            {
                var p = pairs[0];
                p.Id.Is("401a");
                p.Source.DebuggerDisplay.Is("Segmentation");
                p.Target.DebuggerDisplay.Is("SEGMENTATION");
            }
            {
                var p = pairs[1];
                p.Source.DebuggerDisplay.Is("  ");
                p.Target.DebuggerDisplay.Is("  ");
            }
            {
                var p = pairs[2];
                p.Id.Is("402a");
                p.Source.DebuggerDisplay.Is("Spaces are excluded from the segment.");
                p.Target.DebuggerDisplay.Is("SPACES ARE EXCLUDED FROM THE SEGMENT.");
            }
            {
                var p = pairs[3];
                p.Source.DebuggerDisplay.Is("  ");
                p.Target.DebuggerDisplay.Is("  ");
            }
            {
                var p = pairs[4];
                p.Id.Is("403a");
                p.Source.DebuggerDisplay.Is("Multiple sentences.");
                p.Target.DebuggerDisplay.Is("MULTIPLE SENTENCES.");
            }
            {
                var p = pairs[5];
                p.Source.DebuggerDisplay.Is("  ");
                p.Target.DebuggerDisplay.Is("  ");
            }
            {
                var p = pairs[6];
                p.Id.Is("403b");
                p.Source.DebuggerDisplay.Is("In a trans-unit.");
                p.Target.DebuggerDisplay.Is("IN A trans-unit.");
            }
            {
                var p = pairs[7];
                p.Source.DebuggerDisplay.Is("  ");
                p.Target.DebuggerDisplay.Is("  ");
            }
            {
                var p = pairs[8];
                p.Id.Is("403c");
                p.Source.DebuggerDisplay.Is("Broken into segments.");
                p.Target.DebuggerDisplay.Is("BROKEN INTO SEGMENTS.");
            }
            {
                var p = pairs[9];
                p.Source.DebuggerDisplay.Is("  ");
                p.Target.DebuggerDisplay.Is("  ");
            }
            {
                var p = pairs[10];
                p.Source.DebuggerDisplay.Is("\u0009{g;404.1}{ph;404.2} ");
                p.Target.DebuggerDisplay.Is("\u0009{g;404.1}{ph;404.2} ");
            }
            {
                var p = pairs[11];
                p.Id.Is("404a");
                p.Source.DebuggerDisplay.Is("Multiple sentences.");
                p.Target.DebuggerDisplay.Is("MULTIPLE SENTENCES.");
            }
            {
                var p = pairs[12];
                p.Source.DebuggerDisplay.Is("  ");
                p.Target.DebuggerDisplay.Is("  ");
            }
            {
                var p = pairs[13];
                p.Id.Is("404b");
                p.Source.DebuggerDisplay.Is("In a single {g;404.3}g{g;404.3}.");
                p.Target.DebuggerDisplay.Is("IN A SINGLE {g;404.3}g{g;404.3}.");
            }
            {
                var p = pairs[14];
                p.Source.DebuggerDisplay.Is(" {ph;404.4}{g;404.1} ");
                p.Target.DebuggerDisplay.Is(" {ph;404.4}{g;404.1} ");
            }
            pairs.Length.Is(15);
        }
    }
}
