using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    public interface IAssetReader
    {
        string Name { get; }

        /// <summary>
        /// A positive value to indicate the priority of this asset reader in auto-detection.
        /// </summary>
        /// <value>
        /// A reader with higher priority will be tried earlier than those with lower.  
        /// </value>
        int Priority { get; }

        IList<string> FilterString { get; }

        IEnumerable<IAsset> Read(string filename, int filterindex);
    }
}
