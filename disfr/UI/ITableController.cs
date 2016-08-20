using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.UI
{
    /// <summary>
    /// Defines interface that <see cref="TableController"/> provides for <see cref="TableView"/>.
    /// </summary>
    /// <remarks>
    /// See <see cref="TableController"/> for semantics.
    /// </remarks>
    public interface ITableController
    {
        string Name { get; }
        IEnumerable<IRowData> Rows { get; }
        IEnumerable<IRowData> AllRows { get; }
        IEnumerable<AdditionalPropertiesInfo> AdditionalProps { get; }

        ITableController LoadAltAssets();
        bool HasAltAssets { get; }

        Func<IRowData, bool> ContentsFilter { get; set; }
    }
}
