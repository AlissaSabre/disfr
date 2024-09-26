using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;
using disfr.po;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class PoReaderTest : ReaderTestBase
    {
        [TestMethod]
        public void issue_24_1()
        {
            var a = new PoReader().Read(Path.Combine(IDIR, "issue24.po"), -1).Assets.ToArray();
            a.Length.Is(1);
            a[0].TransPairs.Count().Is(2);
            var p = a[0].TransPairs.ToArray();
            p[0].Source.ToString().Is("foo");
            p[0].Target.ToString().Is("");
            p[0].Notes.Count().Is(1);
            p[0].Notes.ElementAt(0).Is("abc");
            p[1].Source.ToString().Is("bar");
            p[1].Target.ToString().Is("");
            p[1].Notes.Count().Is(1);
            p[1].Notes.ElementAt(0).Is("def");
        }

        [TestMethod]
        public void GoodCase()
        {
            GoodFile("c-strings.po");
        }

        [TestMethod]
        public void GentleDeclines_01()
        {
            WrongFile("Crafted00.xliff");
        }

        [TestMethod]
        public void GentleDeclines_02()
        {
            WrongFile("Language_Support.doc.sdlxliff");
        }

        [TestMethod]
        public void GentleDeclines_03()
        {
            WrongFile("ChangeTracking_memoQ_1.mqxliff");
        }

        [TestMethod]
        public void GentleDeclines_04()
        {
            WrongFile("git.html_jpn.mqxlz");
        }

        [TestMethod]
        public void GentleDeclines_05()
        {
            WrongFile("The Man in the Moon_txt.txlf");
        }

        [TestMethod]
        public void GentleDeclines_06()
        {
            WrongFile("my_memory.tmx");
        }

        [TestMethod]
        public void GentleDeclines_07()
        {
            WrongFile("XuanZang.tmx");
        }

        [TestMethod]
        public void GentleDeclines_08()
        {
            WrongFile("tea-party.sdltm");
        }

        [TestMethod]
        public void GentleDeclines_09()
        {
            WrongFile("Example_Excel_Glossary.xlsx");
        }

        [TestMethod]
        public void GentleDeclines_10()
        {
            WrongFile("ChangeTracking.html");
        }

        [TestMethod]
        public void GentleDeclines_11()
        {
            WrongFile("my_memory.doc");
        }

        [TestMethod]
        public void GentleDeclines_12()
        {
            WrongFile("Readme.md");
        }

        [TestMethod]
        public void Broken_01()
        {
            TestLines(BrokenFile,
                "msgid \"\"",
                "msgstr \"\"",
                "\"Project-Id-Version: PACKAGE VERSION\\n\"",
                "\"Report-Msgid-Bugs-To: \\n\"",
                "\"POT-Creation-Date: 2021-05-27 00:00+0000\\n\"",
                "\"PO-Revision-Date: YEAR-MO-DA HO:MI+ZONE\\n\"",
                "\"Last-Translator: FULL NAME <EMAIL@ADDRESS>\\n\"",
                "\"Language-Team: LANGUAGE <LL@li.org>\\n\"",
                "\"Language: en\\n\"",
                "\"MIME-Version: 1.0\\n\"",
                "\"Content-Type: text/plain; charset=CHARSET\\n\"",
                "\"Content-Transfer-Encoding: 8bit\\n\"",
                "",
                "msgid"
            );
        }

        private void GoodFile(string filename)
        {
            new PoReader().Read(Path.Combine(IDIR, filename), -1).IsNotNull();
            new PoReader().Read(Path.Combine(IDIR, filename), 0).IsNotNull();
        }

        private void WrongFile(string filename)
        {
            new PoReader().Read(Path.Combine(IDIR, filename), -1).IsNull();
            AssertEx.Catch<Exception>(() => new PoReader().Read(Path.Combine(IDIR, filename), 0));
        }

        private void BrokenFile(string filename)
        {
            AssertEx.Catch<Exception>(() => new PoReader().Read(Path.Combine(IDIR, filename), -1));
            AssertEx.Catch<Exception>(() => new PoReader().Read(Path.Combine(IDIR, filename), 0));
        }

        private void TestLines(Action<string> test, params string[] lines)
        {
            var filename = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(filename, lines);
                test(filename);
            }
            finally
            {
                File.Delete(filename);
            }
        }
    }
}
