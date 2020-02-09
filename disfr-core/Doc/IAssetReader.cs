using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    /// <summary>
    /// The interface that represents a reader of a bilingual file.
    /// </summary>
    /// <remarks>
    /// This is one of the two interfaces you must implement when you create a plug-in to support a new file format.
    /// </remarks>
    /// <see cref="disfr.Plugin.IPlugin"/>
    public interface IAssetReader : disfr.Plugin.IReader
    {
        /// <summary>
        /// The name of this asset reader.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Priority of this asset reader in auto-detection.
        /// </summary>
        /// <remarks>
        /// A positive value indicates that this asset reader contributes to auto-detection.
        /// A reader with higher priority will be tried earlier than those with lower.
        /// A value of zero indicates that this asset reader will be used only if explicitly specified.
        /// Any negative value is not allowed.
        /// </remarks>
        int Priority { get; }

        /// <summary>
        /// List of filter strings for OpenFileDialog.
        /// </summary>
        /// <remarks>
        /// A list of strings of form "label|*.ext;*.ext2" as in <see cref="Microsoft.Win32.OpenFileDialog"/>.
        /// A user may choose one of them to indicate a logical file type he/she wants to read.
        /// Note you can only define one label in a single string (i.e., you can only put one "|".)
        /// </remarks>
        /// <see cref="Read(string, int)"/>
        IList<string> FilterString { get; }

        /// <summary>
        /// Tries to read a bilingual file. 
        /// </summary>
        /// <param name="filename">Full path name of a file.</param>
        /// <param name="filterindex">An index in <see cref="FilterString"/> that the user specified, or -1 if he/she didn't specified any.</param>
        /// <returns>A series of assets read from the file, or null if the given file was incompatible to this asset reader.</returns>
        /// <remarks>
        /// If <paramref name="filterindex"/> is 0 or larger, it is an index in <see cref="FilterString"/> to indicate a particular file type.
        /// This method tries to read the specified file type only.
        /// If <paramref name="filterindex"/> is a negative value, this method tries to detect the file format among its supported variations.
        /// </remarks>
        IAssetBundle Read(string filename, int filterindex);
    }
}
