using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;
using disfr.sdltm;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class SdltmReaderTest : ReaderTestBase
    {
        [TestMethod]
        public void tea_party_sdltm_Test()
        {
            Comprehensive(new SdltmReader(), "tea-party.sdltm");
        }
    }
}
