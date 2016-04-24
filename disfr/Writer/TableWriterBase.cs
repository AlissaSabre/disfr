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
                    columns.Select(c => new XElement(D + "Col", c.Header))),
                rows.Select(r => new XElement(D + "Row",
                    columns.Select(c => new XElement(D + "Data",
                        new XAttribute("Path", c.Path),
                        ConvertContent(r, c))))));
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
                    new XElement(D + "Span",
                        p.Gloss == Gloss.None ? null : new XAttribute("Gloss", p.Gloss.ToString()),
                        p.Text));
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
