using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using disfr.UI;

namespace disfr.Writer
{
    public class XmlTableWriter : TableWriterBase, IRowsWriter
    {
        public string Name { get { return "XML Table Writer"; } }

        private readonly IList<string> _FilterString = new string[]
        {
            "XML Table Data|*.xml",
        };

        public IList<string> FilterString { get { return _FilterString; } }

        public void Write(string filename, int filterindex, IEnumerable<IRowData> rows, ColumnDesc[] columns)
        {
            var table = CreateXmlTree(rows, columns);
            using (var output = File.Create(filename))
            {
                Transform(table, output, "table");
            }
        }
    }

    public class XmlDebugTreeWriter : TableWriterBase, IRowsWriter
    {
        public string Name { get { return "Debug Tree Writer"; } }

        private readonly IList<string> _FilterString = new string[]
        {
            "Debug Tree |*.tree",
        };

        public IList<string> FilterString { get { return _FilterString; } }

        public void Write(string filename, int filterindex, IEnumerable<IRowData> rows, ColumnDesc[] columns)
        {
            CreateXmlTree(rows, columns).Save(filename);
        }
    }
}
