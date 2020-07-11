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

            ParseSkeleton(file);
        }

        protected readonly Dictionary<string, string> InterSegmentsFromSkeleton = new Dictionary<string, string>();

        protected string FirstInterSegmentFromSkeleton;

        private const int MARKER_EXTRA = 4;

        protected void ParseSkeleton(XElement file)
        {
            // See if this file has a skeleton.
            var X = file.Name.Namespace;
            var skeleton = file.Element(X + "header")?.Element(X + "skl")?.Element(X + "internal-file")?.Value;
            if (skeleton == null) return;

            var intersegments = InterSegmentsFromSkeleton;

            // Break the skeleton into inter-segment contents by markers.
            int p = 0;
            string last_tuid = "";
            foreach (var group in file.Element(X + "body")?.Descendants(X + "group"))
            {
                var id = (string)group.Attribute("id");
                if (id == null) continue;
                var q = FindMarker(skeleton, id, p);
                if (q < 0) continue;
                if (q > p)
                {
                    var tuid = (string)group.Descendants(X + "trans-unit").LastOrDefault()?.Attribute("id");
                    if (tuid == null) continue;
                    intersegments[last_tuid] = skeleton.Substring(p, q - p);
                    last_tuid = tuid;
                }
                p = q + id.Length + MARKER_EXTRA;
            }

            // Record the remnant of the skeleton as the last inter-segment content.
            if (p < skeleton.Length)
            {
                intersegments[last_tuid] = skeleton.Substring(p);
            }

            // Take care of the first inter-segment content in a special way.
            intersegments.TryGetValue("", out FirstInterSegmentFromSkeleton);
        }

        private const char ORM = '\uFFFC';

        protected int FindMarker(string skeleton, string id, int start_index)
        {
            var m = "{" + id + "}";
            var x = skeleton.Length - m.Length - 2;
            for (int p = start_index + 1, q; ; p = q + m.Length)
            {
                q = skeleton.IndexOf(m, p);
                if (q < 0 || q > x) return -1;
                if (skeleton[q - 1] == ORM && skeleton[q + m.Length] == ORM) return q - 1;
            }
        }

        protected override IEnumerable<XliffTransPair> ExtractPairs(XElement tu)
        {
            // TransPairs related to the given tu are collected in this list.
            var pairs = new List<XliffTransPair>();

            // Add the first inter-segment content from the skeleton before the first trans-unit if any.
            if (FirstInterSegmentFromSkeleton != null)
            {
                pairs.Add(InterSegmentPair(new InlineString(FirstInterSegmentFromSkeleton)));
                FirstInterSegmentFromSkeleton = null;
            }

            // Add the _white_spaces_ *before* the segment content.
            foreach (var ws in tu.Elements(GS + "ws").Where(ws => (string)ws.Attribute("pos") == "before"))
            {
                pairs.Add(InterSegmentPair(GetInline(ws)));
            }

            // Add the segment content.
            // Note that Wordfast XLIFF appears not employing <mrk mtype="seg"/> segmentation,
            // so a single translation unit always produces a single pair.
            pairs.Add(ExtractSinglePair(tu));

            // Add the _white_spaces_ *after* the segment content.
            foreach (var ws in tu.Elements(GS + "ws").Where(ws => (string)ws.Attribute("pos") == "after"))
            {
                pairs.Add(InterSegmentPair(GetInline(ws)));
            }

            // Add the inter-segment content from the skeleton if any.
            string intersegment;
            if (InterSegmentsFromSkeleton.TryGetValue((string)tu.Attribute("id"), out intersegment))
            {
                pairs.Add(InterSegmentPair(new InlineString(intersegment)));
            }

            // That's all from this tu.
            return pairs;
        }

        protected XliffTransPair InterSegmentPair(InlineString inline)
        {
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
            // It looks like that a Wordfast XLIFF consistently uses ctype attributes on inline tags,
            // which is suitable for InlineTag.Display, but they are only on start tags.
            // When seeing an end tag, try to find the paired start tag to know its ctype.
            var display = (string)element.Attribute("ctype");
            if (display == null && type == Tag.E)
            {
                var rid = (string)element.Attribute("rid");
                display = element.Parent.Elements()
                    .Where(e => (string)e.Attribute("rid") == rid)
                    .Select(e => (string)e.Attribute("ctype"))
                    .FirstOrDefault(s => s != null);
            }

            // It looks like that a Wordfast XLIFF often has equiv-text attributes on inline tags.
            // It is useful for an alternative text if one is missing.
            var equiv = (string)element.Attribute("equiv-text");
            return new InlineTag(
                type: type,
                id: (string)element.Attribute("id"),
                rid: (string)element.Attribute("rid") ?? (string)element.Attribute("id"),
                name:  element.Name.LocalName,
                ctype: (string)element.Attribute("ctype"),
                display: display ?? equiv,
                code: has_code ? element.Value : equiv);
        }

        protected override void AddTargetAttrProps(XliffTransPair pair, XElement target)
        {
            // Wordfast XLIFF puts multiple segment metadata in a single attribute target/@gs4tr:seginfo
            // in a _unique_ format.
            var seginfo = target.Attribute(GS + "seginfo");
            if (seginfo != null)
            {
                try
                {
                    foreach (var attr in XElement.Parse((string)seginfo).Attributes())
                    {
                        AddProp(pair, attr.Name.LocalName, attr.Value);
                    }
                    seginfo.Remove();
                }
                catch (Exception)
                {
                }
            }
            base.AddTargetAttrProps(pair, target);
        }
    }
}
