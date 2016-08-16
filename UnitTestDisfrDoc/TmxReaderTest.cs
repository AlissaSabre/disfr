using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class TmxReaderTest
    {
        private const string IDIR = @"..\..\Samples";
        private const string ODIR = @"..\..\Expected";

        [TestMethod]
        public void my_memory_tmx_Test()
        {
            Comprehensive("my_memory.tmx");
        }

        [TestMethod]
        public void XuanZang_tmx_Test()
        {
            Comprehensive("XuanZang.tmx");
        }

        private static void Comprehensive(string filename)
        {
            var assets = new TmxReader().Read(Path.Combine(IDIR, filename), -1);
            var got = new PairVisualizer().Visualize(assets);

            var dumpname = Path.GetFileNameWithoutExtension(filename) + ".dump";
            var expected = File.ReadAllText(Path.Combine(ODIR, dumpname));

            got.Is(expected);
        }
    }
}
