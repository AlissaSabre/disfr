using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disfr.Doc
{
    public interface IAsset
    {
        /// <summary>
        /// Local identifier of a pckage in which this asset is read from. 
        /// </summary>
        /// <remarks>
        /// This is usually a local full path name of the package file.
        /// </remarks>
        string Package { get; }

        /// <summary>
        /// The identifier of this asset as in the originating environment.
        /// </summary>
        /// <remarks>
        /// It is the value of /xliff/file/@original if this asset is from a standard XLIFF 1.2 file.
        /// </remarks>
        string Original { get; }
        
        string SourceLang { get; }
        
        string TargetLang { get; }

        IEnumerable<ITransPair> TransPairs { get; }

        IEnumerable<ITransPair> AltPairs { get; }
    }
}
