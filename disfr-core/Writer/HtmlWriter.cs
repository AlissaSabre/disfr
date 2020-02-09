using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Doc;

namespace disfr.Writer
{
    public class HtmlWriter : TableWriterBase, IPairsWriter
    {
        public string Name { get { return "HTML Writer"; } }

        private readonly IList<string> _FilterString = new string[]
        {
            "HTML5 Table|*.html",
        };

        public IList<string> FilterString { get { return _FilterString; } }

        public void Write(string filename, int filterindex, IEnumerable<ITransPair> pairs, IColumnDesc[] columns, InlineString.Render render)
        {
            var table = CreateXmlTree(pairs, columns, render);
            using (var output = File.Create(filename))
            {
                Transform(table, output, "html5");
            }
        }
    }
}
