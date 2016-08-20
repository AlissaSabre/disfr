using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class TmxReaderTest : ReaderTestBase
    {
        [TestMethod]
        public void my_memory_tmx_Test()
        {
            Comprehensive(new TmxReader(), "my_memory.tmx");
        }

        [TestMethod]
        public void XuanZang_tmx_Test()
        {
            Comprehensive(new TmxReader(), "XuanZang.tmx");
        }
    }
}
