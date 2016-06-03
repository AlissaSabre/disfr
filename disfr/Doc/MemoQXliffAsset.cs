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
            ParseSkeleton(file, zip_entry);
        }

        private const string DOC_MRD = "doc.mrd";

        private static readonly Regex Marker = new Regex("(?:#L?[1-9][0-9]*#)+");

        //private Dictionary<int, int[]> LabelConv;

        private string[] InterSegments = null;

        /// <summary>
        /// Tries to retrieve the corresponding skeleton info., parses it, and save it for later use.
        /// </summary>
        /// <param name="file">XLIFF file element to find a skeleton for.</param>
        /// <param name="zip_entry">ZIP entry the XLIFF instance containing <paramref name="file"/> is read from.
        /// It can be null if the XLIFF instance is not from a ZIP archive.
        /// This method does nothing in the case.</param>
        protected void ParseSkeleton(XElement file, ZipArchiveEntry zip_entry)
        {
            // Locate the external skeleton file.
            // Note that the syntax and semantics of skl element is defined in XLIFF spec.,
            // and it is allows much more various cases than the following code,
            // but I believe memoQ uses it in a single uniform pattern.

            var skl_href = (string)file.Element(X + "header")?.Element(X + "skl")?.Element(X + "external-file")?.Attribute("href");
            if (skl_href == null) return;

            var skl_entry = zip_entry?.Archive?.GetEntry(skl_href);
            if (skl_entry == null) return;

            // Read the skeleton portion of the skeleton info into a memory stream.

            MemoryStream skl_stream;
            using (var stream = skl_entry.Open())
            {
                XElement skeleton;
                try
                {
                    skeleton = XElement.Load(stream).Element(MQX + "skeleton");
                    if (skeleton == null) return;
                }
                catch (Exception)
                {
                    // Try to catch any Exceptions from LINQ to XML methods indicating XML parse errors.
                    // We should report (rethrow) others, but it is not easy to distinguish them.
                    return;
                }
                skl_stream = new MemoryStream(Convert.FromBase64String(skeleton.Value), false);
                if (skl_stream.Length == 0) return;
            }

            // Extract the file doc.mrd that contains no translatable contents of source file
            // out of the skeleton zip archive.

            string[] intersegs;
            using (var zip = new ZipArchive(skl_stream, ZipArchiveMode.Read, false, Encoding.GetEncoding(850)))
            {
                var doc = zip.GetEntry(DOC_MRD);
                if (doc == null) return;
                using (var stream = doc.Open())
                {
                    // Some memoQ import filter creates a doc.mrd which is a zip archive.
                    // This version of disfr can't handle such skeleton (unfortunately.)
                    // I think it's better to detect the case early,
                    // since such doc.mrd is often big, and Regex.Split could take some time unnecessarily.
                    //
                    // Although undocumented
                    // (and I'm not surprised to see some fundamental feature is undocumented in Microsoft's API definitions),
                    // the Stream object returned by ZipArchiveEntry.Open() has a CanSeek set to false.
                    // So, we need to call Open() twice; once to check whether it is a zip, and again to process it.
                    if (stream.ReadByte() == 0x50 &&
                        stream.ReadByte() == 0x4B &&
                        stream.ReadByte() == 0x03 &&
                        stream.ReadByte() == 0x04)
                    {
                        return; 
                    }
                }
                using (var stream = doc.Open())
                {
                    intersegs = Marker.Split(new StreamReader(stream, true).ReadToEnd());
                }
                for (int i = 0; i < intersegs.Length; i++)
                {
                    intersegs[i] = intersegs[i].Replace("##", "#");
                }

                // Sanity check against LabConversionTable.
                // Some XML based bilingual file creates an XML based doc.mrd which uses
                // XML techniques to identify insertion points of target texts,
                // which this version of disfr can't handle.
                var conv = zip.GetEntry("LabConversionTable.dat");
                if (conv == null)
                {
                    // The label conversion table is missing in the skeleton...
                    // Probably this is a totally different format of skeleton,
                    // and we should not rely on our understanding of the doc.mrd format.
                    return;
                }
                using (var stream = conv.Open())
                {
                    var count = XElement.Load(stream).Elements("Item").Count();
                    if (count + 1 != intersegs.Length) return;
                }
            }

            InterSegments = intersegs;
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
                if (first_label > 0 && prev_last_label >= 0 && first_label != prev_last_label && prev_last_label < InterSegments.Length)
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
                if (prev_last_label != null && prev_last_label >= 0 && prev_last_label < InterSegments.Length)
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

            // The following handling of mq:comment/@deleted may not be right.  FIXME.
            var memoq = unit.Element(MQ + "comments")?.Elements(MQ + "comment")?.Where(c => (string)c.Attribute("deleted") != "true")?.Select(c => (string)c);

            if (memoq == null)
            {
                return standard;
            }
            else if (standard == null)
            {
                return memoq;
            }
            else
            {
                return standard.Concat(memoq);
            }
        }
    }
}
