using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfr
{
    [TestClass]
    public class SdltmReaderTest
    {
        [TestMethod]
        public void SdlReaderTestMethod1()
        {
            var rd = new SdltmReader();
            var entries = rd.Read(@"..\..\..\..\PRIVATE\sample.sdltm", -1);
        }
    }
}
