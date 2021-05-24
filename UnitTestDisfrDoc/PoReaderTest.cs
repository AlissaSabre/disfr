using System;
using System.IO;
using System.Linq;
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
    }
}
