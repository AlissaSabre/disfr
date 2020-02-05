using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using disfr.Doc;

namespace disfr.Writer
{
    public class XmlssWriter : TableWriterBase, IPairsWriter
    {
        public string Name { get { return "Xlsx Writer"; } }

        private readonly IList<string> _FilterString = new string[]
        {
            "XML Spreadsheet|*.xml",
        };

        public IList<string> FilterString { get { return _FilterString; } }

        public void Write(string filename, int filterindex, IEnumerable<ITransPair> pairs, IColumnDesc[] columns, InlineString.Render render)
        {
            switch (filterindex)
            {
                case 0: WriteXmlss(filename, pairs, columns, render); break;
                default:
                    throw new ArgumentOutOfRangeException("filterindex");
            }
        }

        public static void WriteXmlss(string filename, IEnumerable<ITransPair> pairs, IColumnDesc[] columns, InlineString.Render render)
        {
            using (var output = File.Create(filename))
            {
                var table = CreateXmlTree(pairs, columns, render);
                Transform(table, output, "xmlss");
            }
        }
    }
}
