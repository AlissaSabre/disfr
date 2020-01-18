using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

using disfr.UI;
using disfr.Doc;

namespace disfr.Writer
{
    public abstract class TableWriterBase
    {
        public static readonly XNamespace D = XNamespace.Get("http://github.com/AlissaSabre/disfr/");

        protected static XElement CreateXmlTree(IEnumerable<ITransPair> pair, IColumnDesc[] columns)
        {
            return new XElement(D + "Tree",
                new XElement(D + "Columns",
                    columns.Select(c => new XElement(D + "Col",
                        new XAttribute("Path", c.Path),
                        c.Header))),
                pair.Select(p => new XElement(D + "Row",
                    columns.Select(c => new XElement(D + "Data",
                        new XAttribute("Path", c.Path),
                        ConvertContent(c.GetContent(p)))))));
        }

        private static readonly string[] GlossLabel;

        static TableWriterBase()
        {
            var map = new string[Enum.GetValues(typeof(Gloss)).Cast<int>().Aggregate((x, y) => x | y) + 1];

            map[(int)(Gloss.NOR | Gloss.COM)] = "NOR";
            map[(int)(Gloss.NOR | Gloss.INS)] = "NOR INS";
            map[(int)(Gloss.NOR | Gloss.DEL)] = "NOR DEL";
            map[(int)(Gloss.TAG | Gloss.COM)] = "TAG";
            map[(int)(Gloss.TAG | Gloss.INS)] = "TAG INS";
            map[(int)(Gloss.TAG | Gloss.DEL)] = "TAG DEL";
            map[(int)(Gloss.SYM | Gloss.COM)] = "SYM";
            map[(int)(Gloss.SYM | Gloss.INS)] = "SYM INS";
            map[(int)(Gloss.SYM | Gloss.DEL)] = "SYM DEL";
            map[(int)(Gloss.ALT | Gloss.COM)] = "ALT";
            map[(int)(Gloss.ALT | Gloss.INS)] = "ALT INS";
            map[(int)(Gloss.ALT | Gloss.DEL)] = "ALT DEL";

            GlossLabel = map;
        }

        protected static IEnumerable<XNode> ConvertContent(object content)
        {
            if (content == null)
            {
                return null;
            }
            else if (content is int)
            {
                return new [] { new XText(content.ToString()) };
            }
            else if (content is string)
            {
                return new [] { new XText(content as string) };
            }
            //else if (content is GlossyString)
            //{
            //    return (content as GlossyString).AsCollection().Select(p =>
            //        new XElement(D + "Span", new XAttribute("Gloss", GlossLabel[(int)p.Gloss]), p.Text));
            //}
            else if (content is InlineString)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new Exception("Internal error");
            }
        }

        protected static void Transform(XElement tree, Stream output, string transform_name, string folder = null)
        {
            if (folder == null)
            {
                folder = Path.GetDirectoryName(typeof(TableWriterBase).Assembly.Location);
            }
            var transform = new XslCompiledTransform();
            transform.Load(Path.Combine(folder, transform_name + ".xslt"));
            using (var wr = new SpaceSensitiveXmlTextWriter(output, transform.OutputSettings))
            {
                using (var rd = tree.CreateReader(ReaderOptions.OmitDuplicateNamespaces))
                {
                    transform.Transform(rd, null, wr);
                }
            }
        }
    }
}
