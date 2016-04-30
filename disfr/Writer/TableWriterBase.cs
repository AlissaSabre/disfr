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

namespace disfr.Writer
{
    public abstract class TableWriterBase
    {
        public static readonly XNamespace D = XNamespace.Get("http://github.com/AlissaSabre/disfr/");

        protected static XElement CreateXmlTree(IEnumerable<IRowData> rows, ColumnDesc[] columns)
        {
            return new XElement(D + "Tree",
                new XElement(D + "Columns",
                    columns.Select(c => new XElement(D + "Col",
                        new XAttribute("Path", c.Path),
                        c.Header))),
                rows.Select(r => new XElement(D + "Row",
                    columns.Select(c => new XElement(D + "Data",
                        new XAttribute("Path", c.Path),
                        ConvertContent(r, c))))));
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

            GlossLabel = map;
        }

        protected static IEnumerable<XNode> ConvertContent(IRowData row, ColumnDesc column)
        {
            object content = null;
            if (column.Path.StartsWith("["))
            {
                content = row[column.Path.Substring(1, column.Path.Length - 2)];
            }
            else
            {
                content = typeof(IRowData).GetProperty(column.Path).GetValue(row);
            }

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
            else if (content is GlossyString)
            {
                return (content as GlossyString).AsCollection().Select(p =>
                    new XElement(D + "Span", new XAttribute("Gloss", GlossLabel[(int)p.Gloss]), p.Text));
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
            using (var rd = tree.CreateReader(ReaderOptions.OmitDuplicateNamespaces))
            {
                transform.Transform(rd, null, output);
            }
        }
    }
}
