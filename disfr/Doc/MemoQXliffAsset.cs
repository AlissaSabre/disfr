using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace disfr.Doc
{
    class MemoQXliffAsset : XliffAsset
    {
        public static readonly XNamespace MQ = XNamespace.Get("MQXliff"); // This is NOT a placeholder; it's real!  :(
        public static readonly XNamespace MQX = XNamespace.Get("MQXliff-ExternalParts");

        public MemoQXliffAsset(XElement file, ZipArchiveEntry zip_entry) : base(file)
        {
            // memoQ bilingual files appear not to support XLIFF segmentation.
            SegmentationAllowed = false;

            // Parse skeleton if this file has one and is available.
            if (zip_entry != null)
            {
                ParseSkeleton(file, zip_entry);
            }
        }

        //private const string LABEL_CONVERSION_TABLE = "LabConversionTable.dat";

        private const string DOC_MRD = "doc.mrd";

        private static readonly Regex Marker = new Regex("(?:#L?[1-9][0-9]*#)+");

        //private Dictionary<int, int[]> LabelConv;

        private string[] InterSegments = null;

        /// <summary>
        /// Tries to retrieve the corresponding skeleton info. and parses it for later use.
        /// </summary>
        /// <param name="file">XLIFF file element to find a skeleton for.</param>
        /// <param name="zip_entry">ZIP entry the XLIFF instance containing <paramref name="file"/> is read from.</param>
        protected void ParseSkeleton(XElement file, ZipArchiveEntry zip_entry)
        {
            var skl_href = (string)file.Element(X + "header")?.Element(X + "skl")?.Element(X + "external-file")?.Attribute("href");
            if (skl_href == null) return;

            var skl_entry = zip_entry.Archive.GetEntry(skl_href);
            if (skl_entry == null) return;

            MemoryStream skl_stream = null;
            using (var stream = skl_entry.Open())
            {
                XElement skeleton = null;
                try
                {
                    skeleton = XElement.Load(stream).Element(MQX + "skeleton");
                    if (skeleton == null) return;
                }
                catch (Exception)
                {
                    // Try to catch any Exceptions from LINQ to XML methods indicating XML parse errors.
                    return;
                }
                skl_stream = new MemoryStream(Convert.FromBase64String(skeleton.Value), false);
                if (skl_stream.Length == 0) return;
            }

            //Dictionary<int, int[]> labels = null;
            string[] interseg = null;
            using (var zip = new ZipArchive(skl_stream, ZipArchiveMode.Read, false, Encoding.GetEncoding(850)))
            {
                //var label_conversion_table = zip.GetEntry(LABEL_CONVERSION_TABLE);
                //if (label_conversion_table == null) return;
                //using (var stream = label_conversion_table.Open())
                //{
                //    labels = XElement.Load(stream).Elements("Item").ToDictionary(
                //        item => (int)item.Element("Key").Element("int"),
                //        item => item.Element("Value").Element("ArrayOfInt").Elements("int").Select(i => (int)i).ToArray());
                //}

                var doc = zip.GetEntry(DOC_MRD);
                if (doc == null) return;
                using (var stream = doc.Open())
                {
                    interseg = Marker.Split(new StreamReader(stream, true).ReadToEnd());
                }
                for (int i = 0; i < interseg.Length; i++)
                {
                    interseg[i] = interseg[i].Replace("##", "#");
                }
            }

#if false
            for (int n = labels.Count; n > 0; --n)
            {
                if (!labels.ContainsKey(n))
                {
                    throw new Exception("OOPS!");
                }
            }
            var w = labels.Values.SelectMany(v => v).Count();
            if (w != interseg.Length - 1)
            {
                throw new Exception("OOPS!");
            }
#endif

            //LabelConv = labels;
            InterSegments = interseg;
        }

        protected override IEnumerable<XliffTransPair> ExtractPairs(XElement tu)
        {
            // Take care of preceding inter-segment contents if we have a skeleton
            if (InterSegments != null)
            {
                // Note that we put segments from subflows, i.e., 
                // trans-unit[@mq:firstlabel="-1" and @mq:lastlabel="-1"],
                // immediately after the preceeding main-flow segments.
                // inter-segment contents are placed after the subflow segments in the case. 
                // That's why we say .LastOrDefault(label => label > 0).
                // Also note that we often have a inter-(not actually)-segment
                // contents before the first main-flow segment.
                // That's why we say last_last_label >= 0 but >.
                var first_label = (int?)tu.Attribute(MQ + "firstlabel") ?? 0;
                var prev_last_label = tu.ElementsBeforeSelf(X + "trans-unit").Select(e=> (int?)e.Attribute(MQ + "lastlabel")).LastOrDefault(label => label > 0) ?? 0;
                if (first_label > 0 && prev_last_label >= 0 && first_label != prev_last_label)
                {
                    // I assume first_label == last_last_label + 1.  I have no idea what we should do otherwise.
                    yield return InterSegmentPair(InterSegments[prev_last_label]);
                }
            }

            // Take care of hidden white spaces before segment text.
            var startingws = (string)tu.Element(MQ + "startingws");
            if (!string.IsNullOrEmpty(startingws)) yield return InterSegmentPair(startingws);

            // return the trans-unit.
            yield return ExtractSinglePair(tu);

            // Take care of hidden white spaces after segment text.
            var endingws = (string)tu.Element(MQ + "endingws");
            if (!string.IsNullOrEmpty(endingws)) yield return InterSegmentPair(endingws);

            // A special case:
            // If we have a skeleton, and if this is the last segment and is from a subflow,
            // we put the last inter-segment contents after this segment.
            // We need to look back for the previous main-flow segment to do so.
            if (InterSegments != null && !tu.ElementsAfterSelf(X + "trans-unit").Any() && (int?)tu.Attribute(MQ + "lastlabel") == -1)
            {
                var prev_last_label = tu.ElementsBeforeSelf(X + "trans-unit").Select(t => (int?)t.Attribute(MQ + "lastlabel")).LastOrDefault(label => label > 0);
                if (prev_last_label != null)
                {
                    yield return InterSegmentPair(InterSegments[(int)prev_last_label]);
                }
            }
        }

        /// <summary>
        /// Wraps an inter-segment text in an XliffTransPair.
        /// </summary>
        /// <param name="text">Inter-segment text to wrap.</param>
        /// <returns>XliffTransPair instance that wraps the given text.</returns>
        private XliffTransPair InterSegmentPair(string text)
        {
            var inline = new InlineString() { text };
            return new XliffTransPair()
            {
                Serial = -1,
                Id = "*",
                Source = inline,
                Target = inline,
                SourceLang = SourceLang,
                TargetLang = TargetLang,
            };
        }

        protected override InlineTag BuildNativeCodeTag(Tag type, XElement element, bool has_code)
        {
            if (element.Name.LocalName == "x" && element.Name.Namespace == X)
            {
                // In memoQ, x placeholder refers to a skeleton.
                string code = null;
                if (InterSegments != null)
                {
                    var tu = element.Ancestors(X + "trans-unit").First();
                    var first = (int?)tu.Attribute(MQ + "firstlabel");
                    var id0 = (int?)element.Attribute("id");
                    if (first != null && id0 != null)
                    {
                        var index = first + tu.Element(X + "source")?.Descendants(X + "x")?.Count(x => (int?)x.Attribute("id") < id0);
                        if (index > 0 && index < InterSegments.Length)
                        {
                            code = InterSegments[(int)index];
                        }
                    }
                }

                var id = (string)element.Attribute("id") ?? "*";
                return new InlineTag(
                    type: type,
                    id: id,
                    rid: id,
                    name: "x",
                    ctype: null,
                    display: "{" + id + "}",
                    code: code);
            }
            else
            {
                return base.BuildNativeCodeTag(type, element, has_code);
            }
        }

        protected override IEnumerable<string> GetNotes(XElement unit)
        {
            // memoQ appears not to use standard XLIFF note at all, but just in case.
            var standard = base.GetNotes(unit);

            var memoQ = unit.Element(MQ + "comments")?.Elements(MQ + "comment")?.Select(c => (string)c);
            if (memoQ == null)
            {
                return standard;
            }
            else
            {
                return standard.Concat(memoQ);
            }
        }
    }
}
