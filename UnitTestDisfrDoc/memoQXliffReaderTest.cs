using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.Doc;

namespace UnitTestDisfrDoc
{
    [TestClass]
    public class MemoQXliffReaderTest : ReaderTestBase
    {
        [TestMethod]
        public void ChangeTracking_1()
        {
            var path = Path.Combine(IDIR, "ChangeTracking_memoQ_1.mqxliff");
            var bundle = new XliffReader().Read(path);
            bundle.Assets.Count().Is(1);
            var pairs = bundle.Assets.ElementAt(0).TransPairs.ToArray();
            {
                pairs[0].Source.ToString().Is("Change Tracking");
                pairs[1].Source.ToString().Is("Paragraph #1.");
                pairs[2].Source.ToString().Is("Paragraph #2.");
                pairs[3].Source.ToString().Is("<b>Paragraph</b> #3 with <i>tags</i>.");
                pairs[4].Source.ToString().Is("<b>Paragraph</b> #4 with <i>tags</i>.");

                pairs[0].Source.ToString(InlineToString.Debug).Is("Change Tracking");
                pairs[1].Source.ToString(InlineToString.Debug).Is("Paragraph #1.");
                pairs[2].Source.ToString(InlineToString.Debug).Is("Paragraph #2.");
                pairs[3].Source.ToString(InlineToString.Debug).Is("{bpt;1}Paragraph{ept;2} #3 with {bpt;3}tags{ept;4}.");
                pairs[4].Source.ToString(InlineToString.Debug).Is("{bpt;1}Paragraph{ept;2} #4 with {bpt;3}tags{ept;4}.");

                pairs[0].Target.ToString().Is("CHANGE TRACKING");
                pairs[1].Target.ToString().Is("#1.");
                pairs[2].Target.ToString().Is("PARAGRAPH #2 INSERTED.");
                pairs[3].Target.ToString().Is("WITH <i>TAGS</i>, <b>PARAGRAPH</b> #3.");
                pairs[4].Target.ToString().Is("<b>ABRACADABRA</b> #4 with <i>TAGS</i>.");

                pairs[0].Target.ToString(InlineToString.Normal).Is("CHANGE TRACKING");
                pairs[1].Target.ToString(InlineToString.Normal).Is("#1.");
                pairs[2].Target.ToString(InlineToString.Normal).Is("PARAGRAPH #2 INSERTED.");
                pairs[3].Target.ToString(InlineToString.Normal).Is("WITH <i>TAGS</i>, <b>PARAGRAPH</b> #3.");
                pairs[4].Target.ToString(InlineToString.Normal).Is("<b>ABRACADABRA</b> #4 with <i>TAGS</i>.");

                pairs[0].Target.ToString(InlineToString.Flat).Is("CHANGE TRACKING");
                pairs[1].Target.ToString(InlineToString.Flat).Is("#1.");
                pairs[2].Target.ToString(InlineToString.Flat).Is("PARAGRAPH #2 INSERTED.");
                pairs[3].Target.ToString(InlineToString.Flat).Is("WITH TAGS, PARAGRAPH #3.");
                pairs[4].Target.ToString(InlineToString.Flat).Is("ABRACADABRA #4 with TAGS.");

                pairs[0].Target.ToString(InlineToString.Debug).Is("CHANGE TRACKING");
                pairs[1].Target.ToString(InlineToString.Debug).Is("{Del}PARAGRAPH {None}#1.");
                pairs[2].Target.ToString(InlineToString.Debug).Is("PARAGRAPH #2{Ins} INSERTED{None}.");
                pairs[3].Target.ToString(InlineToString.Debug).Is("{Del}{bpt;1}PARAGRAPH{ept;2} #3 {None}WITH {bpt;3}TAGS{ept;4}{Ins}, {bpt;5}PARAGRAPH{ept;6} #3{None}.");
                pairs[4].Target.ToString(InlineToString.Debug).Is("{bpt;1}{Del}PARAGRAPH{Ins}ABRACADABRA{None}{ept;2} #4 with {bpt;3}TAGS{ept;4}.");
            }
        }
    }
}
