using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace disfr.Doc
{
    class WordfastXliffAsset : XliffAsset
    {
        public static readonly XNamespace GS = "http://www.gs4tr.org/schema/xliff-ext";

        public WordfastXliffAsset(XElement file) : base (file)
        {
            // Wordfast XLIFF seems not supporting XLIFF segmentation.
            SegmentationAllowed = false;
        }
    }
}
