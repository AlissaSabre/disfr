using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

using disfr.Doc;

namespace disfr.Writer
{
    public abstract class TableWriterBase
    {
        public static readonly XNamespace D = XNamespace.Get("http://github.com/AlissaSabre/disfr/");

        protected static XElement CreateXmlTree(IEnumerable<ITransPair> pair, IColumnDesc[] columns, InlineString.Render render_options)
        {
            if (columns == null) columns = DefaultColumnDesc.DefaultDescs;

            // We make ins/del sections glossy only if both sections are shown (i.e., neither is hidden).
            var properties_mask = (render_options & (InlineString.Render.HideIns | InlineString.Render.HideDel)) == 0
                ? ~0
                : ~(int)(InlineProperty.Ins | InlineProperty.Del);

            return new XElement(D + "Tree",
                new XElement(D + "Columns",
                    columns.Select(c => new XElement(D + "Col",
                        new XAttribute("Path", c.Path),
                        c.Header))),
                pair.Select(p => new XElement(D + "Row",
                    columns.Select(c => new XElement(D + "Data",
                        new XAttribute("Path", c.Path),
                        ConvertContent(c.GetContent(p), render_options, properties_mask))))));
        }

        private static readonly string[] GlossLabel;

        private static readonly int GlossLabelShift;

        static TableWriterBase()
        {
            var n = Enum.GetValues(typeof(InlineProperty)).Cast<int>().Aggregate((x, y) => x | y) + 1;
            var map = new string[n * 2];
            map[(int)InlineProperty.None] = "NOR";
            map[(int)InlineProperty.Ins] = "NOR INS";
            map[(int)InlineProperty.Del] = "NOR DEL";
            map[(int)InlineProperty.Emp] = "NOR EMP";
            map[(int)InlineProperty.None + n] = "TAG";
            map[(int)InlineProperty.Ins + n] = "TAG INS";
            map[(int)InlineProperty.Del + n] = "TAG DEL";
            map[(int)InlineProperty.Emp + n] = "TAG EMP";
            GlossLabel = map;
            GlossLabelShift = n;
        }

        protected static IEnumerable<XNode> ConvertContent(object content, InlineString.Render render_options, int properties_mask)
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
            else if (content is InlineString)
            {
                return (content as InlineString).RunsWithProperties.Select(rwp =>
                {
                    var s = rwp.ToString(render_options);
                    if (string.IsNullOrEmpty(s))
                    {
                        return null;
                    }
                    else
                    {
                        return new XElement(D + "Span",
                            new XAttribute("Gloss", GlossLabel[(rwp.Run is InlineText ? 0 : GlossLabelShift) + ((int)rwp.Property & properties_mask)]), 
                            s);
                    }
                });
            }
            else if (content is string[])
            {
                return new[] { new XText(string.Join(Environment.NewLine, content as string[])) };
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
