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
    class TradosXliffAsset : XliffAsset
    {
        public static readonly XNamespace SDL = XNamespace.Get("http://sdl.com/FileTypes/SdlXliff/1.0");

        public TradosXliffAsset(XElement file) : base(file)
        {
            var comments = file.Parent.Elements(SDL + "doc-info").Descendants(SDL + "cmt-def")
                .ToDictionary(c => (string)c.Attribute("id"), c => c.Descendants(SDL + "Comment").Select(s => s.Value.TrimEnd()).ToArray());
            SdlComments = comments.Count == 0 ? null : comments;

            var tags = file.Element(X + "header")?.Element(SDL + "tag-defs")?.Elements(SDL + "tag")?.ToArray();
            PhTags = tags?.Where(tag => tag.Element(SDL + "ph") != null)?.ToDictionary(tag => (string)tag.Attribute("id"), tag => tag);
            PtTags = tags?.Where(tag => tag.Element(SDL + "bpt") != null)?.ToDictionary(tag => (string)tag.Attribute("id"), tag => tag);
            StTags = tags?.Where(tag => tag.Element(SDL + "st") != null)?.ToDictionary(tag => (string)tag.Attribute("id"), tag => tag);

            Cxts = file.Element(X + "header")?.Element(SDL + "cxt-defs")?.Elements(SDL + "cxt-def")?.ToDictionary(d => (string)d.Attribute("id"), d => (string)d.Attribute("type"));
        }

        protected readonly Dictionary<string, string[]> SdlComments;

        protected readonly Dictionary<string, XElement> PhTags, PtTags, StTags;

        protected readonly Dictionary<string, string> Cxts;

        protected override IEnumerable<XliffTransPair> ExtractPairs(XElement tu)
        {
            // Extract pairs and bind the cxt info to the first segment from this tu.
            var pairs = base.ExtractPairs(tu);
            if (Cxts != null && tu.Parent.Name == X + "group")
            {
                var cxts = tu.Parent.Element(SDL + "cxts")?.Elements(SDL + "cxt")?.Select(cxt =>
                {
                    string t;
                    Cxts.TryGetValue((string)cxt.Attribute("id"), out t);
                    return t;
                })?.Where(t => t != null)?.Distinct();
                if (cxts != null)
                {
                    var array = (pairs is XliffTransPair[]) ? (pairs as XliffTransPair[]) : pairs.ToArray();
                    if (array.Length > 0)
                    {
                        var pair = array.FirstOrDefault(p => p.Serial > 0) ?? array[0];
                        pair.AddProp("sdl:cxt", string.Join("\r\n", cxts));
                    }
                    pairs = array;
                }
            }
            return pairs;
        }

        protected override IEnumerable<XliffTransPair> SegmentAndExtractPairs(XElement tu)
        {
            // extract segmented pairs and bind various meta info stored in sdl:seg-defs/sdl:seg elements to them.
            var seginfos = tu.Elements(SDL + "seg-defs").Elements(SDL + "seg").ToDictionary(seg => (string)seg.Attribute("id"));
            var pairs = base.SegmentAndExtractPairs(tu).ToArray();
            foreach (var pair in pairs)
            {
                XElement seg;
                if (seginfos.TryGetValue(pair.Id, out seg))
                {
                    foreach (var attr in seg.Attributes().Where(a => a.Name != "id"))
                    {
                        pair.AddProp(attr.Name.LocalName, attr.Value);
                    }
                    foreach (var value in seg.Elements(SDL + "value"))
                    {
                        pair.AddProp((string)value.Attribute("key"), value.Value);
                    }
                }
            }
            return pairs;
        }

        protected override XliffTransPair ExtractSinglePair(XElement tu)
        {
            var p = base.ExtractSinglePair(tu);
            p.Serial = -1; // Trados Studio always segments (via X:mrk), so an unsegmented tu is never for translation. 
            return p;
        }

        protected override InlineTag BuildNativeCodeTag(Tag type, XElement element, bool has_code)
        {
            var tag_name = element.Name.LocalName;
            var id = (string)element.Attribute("id");
            if (tag_name == "g")
            {
                XElement tag;
                PtTags.TryGetValue(id, out tag);
                var pt = tag?.Element(SDL + (type == Tag.B ? "bpt" : "ept"));

                return new InlineTag(
                    type: type,
                    id: id,
                    rid: (string)element.Attribute("rid") ?? id,
                    name: tag_name,
                    ctype: (string)element.Attribute("ctype"),
                    display: (string)pt?.Attribute("name"),
                    code: (string)pt?.Value);
            }
            else if (tag_name == "x" && (string)element.Ancestors(X + "trans-unit").FirstOrDefault()?.Attribute("translate") == "no")
            {
                // Oh, I'm not very sure whether translate="no" is the correct way to distinguish <st> tags...
                XElement tag;
                StTags.TryGetValue(id, out tag);
                var st = tag?.Element(SDL + "st");

                return new InlineTag(
                    type: type,
                    id: id,
                    rid: (string)element.Attribute("rid") ?? id,
                    name: tag_name,
                    ctype: (string)element.Attribute("ctype"),
                    display: (string)st?.Attribute("name"),
                    code: (string)st?.Value);
            }
            else if (tag_name == "x")
            {
                XElement tag;
                PhTags.TryGetValue(id, out tag);
                var ph = tag?.Element(SDL + "ph");

                return new InlineTag(
                    type: type,
                    id: id,
                    rid: (string)element.Attribute("rid") ?? id,
                    name: tag_name,
                    ctype: (string)element.Attribute("ctype"),
                    display: (string)ph?.Attribute("name"),
                    code: (string)ph?.Value);
            }
            else
            {
                return base.BuildNativeCodeTag(type, element, has_code);
            }
        }
    }
}
