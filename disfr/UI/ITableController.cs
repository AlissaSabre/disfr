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
        /// <summary>
        /// An opaque object used by UI components for their own circumstances.
        /// </summary>
        /// <remarks>
        /// An implementation of <see cref="ITableController"/> only provides a storage.
        /// It never uses it for its own purpose.
        /// </remarks>
        object Tag { get; set; }

        string Name { get; }
        IEnumerable<IRowData> Rows { get; }
        IEnumerable<IRowData> AllRows { get; }
        IEnumerable<AdditionalPropertiesInfo> AdditionalProps { get; }

        ITableController LoadAltAssets();
        bool HasAltAssets { get; }

        Func<IRowData, bool> ContentsFilter { get; set; }
    }
}
