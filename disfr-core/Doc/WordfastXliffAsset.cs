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
    }
}
