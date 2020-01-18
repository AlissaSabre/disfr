using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.UI;
using disfr.Doc;

namespace disfr.Writer
{
    public interface IRowsWriter : disfr.Plugin.IWriter
    {
        string Name { get; }

        IList<string> FilterString { get; }

        void Write(string filename, int filterindex, IEnumerable<ITransPair> rows, IColumnDesc[] columns);
    }
}
