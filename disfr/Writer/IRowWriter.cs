using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.UI;

namespace disfr.Writer
{
    public interface IRowsWriter
    {
        string Name { get; }

        IList<string> FilterString { get; }

        void Write(string filename, int filterindex, IEnumerable<IRowData> rows, ColumnDesc[] write_params);
    }
}
