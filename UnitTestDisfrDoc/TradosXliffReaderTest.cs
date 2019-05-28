using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class TradosXliffReaderTest : ReaderTestBase
    {
        [TestMethod]
        public void ChangeTracking_1()
        {
            var path = Path.Combine(IDIR, "ChangeTracking_Trados_1.sdlxliff");
            var bundle = new XliffReader().Read(path);
            bundle.Assets.Count().Is(1);
            var pairs = bundle.Assets.ElementAt(0).TransPairs.ToArray();
            {
                pairs[0].Source.ToString().Is("Change Tracking");
                pairs[0].Target.ToString().Is("CHANGE TRACKING");
                pairs[1].Source.ToString().Is("Paragraph #1.");
                pairs[1].Target.ToString().Is("#1.");
                pairs[2].Source.ToString().Is("Paragraph #2.");
                pairs[2].Target.ToString().Is("PARAGRAPH #2 INSERTED.");
            }
        }
    }
}
