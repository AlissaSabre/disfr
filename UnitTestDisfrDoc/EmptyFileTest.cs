using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    /// <summary>
    /// Summary description for EmptyFileTest
    /// </summary>
    [TestClass]
    public class EmptyFileTest : ReaderTestBase
    {
        [TestMethod]
        public void EmptyXliff()
        {
            TestEmpty(new XliffReader(), "empty-xliff12.xliff");
        }

        [TestMethod]
        public void EmptyTmx()
        {
            TestEmpty(new TmxReader(), "empty-tmx.tmx");
        }

        [TestMethod]
        public void EmptyPo()
        {
            TestEmpty(new disfr.po.PoReader(), "empty-gnupo.po");
        }

        private static void TestEmpty(IAssetReader reader, string filename)
        {
            var assets = reader.Read(Path.Combine(IDIR, filename), -1).Assets;
            foreach (var asset in assets)
            {
                asset.TransPairs.Count().Is(0);
            }
        }
    }
}
