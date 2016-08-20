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
    class IdiomXliffAsset : XliffAsset
    {
        public static readonly XNamespace IWS = XNamespace.Get("http://www.idiominc.com/ws/asset");

        public IdiomXliffAsset(XElement file) : base(file)
        {
            // IWS package doesn't support XLIFF segmentation.
            SegmentationAllowed = false;

            // Some IWS package contains an XLIFF whose file element
            // has no original attribute,
            // storing that information in another place...
            if (Original == "") Original = (string)file.Descendants(IWS + "ais_src_path").FirstOrDefault() ?? "";
        }

        protected override IEnumerable<XliffTransPair> ExtractPairs(XElement tu)
        {
            return base.ExtractPairs(tu)
                .Concat(tu.Elements(IWS + "markup-seg").Select(ExtractMsegPair))
                .OrderBy(pair => pair.Id);
        }

        private XliffTransPair ExtractMsegPair(XElement mseg)
        {
            var markup = new InlineString() { mseg.Value };
            return new XliffTransPair()
            {
                Serial = -1,
                Id = (string)mseg.Attribute("sequence"),
                Source = markup,
                Target = markup,
                SourceLang = SourceLang,
                TargetLang = TargetLang,
            };
        }

        protected override XliffTransPair ExtractSinglePair(XElement tu)
        {
            var pair = base.ExtractSinglePair(tu);
            var metadata = tu.Element(IWS + "segment-metadata");
            if (metadata != null)
            {
                foreach (var attr in metadata.Attributes())
                {
                    AddProp(pair, attr.Name.LocalName, attr.Value);
                }
                foreach (var attr in metadata.Elements(IWS + "status").Attributes())
                {
                    AddProp(pair, attr.Name.LocalName, attr.Value);
                }
            }
            return pair;
        }

        protected override InlineTag BuildNativeCodeTag(Tag type, XElement element, bool has_code)
        {
            // WorldServer appears to place the real native code in x attribute,
            // making the contents of an element a display string, if any.
            return new InlineTag(
                type: type,
                id: (string)element.Attribute("id"),
                rid: (string)element.Attribute("rid") ?? (string)element.Attribute("id"),
                name: element.Name.LocalName,
                ctype: (string)element.Attribute("ctype"),
                display: has_code ? element.Value : null,
                code: (string)element.Attribute("x"));
        }
    }
}
