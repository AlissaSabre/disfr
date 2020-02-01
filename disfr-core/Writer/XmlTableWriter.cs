using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Doc;

namespace disfr.Writer
{
    public class XmlTableWriter : TableWriterBase, IPairsWriter
    {
        public string Name { get { return "XML Table Writer"; } }

        private readonly IList<string> _FilterString = new string[]
        {
            "XML Table Data|*.xml",
        };

        public IList<string> FilterString { get { return _FilterString; } }

        public void Write(string filename, int filterindex, IEnumerable<ITransPair> pairs, IColumnDesc[] columns, InlineString.Render render)
        {
            var table = CreateXmlTree(pairs, columns, render);
            using (var output = File.Create(filename))
            {
                Transform(table, output, "table");
            }
        }
    }

    public class XmlDebugTreeWriter : TableWriterBase, IPairsWriter
    {
        public string Name { get { return "Debug Tree Writer"; } }

        private readonly IList<string> _FilterString = new string[]
        {
            "debug tree for developers|*.tree",
        };

        public IList<string> FilterString { get { return _FilterString; } }

        public void Write(string filename, int filterindex, IEnumerable<ITransPair> pairs, IColumnDesc[] columns, InlineString.Render render)
        {
            CreateXmlTree(pairs, columns, render).Save(filename);
        }
    }
}
