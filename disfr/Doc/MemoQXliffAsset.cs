using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace disfr.Doc
{
    class MemoQXliffAsset : XliffAsset
    {
        public static readonly XNamespace MQ = XNamespace.Get("MQXliff"); // This is NOT a placeholder; it's real!  :(

        public MemoQXliffAsset(XElement file) : base(file)
        {
            // memoQ bilingual files appear not to support XLIFF segmentation.
            SegmentationAllowed = false;
        }
    }
}
