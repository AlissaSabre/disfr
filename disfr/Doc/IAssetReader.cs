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
        /// Priority of this asset reader in auto-detection.
        /// </summary>
        /// <value>
        /// A positive value indicates that this asset reader contributes to auto-detection.
        /// A reader with higher priority will be tried earlier than those with lower.
        /// A value of zero indicates that this asset reader will be used only if explicitly specified.
        /// Any negative value is not allowed.
        /// </value>
        int Priority { get; }

        /// <summary>
        /// List of filter strings for OpenFileDialog.
        /// </summary>
        /// <value>
        /// A list of strings of form "label|*.ext" as in <see cref="Microsoft.Win32.OpenFileDialog"/>.
        /// Use one string for a logical file type.
        /// </value>
        IList<string> FilterString { get; }

        IEnumerable<IAsset> Read(string filename, int filterindex);
    }
}
