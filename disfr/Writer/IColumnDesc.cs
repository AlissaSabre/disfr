using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using disfr.UI;

namespace disfr.Writer
{
    public interface IColumnDesc
    {
        string Header { get; }
        string Path { get; }

        object GetContent(IRowData row);
    }
}
