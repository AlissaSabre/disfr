using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using disfr.UI;
using disfr.Doc;

namespace disfr.Writer
{
    public class TmxWriter : IRowsWriter
    {
        private static readonly string[] _FilterString = new[]
        {
            "TMX Translation Memory|*.tmx",
        };

        public IList<string> FilterString { get { return _FilterString; } }

        public string Name { get { return "TmxWriter"; } }

        private static readonly XNamespace X = XNamespace.Get("http://www.lisa.org/tmx14");

        private static readonly XName LANG = XNamespace.Xml + "lang";

        private static readonly XName SPACE = XNamespace.Xml + "space";

        public void Write(string filename, int filterindex, IEnumerable<IRowData> rows, object write_params)
        {
            var tmx =
                new XElement(X + "tmx",
                    new XAttribute("xmlns", X.NamespaceName),
                    new XAttribute("version", "1.4"),
                    new XElement(X + "header",
                        new XAttribute("creationtool", "disfr"),
                        new XAttribute("creationtoolversion", "1"),
                        new XAttribute("segtype", "block"),
                        new XAttribute("o-tmf", "unknown"),
                        new XAttribute("adminlang", "en"),
                        new XAttribute("srclang", rows.Select(r => r.SourceLang).First(s => s != null)),
                        new XAttribute("datatype", "unknown"),
                        new XAttribute("creationdate", DateTime.UtcNow.ToString(@"yyyyMMdd\THHmmss\Z"))),
                    new XElement(X + "body",
                        rows.Select(ConvertEntry)));
            tmx.Save(filename, SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
        }

        private XElement ConvertEntry(IRowData data)
        {
            return 
                new XElement(X + "tu",
                    new XElement(X + "tuv",
                        new XAttribute(LANG, data.SourceLang),
                        new XElement(X + "seg",
                            new XAttribute(SPACE, "preserve"),
                            data.RawSource.Contents.Select(ConvertContent))),
                    new XElement(X + "tuv",
                        new XAttribute(LANG, data.TargetLang),
                        new XElement(X + "seg",
                            new XAttribute(SPACE, "preserve"),
                            data.RawTarget.Contents.Select(ConvertContent))));
        }

        private object ConvertContent(object item)
        {
            if (item is string)
            {
                return item;
            }
            else if (item is InlineChar)
            {
                return ((InlineChar)item).Char;
            }
            else if (item is InlineTag)
            {
                // Create a TMX <ph> tag for any tag.  
                var tag = item as InlineTag;
                return new XElement(X + "ph",
                    new XAttribute("x", tag.Number),
                    "{" + tag.Number + "}");
            }
            else
            {
                throw new Exception("Internal error");
            }
        }
    }
}
