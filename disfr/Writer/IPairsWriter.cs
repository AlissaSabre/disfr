using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.Doc;

namespace disfr.Writer
{
    public interface IPairsWriter : disfr.Plugin.IWriter
    {
        string Name { get; }

        IList<string> FilterString { get; }

        void Write(string filename, int filterindex, IEnumerable<ITransPair> pairs, IColumnDesc[] columns, InlineString.Render render);
    }
}
